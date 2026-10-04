import type { RestaurantCategory } from './restaurantCategories'

export interface SearchRequest {
  latitude: number
  longitude: number
  radiusMeters: number
  minRating?: number | null
  restaurantCategory?: RestaurantCategory | null
  excludedNavigationUrls?: string[] | null
  lastNavigationUrl?: string | null
}

export interface PlaceResult {
  name: string
  address: string
  latitude: number
  longitude: number
  distanceMeters: number
  navigationUrl: string
  provider: string
  rating?: number | null
  userRatingCount?: number | null
  resetNavigationUrls?: string[]
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
