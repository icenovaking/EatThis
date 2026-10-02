import { describe, expect, it, vi } from 'vitest'
import { useNearbyFood } from './useNearbyFood'
import type { PlaceResult } from '../types'

const selectedPlace: PlaceResult = {
  name: 'Example Food Shop',
  address: 'Taipei City',
  latitude: 25.0331,
  longitude: 121.5655,
  distanceMeters: 420,
  navigationUrl: 'https://www.google.com/maps/place/example',
  provider: 'google',
}

function createGeolocation(
  behavior: 'success' | 'permission-denied' | 'unsupported' = 'success',
) {
  if (behavior === 'unsupported') {
    return undefined
  }

  return {
    getCurrentPosition: vi.fn((success: PositionCallback, error?: PositionErrorCallback) => {
      if (behavior === 'permission-denied') {
        error?.({ code: 1, message: 'permission denied' } as GeolocationPositionError)
        return
      }

      success({
        coords: {
          latitude: 25.0330,
          longitude: 121.5654,
          accuracy: 10,
          altitude: null,
          altitudeAccuracy: null,
          heading: null,
          speed: null,
        },
        timestamp: Date.now(),
      } as GeolocationPosition)
    }),
  }
}

describe('useNearbyFood', () => {
  it('keeps rating changes pending and submits null for unrestricted searches', async () => {
    const geolocation = createGeolocation()
    const pick = vi.fn().mockResolvedValue(selectedPlace)
    const flow = useNearbyFood({ geolocation, pick })
    expect(flow.selectedMinRating.value).toBeNull()
    flow.selectedMinRating.value = 4.5
    flow.selectedMinRating.value = null
    expect(pick).not.toHaveBeenCalled()
    expect(geolocation?.getCurrentPosition).not.toHaveBeenCalled()
    await flow.recommend()
    expect(pick).toHaveBeenCalledExactlyOnceWith({ latitude: 25.033, longitude: 121.5654, radiusMeters: 100, minRating: null, restaurantCategory: null })
  })

  it('snapshots both conditions before asynchronous location and prevents duplicate actions', async () => {
    let resolveLocation!: PositionCallback
    const geolocation = { getCurrentPosition: vi.fn((success: PositionCallback) => { resolveLocation = success }) }
    const pick = vi.fn().mockResolvedValue(selectedPlace)
    const flow = useNearbyFood({ geolocation, pick })
    flow.selectedRadiusMeters.value = 700
    flow.selectedMinRating.value = 4.5
    const first = flow.recommend()
    flow.selectedRadiusMeters.value = 1000
    flow.selectedMinRating.value = 3.5
    await flow.recommend()
    resolveLocation({ coords: { latitude: 25.033, longitude: 121.5654 } } as GeolocationPosition)
    await first
    expect(pick).toHaveBeenCalledExactlyOnceWith({ latitude: 25.033, longitude: 121.5654, radiusMeters: 700, minRating: 4.5, restaurantCategory: null })
    expect(geolocation.getCurrentPosition).toHaveBeenCalledTimes(1)
    expect(flow.submittedConditions.value).toEqual({ radiusMeters: 700, minRating: 4.5, restaurantCategory: null })
    expect(flow.selectedRadiusMeters.value).toBe(1000)
    expect(flow.selectedMinRating.value).toBe(3.5)
  })

  it.each([0.5, 1, 1.5, 2, 2.5, 3, 3.5, 4, 4.5, 5])('accepts half-star threshold %s', async (minRating) => {
    const pick = vi.fn().mockResolvedValue(selectedPlace)
    const flow = useNearbyFood({ geolocation: createGeolocation(), pick })
    flow.selectedMinRating.value = minRating
    await flow.recommend()
    expect(pick).toHaveBeenCalledExactlyOnceWith({ latitude: 25.033, longitude: 121.5654, radiusMeters: 100, minRating, restaurantCategory: null })
  })

  it.each([0, -0.5, 5.5, 4.3, NaN, Infinity, '4.5', true])('rejects invalid minimum rating %s before locating', async (value) => {
    const geolocation = createGeolocation()
    const pick = vi.fn()
    const flow = useNearbyFood({ geolocation, pick })
    flow.selectedMinRating.value = value as number
    await flow.recommend()
    expect(geolocation?.getCurrentPosition).not.toHaveBeenCalled()
    expect(pick).not.toHaveBeenCalled()
    expect(flow.error.value?.code).toBe('invalid_request')
    expect(flow.error.value?.message).toContain('0.5')
    expect(flow.state.value).toBe('provider-error')
  })

  it('starts at 100 metres and submits the default radius after one location request', async () => {
    const geolocation = createGeolocation()
    const pick = vi.fn().mockResolvedValue(selectedPlace)
    const flow = useNearbyFood({ geolocation, pick })

    expect(flow.selectedRadiusMeters.value).toBe(100)

    await flow.recommend()

    expect(geolocation?.getCurrentPosition).toHaveBeenCalledTimes(1)
    expect(pick).toHaveBeenCalledWith({
      latitude: 25.0330,
      longitude: 121.5654,
      radiusMeters: 100,
      minRating: null, restaurantCategory: null,
    })
    expect(flow.state.value).toBe('selected')
    expect(flow.place.value).toEqual(selectedPlace)
  })

  it('updates the pending radius without locating or searching until recommend is activated', async () => {
    const geolocation = createGeolocation()
    const pick = vi.fn().mockResolvedValue(selectedPlace)
    const flow = useNearbyFood({ geolocation, pick })

    flow.selectedRadiusMeters.value = 700

    expect(flow.selectedRadiusMeters.value).toBe(700)
    expect(geolocation?.getCurrentPosition).not.toHaveBeenCalled()
    expect(pick).not.toHaveBeenCalled()

    await flow.recommend()

    expect(geolocation?.getCurrentPosition).toHaveBeenCalledTimes(1)
    expect(pick).toHaveBeenCalledWith({
      latitude: 25.0330,
      longitude: 121.5654,
      radiusMeters: 700,
      minRating: null, restaurantCategory: null,
    })
  })

  it('submits the three-kilometre maximum without exposing an expanded retry', async () => {
    const geolocation = createGeolocation()
    const pick = vi.fn().mockResolvedValue(selectedPlace)
    const flow = useNearbyFood({ geolocation, pick })

    flow.selectedRadiusMeters.value = 3000
    await flow.recommend()

    expect(pick).toHaveBeenCalledWith(expect.objectContaining({ radiusMeters: 3000 }))
    expect('retryExpandedRadius' in flow).toBe(false)
    expect('canRetryExpandedRadius' in flow).toBe(false)
  })

  it.each([99, 3001])(
    'rejects pending radius %i outside 100 metres through 3 kilometres before locating',
    async (radiusMeters) => {
      const geolocation = createGeolocation()
      const pick = vi.fn()
      const flow = useNearbyFood({ geolocation, pick })

      flow.selectedRadiusMeters.value = radiusMeters
      await flow.recommend()

      expect(geolocation?.getCurrentPosition).not.toHaveBeenCalled()
      expect(pick).not.toHaveBeenCalled()
      expect(flow.state.value).toBe('provider-error')
      expect(flow.error.value?.message).toContain('100 公尺')
      expect(flow.error.value?.message).toContain('3 公里')
    },
  )

  it('does not call the API when location permission is denied', async () => {
    const geolocation = createGeolocation('permission-denied')
    const pick = vi.fn()
    const flow = useNearbyFood({ geolocation, pick })

    await flow.recommend()

    expect(pick).not.toHaveBeenCalled()
    expect(flow.state.value).toBe('permission-denied')
  })

  it('uses an unsupported state when browser geolocation is unavailable', async () => {
    const pick = vi.fn()
    const flow = useNearbyFood({ geolocation: createGeolocation('unsupported'), pick })

    await flow.recommend()

    expect(pick).not.toHaveBeenCalled()
    expect(flow.state.value).toBe('unsupported-geolocation')
  })

  it('prevents duplicate submissions while the backend search is pending', async () => {
    const geolocation = createGeolocation()
    let resolvePick: ((place: PlaceResult) => void) | undefined
    const pick = vi.fn().mockImplementation(
      () => new Promise<PlaceResult>((resolve) => { resolvePick = resolve }),
    )
    const flow = useNearbyFood({ geolocation, pick })

    const first = flow.recommend()
    const second = flow.recommend()
    await vi.waitFor(() => expect(pick).toHaveBeenCalledTimes(1))
    resolvePick?.(selectedPlace)
    await Promise.all([first, second])

    expect(geolocation?.getCurrentPosition).toHaveBeenCalledTimes(1)
    expect(pick).toHaveBeenCalledTimes(1)
  })

  it('keeps the searched radius after no results without retrying automatically', async () => {
    const geolocation = createGeolocation()
    const pick = vi.fn().mockRejectedValue(new Error('no_results'))
    const flow = useNearbyFood({ geolocation, pick })

    flow.selectedRadiusMeters.value = 700
    await flow.recommend()

    expect(flow.state.value).toBe('no-results')
    expect(flow.error.value?.message).toBe('本次未找到符合搜尋條件的餐飲地點。')
    expect(flow.selectedRadiusMeters.value).toBe(700)
    expect(pick).toHaveBeenCalledTimes(1)
    expect(pick).toHaveBeenCalledWith({
      latitude: 25.0330,
      longitude: 121.5654,
      radiusMeters: 700,
      minRating: null, restaurantCategory: null,
    })
    expect(geolocation?.getCurrentPosition).toHaveBeenCalledTimes(1)
  })
})

