export interface HistoryOptions {
  storage?: () => Pick<Storage, 'getItem' | 'setItem'>
  now?: () => number
}

export interface HistoryRequest {
  excludedNavigationUrls?: string[]
  lastNavigationUrl?: string
}

const STORAGE_KEY = 'eatthis.recommendation-history.v1'
const LIFETIME_MS = 30 * 60 * 1000
const MAX_ENTRIES = 1000
const MAX_KEY_LENGTH = 2048

interface Entry { navigationUrl: string; shownAt: number }
interface History { version: 1; entries: Entry[]; lastShown: Entry | null }

const emptyHistory = (): History => ({ version: 1, entries: [], lastShown: null })
const key = (url: string) => url.toUpperCase()

function validEntry(value: unknown, now: number): value is Entry {
  if (typeof value !== 'object' || value === null) return false
  const entry = value as Partial<Entry>
  return typeof entry.navigationUrl === 'string' && entry.navigationUrl.trim().length > 0 &&
    entry.navigationUrl.length <= MAX_KEY_LENGTH && typeof entry.shownAt === 'number' &&
    Number.isFinite(entry.shownAt) && entry.shownAt <= now && now - entry.shownAt < LIFETIME_MS
}

function normalize(value: unknown, now: number): History {
  if (typeof value !== 'object' || value === null) return emptyHistory()
  const history = value as Partial<History>
  if (history.version !== 1 || !Array.isArray(history.entries)) return emptyHistory()
  const entries = new Map<string, Entry>()
  for (const entry of history.entries.filter(entry => validEntry(entry, now)).sort((a, b) => a.shownAt - b.shownAt)) {
    entries.delete(key(entry.navigationUrl))
    entries.set(key(entry.navigationUrl), entry)
  }
  return {
    version: 1,
    entries: [...entries.values()].slice(-MAX_ENTRIES),
    lastShown: validEntry(history.lastShown, now) ? history.lastShown : null,
  }
}

export function createRecommendationHistory(options: HistoryOptions = {}) {
  const storage = options.storage ?? (() => window.localStorage)
  const now = options.now ?? Date.now
  let memory = emptyHistory()
  let memoryOnly = false

  function read(): History {
    if (!memoryOnly) {
      try {
        const raw = storage().getItem(STORAGE_KEY)
        try { memory = normalize(raw === null ? null : JSON.parse(raw), now()) }
        catch { memory = emptyHistory() }
      } catch {
        memoryOnly = true
      }
    }
    memory = normalize(memory, now())
    return memory
  }

  function request(): HistoryRequest {
    const history = read()
    return {
      ...(history.entries.length ? { excludedNavigationUrls: history.entries.map(entry => entry.navigationUrl) } : {}),
      ...(history.lastShown ? { lastNavigationUrl: history.lastShown.navigationUrl } : {}),
    }
  }

  function record(navigationUrl: string, resetNavigationUrls: string[] = []): void {
    const shownAt = now()
    const entry = { navigationUrl, shownAt }
    if (!validEntry(entry, shownAt)) return
    const history = read()
    const reset = new Set(resetNavigationUrls.map(key))
    reset.add(key(navigationUrl))
    memory = normalize({ version: 1,
      entries: [...history.entries.filter(item => !reset.has(key(item.navigationUrl))), entry],
      lastShown: entry,
    }, shownAt)
    if (!memoryOnly) {
      try { storage().setItem(STORAGE_KEY, JSON.stringify(memory)) }
      catch { memoryOnly = true }
    }
  }

  return { request, record }
}
