import type { ApiErrorPayload, PlaceResult, SearchRequest } from '../types'

export class ApiRequestError extends Error {
  constructor(
    public readonly status: number,
    public readonly errorCode: string,
    message: string,
    public readonly retryAfterSeconds?: number,
  ) {
    super(message)
    this.name = 'ApiRequestError'
  }
}

export type Fetcher = typeof fetch

export async function pickNearbyFood(
  request: SearchRequest,
  fetcher: Fetcher = fetch,
): Promise<PlaceResult> {
  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? ''
  const response = await fetcher(`${apiBaseUrl}/api/nearby-food/pick`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  })

  if (!response.ok) {
    let payload: Partial<ApiErrorPayload> = {}
    try {
      payload = await response.json() as ApiErrorPayload
    } catch {
      // Keep a stable client error even when a gateway returns no JSON body.
    }

    throw new ApiRequestError(
      response.status,
      payload.errorCode ?? 'provider_unavailable',
      payload.message ?? '附近地點服務暫時無法使用，請稍後再試。',
      payload.retryAfterSeconds ?? undefined,
    )
  }

  return await response.json() as PlaceResult
}