describe('restaurant category flow', () => {
  it('keeps category changes pending and resets to unrestricted without side effects', async () => {
    const geolocation = createGeolocation()
    const pick = vi.fn().mockResolvedValue(selectedPlace)
    const flow = useNearbyFood({ geolocation, pick })
    expect(flow.selectedRestaurantCategory.value).toBeNull()
    flow.selectedRestaurantCategory.value = 'japanese'
    flow.selectedRestaurantCategory.value = null
    expect(geolocation?.getCurrentPosition).not.toHaveBeenCalled()
    expect(pick).not.toHaveBeenCalled()
    await flow.recommend()
    expect(pick).toHaveBeenCalledExactlyOnceWith({ latitude: 25.033, longitude: 121.5654, radiusMeters: 100, minRating: null, restaurantCategory: null })
  })

  it.each(['taiwanese-chinese', 'japanese', 'korean', 'hot-pot', 'barbecue', 'italian', 'breakfast-brunch', 'fast-food', 'vegetarian', 'cafe-dessert'] as const)('submits product category %s in one search', async (restaurantCategory) => {
    const geolocation = createGeolocation()
    const pick = vi.fn().mockResolvedValue(selectedPlace)
    const flow = useNearbyFood({ geolocation, pick })
    flow.selectedRestaurantCategory.value = restaurantCategory
    await flow.recommend()
    expect(pick).toHaveBeenCalledExactlyOnceWith({ latitude: 25.033, longitude: 121.5654, radiusMeters: 100, minRating: null, restaurantCategory })
    expect(geolocation?.getCurrentPosition).toHaveBeenCalledTimes(1)
    expect(flow.state.value).toBe('selected')
  })

  it.each(['', 'all', 'Japanese', ' japanese ', 'restaurant', 'pizza_restaurant', 123, true, [], {}, undefined])('rejects invalid pending category %j before locating', async (value) => {
    const geolocation = createGeolocation()
    const pick = vi.fn()
    const flow = useNearbyFood({ geolocation, pick })
    flow.selectedRestaurantCategory.value = value as never
    await flow.recommend()
    expect(geolocation?.getCurrentPosition).not.toHaveBeenCalled()
    expect(pick).not.toHaveBeenCalled()
    expect(flow.state.value).toBe('provider-error')
    expect(flow.error.value?.code).toBe('invalid_request')
    expect(flow.error.value?.message).toContain('餐廳類型')
  })

  it('snapshots category before locating and uses later pending category on the next explicit action', async () => {
    let resolveLocation!: PositionCallback
    const geolocation = { getCurrentPosition: vi.fn((success: PositionCallback) => { resolveLocation = success }) }
    const pick = vi.fn().mockResolvedValue(selectedPlace)
    const flow = useNearbyFood({ geolocation, pick })
    flow.selectedRadiusMeters.value = 1000
    flow.selectedMinRating.value = 4
    flow.selectedRestaurantCategory.value = 'japanese'
    const first = flow.recommend()
    expect(flow.state.value).toBe('locating')
    flow.selectedRestaurantCategory.value = 'hot-pot'
    await flow.recommend()
    resolveLocation({ coords: { latitude: 25.033, longitude: 121.5654 } } as GeolocationPosition)
    await first
    expect(pick).toHaveBeenCalledExactlyOnceWith({ latitude: 25.033, longitude: 121.5654, radiusMeters: 1000, minRating: 4, restaurantCategory: 'japanese' })
    expect(flow.submittedConditions.value).toEqual({ radiusMeters: 1000, minRating: 4, restaurantCategory: 'japanese' })
    expect(geolocation.getCurrentPosition).toHaveBeenCalledTimes(1)
    const second = flow.recommend()
    resolveLocation({ coords: { latitude: 25.033, longitude: 121.5654 } } as GeolocationPosition)
    await second
    expect(pick).toHaveBeenLastCalledWith({ latitude: 25.033, longitude: 121.5654, radiusMeters: 1000, minRating: 4, restaurantCategory: 'hot-pot' })
    expect(pick).toHaveBeenCalledTimes(2)
  })

  it('preserves submitted category after an empty search without retry or relaxation', async () => {
    let rejectPick!: (reason: unknown) => void
    const geolocation = createGeolocation()
    const pick = vi.fn(() => new Promise<PlaceResult>((_resolve, reject) => { rejectPick = reject }))
    const flow = useNearbyFood({ geolocation, pick })
    flow.selectedRestaurantCategory.value = 'japanese'
    flow.selectedRadiusMeters.value = 1000
    flow.selectedMinRating.value = 4
    const first = flow.recommend()
    await vi.waitFor(() => expect(pick).toHaveBeenCalledTimes(1))
    flow.selectedRestaurantCategory.value = 'hot-pot'
    await flow.recommend()
    rejectPick(new Error('no_results'))
    await first
    expect(flow.state.value).toBe('no-results')
    expect(flow.selectedRestaurantCategory.value).toBe('hot-pot')
    expect(flow.submittedConditions.value).toEqual({ radiusMeters: 1000, minRating: 4, restaurantCategory: 'japanese' })
    expect(pick).toHaveBeenCalledTimes(1)
    expect(geolocation?.getCurrentPosition).toHaveBeenCalledTimes(1)
  })
})
