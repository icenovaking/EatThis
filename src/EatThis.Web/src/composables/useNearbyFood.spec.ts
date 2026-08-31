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
    expect(flow.selectedRadiusMeters.value).toBe(700)
    expect(pick).toHaveBeenCalledTimes(1)
    expect(pick).toHaveBeenCalledWith({
      latitude: 25.0330,
      longitude: 121.5654,
      radiusMeters: 700,
    })
    expect(geolocation?.getCurrentPosition).toHaveBeenCalledTimes(1)
  })
})
