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
  it('requests one position and searches with the default three-kilometer radius', async () => {
    const geolocation = createGeolocation()
    const pick = vi.fn().mockResolvedValue(selectedPlace)
    const flow = useNearbyFood({ geolocation, pick })

    await flow.recommend()

    expect(geolocation?.getCurrentPosition).toHaveBeenCalledTimes(1)
    expect(pick).toHaveBeenCalledWith({
      latitude: 25.0330,
      longitude: 121.5654,
      radiusMeters: 3000,
    })
    expect(flow.state.value).toBe('selected')
    expect(flow.place.value).toEqual(selectedPlace)
  })

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

  it('offers an explicit five-kilometer retry after no results without watching GPS', async () => {
    const geolocation = createGeolocation()
    const noResults = new Error('no_results')
    const pick = vi.fn()
      .mockRejectedValueOnce(noResults)
      .mockResolvedValueOnce(selectedPlace)
    const flow = useNearbyFood({ geolocation, pick })

    await flow.recommend()
    await flow.retryExpandedRadius()

    expect(flow.state.value).toBe('selected')
    expect(pick).toHaveBeenNthCalledWith(1, {
      latitude: 25.0330,
      longitude: 121.5654,
      radiusMeters: 3000,
    })
    expect(pick).toHaveBeenNthCalledWith(2, {
      latitude: 25.0330,
      longitude: 121.5654,
      radiusMeters: 5000,
    })
    expect(geolocation?.getCurrentPosition).toHaveBeenCalledTimes(1)
  })
})
