import { readFileSync } from 'node:fs'
import { resolve } from 'node:path'
import { describe, expect, it, vi } from 'vitest'
import { ApiRequestError, pickNearbyFood } from './nearbyFoodApi'
import type { PlaceResult, SearchRequest } from '../types'

const request: SearchRequest = {
  latitude: 25.0330,
  longitude: 121.5654,
  radiusMeters: 3000,
}

const result = JSON.parse(
  readFileSync(resolve(process.cwd(), '../../tests/fixtures/nearby-food-success.json'), 'utf8'),
) as PlaceResult

describe('pickNearbyFood', () => {
  it.each([4.5, null])('serializes optional minimum rating %s without provider options', async (minRating) => {
    const fetcher = vi.fn().mockResolvedValue({ ok: true, json: async () => ({ ...result, rating: 4.7 }) } as Response)
    const response = await pickNearbyFood({ ...request, minRating }, fetcher)
    const sentBody = JSON.parse(fetcher.mock.calls[0]![1].body)
    expect(sentBody).toEqual({ latitude: 25.033, longitude: 121.5654, radiusMeters: 3000, minRating })
    expect(response.rating).toBe(4.7)
  })

  it('sends only location and bounded radius to the EatThis API', async () => {
    const fetcher = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => result,
    } as Response)

    await expect(pickNearbyFood(request, fetcher)).resolves.toEqual(result)

    expect(fetcher).toHaveBeenCalledWith(
      '/api/nearby-food/pick',
      expect.objectContaining({
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request),
      }),
    )
    const body = JSON.stringify(request)
    expect(body).not.toContain('ApiKey')
    expect(body).not.toContain('X-Goog-Api-Key')
  })

  it('turns a stable API error response into a typed client error', async () => {
    const fetcher = vi.fn().mockResolvedValue({
      ok: false,
      status: 429,
      json: async () => ({
        errorCode: 'rate_limited',
        message: '請稍候再試。',
        retryAfterSeconds: 60,
      }),
    } as Response)

    const error = await pickNearbyFood(request, fetcher).catch(caught => caught)

    expect(error).toBeInstanceOf(ApiRequestError)
    expect(error).toMatchObject({
      status: 429,
      errorCode: 'rate_limited',
      retryAfterSeconds: 60,
    })
  })
})
