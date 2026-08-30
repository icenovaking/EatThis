import { mount } from '@vue/test-utils'
import { flushPromises } from '@vue/test-utils'
import { describe, expect, it, vi } from 'vitest'
import { ApiRequestError } from './api/nearbyFoodApi'
import App from './App.vue'
import type { PlaceResult } from './types'

const selectedPlace: PlaceResult = {
  name: 'Example Food Shop',
  address: 'Taipei City',
  latitude: 25.0331,
  longitude: 121.5655,
  distanceMeters: 420,
  navigationUrl: 'https://www.google.com/maps/place/example',
  provider: 'google',
}

function successfulGeolocation() {
  return {
    getCurrentPosition: vi.fn((success: PositionCallback) => {
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

describe('EatThis shell', () => {
  it('offers the primary recommendation action', () => {
    const wrapper = mount(App)

    expect(wrapper.get('button').text()).toContain('幫我決定')
  })

  it('displays one selected place and an external navigation link', async () => {
    const wrapper = mount(App, {
      props: {
        geolocation: successfulGeolocation(),
        pick: vi.fn().mockResolvedValue(selectedPlace),
      },
    })

    await wrapper.get('[data-action="recommend"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('[data-state="selected"] h2').text()).toBe('Example Food Shop')
    expect(wrapper.text()).toContain('Taipei City')
    expect(wrapper.text()).toContain('420 公尺')
    expect(wrapper.get('[data-action="navigation"]').attributes('href'))
      .toBe(selectedPlace.navigationUrl)
    expect(wrapper.find('iframe').exists()).toBe(false)
    expect(wrapper.find('[data-map-embed]').exists()).toBe(false)
  })

  it('renders an announced permission state without sending an API request', async () => {
    const pick = vi.fn()
    const geolocation = {
      getCurrentPosition: vi.fn((_success: PositionCallback, error?: PositionErrorCallback) => {
        error?.({ code: 1, message: 'denied' } as GeolocationPositionError)
      }),
    }
    const wrapper = mount(App, { props: { geolocation, pick } })

    await wrapper.get('[data-action="recommend"]').trigger('click')
    await flushPromises()

    const status = wrapper.get('[aria-live="polite"]')
    expect(status.text()).toContain('允許定位')
    expect(status.attributes('role')).toBe('status')
    expect(pick).not.toHaveBeenCalled()
  })

  it('offers a five-kilometer retry only after a no-results response', async () => {
    const pick = vi.fn()
      .mockRejectedValueOnce(new ApiRequestError(404, 'no_results', '找不到附近地點。'))
      .mockResolvedValueOnce(selectedPlace)
    const wrapper = mount(App, {
      props: { geolocation: successfulGeolocation(), pick },
    })

    await wrapper.get('[data-action="recommend"]').trigger('click')
    await flushPromises()
    expect(wrapper.get('[data-state="no-results"]').text()).toContain('沒有找到')

    await wrapper.get('[data-action="retry-expanded"]').trigger('click')
    await flushPromises()

    expect(pick).toHaveBeenNthCalledWith(1, expect.objectContaining({ radiusMeters: 3000 }))
    expect(pick).toHaveBeenNthCalledWith(2, expect.objectContaining({ radiusMeters: 5000 }))
    expect(wrapper.get('[data-state="selected"] h2').text()).toBe('Example Food Shop')
  })

  it('renders provider errors without exposing upstream details', async () => {
    const pick = vi.fn().mockRejectedValue(
      new ApiRequestError(503, 'provider_unavailable', '附近地點服務暫時無法使用，請稍後再試。'),
    )
    const wrapper = mount(App, {
      props: { geolocation: successfulGeolocation(), pick },
    })

    await wrapper.get('[data-action="recommend"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('[data-state="provider-error"]').text()).toContain('服務暫時無法使用')
    expect(wrapper.text()).not.toContain('upstream-secret')
    expect(wrapper.text()).not.toContain('X-Goog-Api-Key')
  })

  it('renders rate-limit retry guidance with the server-provided wait time', async () => {
    const pick = vi.fn().mockRejectedValue(
      new ApiRequestError(429, 'rate_limited', '請稍候再試。', 60),
    )
    const wrapper = mount(App, {
      props: { geolocation: successfulGeolocation(), pick },
    })

    await wrapper.get('[data-action="recommend"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('[data-state="rate-limited"]').text()).toContain('請稍候再試')
    expect(wrapper.text()).toContain('約 60 秒後可以再試')
  })

  it('shows searching state and disables the primary action while waiting', async () => {
    let resolvePick: ((place: PlaceResult) => void) | undefined
    const pick = vi.fn().mockImplementation(
      () => new Promise<PlaceResult>(resolve => { resolvePick = resolve }),
    )
    const wrapper = mount(App, {
      props: { geolocation: successfulGeolocation(), pick },
    })

    const search = wrapper.get('[data-action="recommend"]').trigger('click')
    await vi.waitFor(() => expect(pick).toHaveBeenCalledTimes(1))

    expect(wrapper.get('[data-action="recommend"]').attributes('disabled')).toBeDefined()
    expect(wrapper.get('[aria-live="polite"]').text()).toContain('正在附近搜尋')

    resolvePick?.(selectedPlace)
    await search
    await flushPromises()
    expect(wrapper.get('[data-state="selected"] h2').text()).toBe('Example Food Shop')
  })
})
