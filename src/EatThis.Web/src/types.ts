export interface SearchRequest {
  latitude: number
  longitude: number
  radiusMeters: number
}

export interface PlaceResult {
  name: string
  address: string
  latitude: number
  longitude: number
  distanceMeters: number
  navigationUrl: string
  provider: string
}

export interface ApiErrorPayload {
  errorCode: string
  message: string
  retryAfterSeconds?: number | null
}

export type NearbyFoodState =
  | 'idle'
  | 'locating'
  | 'searching'
  | 'selected'
  | 'permission-denied'
  | 'unsupported-geolocation'
  | 'no-results'
  | 'provider-error'
  | 'rate-limited'
