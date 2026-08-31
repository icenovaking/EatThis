import { computed, ref } from 'vue'
import { ApiRequestError, pickNearbyFood as defaultPickNearbyFood } from '../api/nearbyFoodApi'
import { LocationError, requestCurrentLocation, type GeolocationPort } from '../geolocation'
import type { NearbyFoodState, PlaceResult, SearchRequest } from '../types'

export type PickNearbyFood = (request: SearchRequest) => Promise<PlaceResult>

export interface NearbyFoodOptions {
  geolocation?: GeolocationPort
  pick?: PickNearbyFood
}

export interface FlowError {
  code: string
  message: string
  retryAfterSeconds?: number
}

const defaultErrorMessage = '附近地點服務暫時無法使用，請稍後再試。'
export const MINIMUM_RADIUS_METERS = 100
export const MAXIMUM_RADIUS_METERS = 3000
export const RADIUS_STEP_METERS = 100

function isValidRadius(radiusMeters: number): boolean {
  return Number.isInteger(radiusMeters) &&
    radiusMeters >= MINIMUM_RADIUS_METERS &&
    radiusMeters <= MAXIMUM_RADIUS_METERS
}

function invalidRadiusError(): FlowError {
  return {
    code: 'invalid_request',
    message: '搜尋範圍必須介於 100 公尺與 3 公里之間。',
  }
}

export function useNearbyFood(options: NearbyFoodOptions = {}) {
  const state = ref<NearbyFoodState>('idle')
  const place = ref<PlaceResult | null>(null)
  const error = ref<FlowError | null>(null)
  const selectedRadiusMeters = ref(MINIMUM_RADIUS_METERS)
  const currentLocation = ref<{ latitude: number; longitude: number } | null>(null)
  const isBusy = computed(() => state.value === 'locating' || state.value === 'searching')
  const pick = options.pick ?? defaultPickNearbyFood

  async function recommend(): Promise<void> {
    if (isBusy.value) {
      return
    }

    place.value = null
    error.value = null

    const radiusMeters = selectedRadiusMeters.value
    if (!isValidRadius(radiusMeters)) {
      error.value = invalidRadiusError()
      state.value = 'provider-error'
      return
    }

    state.value = 'locating'

    try {
      currentLocation.value = await requestCurrentLocation(options.geolocation)
    } catch (caught) {
      handleLocationError(caught)
      return
    }

    await search(radiusMeters)
  }

  async function search(radiusMeters: number): Promise<void> {
    const location = currentLocation.value
    if (!location || !isValidRadius(radiusMeters)) {
      error.value = invalidRadiusError()
      state.value = 'provider-error'
      return
    }

    state.value = 'searching'
    error.value = null

    try {
      place.value = await pick({ ...location, radiusMeters })
      state.value = 'selected'
    } catch (caught) {
      handleApiError(caught)
    }
  }

  function handleLocationError(caught: unknown): void {
    if (caught instanceof LocationError) {
      state.value = caught.kind === 'permission-denied'
        ? 'permission-denied'
        : caught.kind === 'unsupported-geolocation'
          ? 'unsupported-geolocation'
          : 'provider-error'
      error.value = {
        code: caught.kind,
        message: caught.kind === 'permission-denied'
          ? '需要允許定位，才能搜尋你附近的餐飲地點。'
          : caught.message,
      }
      return
    }

    state.value = 'provider-error'
    error.value = { code: 'location_error', message: defaultErrorMessage }
  }

  function handleApiError(caught: unknown): void {
    if (caught instanceof ApiRequestError) {
      if (caught.status === 404 || caught.errorCode === 'no_results') {
        state.value = 'no-results'
      } else if (caught.status === 429 || caught.errorCode === 'rate_limited') {
        state.value = 'rate-limited'
      } else {
        state.value = 'provider-error'
      }

      error.value = {
        code: caught.errorCode,
        message: caught.message || defaultErrorMessage,
        retryAfterSeconds: caught.retryAfterSeconds,
      }
      return
    }

    if (caught instanceof Error && caught.message === 'no_results') {
      state.value = 'no-results'
      error.value = { code: 'no_results', message: '目前搜尋範圍內沒有找到適合的地點。' }
      return
    }

    state.value = 'provider-error'
    error.value = { code: 'provider_unavailable', message: defaultErrorMessage }
  }

  return {
    state,
    place,
    error,
    selectedRadiusMeters,
    isBusy,
    recommend,
  }
}
