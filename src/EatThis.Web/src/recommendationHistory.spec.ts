import { describe, expect, it } from 'vitest'
import { createRecommendationHistory } from './recommendationHistory'

function fixture(initial: unknown = null) {
  let value = initial === null ? null : JSON.stringify(initial)
  let now = Date.parse('2026-10-02T12:00:00Z')
  const storage = { getItem: () => value, setItem: (_key: string, next: string) => { value = next } }
  return { storage, now: () => now, advance: (ms: number) => { now += ms }, saved: () => JSON.parse(value!) }
}

describe('recommendation history', () => {
  it('expires at thirty minutes, not one millisecond before', () => {
    const f = fixture()
    const history = createRecommendationHistory({ storage: () => f.storage, now: f.now })
    history.record('A')
    f.advance(1799999)
    expect(history.request()).toEqual({ excludedNavigationUrls: ['A'], lastNavigationUrl: 'A' })
    f.advance(1)
    expect(history.request()).toEqual({})
  })

  it('persists across instances and resets only current candidates before adding the result', () => {
    const f = fixture()
    const history = createRecommendationHistory({ storage: () => f.storage, now: f.now })
    history.record('A'); history.record('B'); history.record('D')
    const reloaded = createRecommendationHistory({ storage: () => f.storage, now: f.now })
    reloaded.record('B', ['a', 'b'])
    expect(reloaded.request()).toEqual({ excludedNavigationUrls: ['D', 'B'], lastNavigationUrl: 'B' })
    expect(f.saved().entries).toEqual([{ navigationUrl: 'D', shownAt: f.now() }, { navigationUrl: 'B', shownAt: f.now() }])
  })

  it('deduplicates case-insensitively using the latest display time', () => {
    const f = fixture()
    const history = createRecommendationHistory({ storage: () => f.storage, now: f.now })
    history.record('A'); f.advance(100); history.record('a')
    expect(f.saved().entries).toEqual([{ navigationUrl: 'a', shownAt: f.now() }])
  })

  it.each([{}, { version: 2, entries: [] }, { version: 1, entries: 'bad' }])('ignores invalid storage %j', initial => {
    const f = fixture(initial)
    expect(createRecommendationHistory({ storage: () => f.storage, now: f.now }).request()).toEqual({})
  })

  it('discards future, invalid, expired and oversized entries', () => {
    const f = fixture({ version: 1, entries: [
      { navigationUrl: 'future', shownAt: Date.parse('2026-10-02T12:00:01Z') },
      { navigationUrl: 'old', shownAt: Date.parse('2026-10-02T11:30:00Z') },
      { navigationUrl: 'bad', shownAt: 'today' },
      { navigationUrl: 'x'.repeat(2049), shownAt: Date.parse('2026-10-02T12:00:00Z') },
      { navigationUrl: ' ', shownAt: Date.parse('2026-10-02T12:00:00Z') },
      { navigationUrl: 'valid', shownAt: Date.parse('2026-10-02T12:00:00Z') },
    ], lastShown: null })
    expect(createRecommendationHistory({ storage: () => f.storage, now: f.now }).request()).toEqual({ excludedNavigationUrls: ['valid'] })
  })

  it('refreshes history for sequential operations in two instances', () => {
    const f = fixture()
    const first = createRecommendationHistory({ storage: () => f.storage, now: f.now })
    const second = createRecommendationHistory({ storage: () => f.storage, now: f.now })
    expect(second.request()).toEqual({})
    first.record('A')
    expect(second.request()).toEqual({ excludedNavigationUrls: ['A'], lastNavigationUrl: 'A' })
    second.record('B')
    expect(first.request()).toEqual({ excludedNavigationUrls: ['A', 'B'], lastNavigationUrl: 'B' })
  })

  it('accepts a 2048-character key and ignores invalid displayed keys', () => {
    const f = fixture()
    const history = createRecommendationHistory({ storage: () => f.storage, now: f.now })
    const boundaryKey = 'x'.repeat(2048)
    history.record(boundaryKey)
    history.record('x'.repeat(2049))
    history.record(' ')
    expect(history.request()).toEqual({ excludedNavigationUrls: [boundaryKey], lastNavigationUrl: boundaryKey })
  })

  it('keeps the newest thousand entries', () => {
    const f = fixture()
    const history = createRecommendationHistory({ storage: () => f.storage, now: f.now })
    for (let i = 0; i <= 1000; i++) { history.record(String(i)); f.advance(1) }
    const request = history.request()
    expect(request.excludedNavigationUrls).toHaveLength(1000)
    expect(request.excludedNavigationUrls).not.toContain('0')
    expect(request.excludedNavigationUrls).toContain('1')
    expect(request.lastNavigationUrl).toBe('1000')
  })

  it('retains page memory when storage writes fail despite readable stale data', () => {
    const f = fixture()
    const storage = { getItem: () => null, setItem: () => { throw new Error('quota') } }
    const history = createRecommendationHistory({ storage: () => storage, now: f.now })
    history.record('A'); history.record('B')
    expect(history.request()).toEqual({ excludedNavigationUrls: ['A', 'B'], lastNavigationUrl: 'B' })
  })

  it('uses memory when storage access throws and tolerates malformed JSON', () => {
    const history = createRecommendationHistory({ storage: () => { throw new Error('denied') } })
    history.record('A')
    expect(history.request().excludedNavigationUrls).toEqual(['A'])
    const corrupt = createRecommendationHistory({ storage: () => ({ getItem: () => '{', setItem: () => {} }) })
    expect(corrupt.request()).toEqual({})
  })
})
