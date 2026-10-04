import { mount } from '@vue/test-utils'
import { flushPromises } from '@vue/test-utils'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { ApiRequestError } from './api/nearbyFoodApi'
import App from './App.vue'
import type { PlaceResult } from './types'
import type { GeolocationPort } from './geolocation'

beforeEach(() => localStorage.clear())
afterEach(() => { vi.restoreAllMocks(); vi.unstubAllGlobals() })

const selectedPlace: PlaceResult = {
  name: 'Example Food Shop',
  address: 'Taipei City',
  latitude: 25.0331,
  longitude: 121.5655,
  distanceMeters: 420,
  navigationUrl: 'https://www.google.com/maps/place/example',
  provider: 'google',
}

const traditionalChinesePlace: PlaceResult = {
  ...selectedPlace,
  name: '老地方牛肉麵',
  address: '新北市板橋區文化路一段 1 號',
}

const longFallbackPlace: PlaceResult = {
  ...selectedPlace,
  name: 'Chin Huajiao Banqiao Xianmin Boulevard Traditional Noodle Workshop',
  address: 'No. 1, Section 2, Xianmin Boulevard, Xinmin Village, Banqiao District, New Taipei City, Taiwan 220',
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
  it.each([
    [1234, '（1,234 則評論）'],
    [0, '（0 則評論）'],
    [2147483647, '（2,147,483,647 則評論）'],
  ])('shows review count %s beside the actual rating', async (userRatingCount, label) => {
    const wrapper = mount(App, { props: {
      geolocation: successfulGeolocation(),
      pick: vi.fn().mockResolvedValue({ ...selectedPlace, rating: 4.9, userRatingCount }),
    } })
    await wrapper.get('[data-action="recommend"]').trigger('click')
    await flushPromises()
    expect(wrapper.get('.place-rating').text()).toContain('4.9')
    expect(wrapper.get('[data-review-count]').text()).toBe(label)
    wrapper.unmount()
  })

  it.each([undefined, null, -1, 1.5, '1234', 2147483648, NaN, Infinity, true])(
    'omits an unavailable or invalid review count %s without losing the rating', async (userRatingCount) => {
      const wrapper = mount(App, { props: {
        geolocation: successfulGeolocation(),
        pick: vi.fn().mockResolvedValue({ ...selectedPlace, rating: 4.9, userRatingCount }),
      } })
      await wrapper.get('[data-action="recommend"]').trigger('click')
      await flushPromises()
      expect(wrapper.get('[data-place-rating]').text()).toBe('4.9')
      expect(wrapper.find('[data-review-count]').exists()).toBe(false)
      expect(wrapper.get('.place-rating').text()).not.toContain('（')
      wrapper.unmount()
    },
  )

  it('shows a known review count even when the rating is absent', async () => {
    const wrapper = mount(App, { props: {
      geolocation: successfulGeolocation(),
      pick: vi.fn().mockResolvedValue({ ...selectedPlace, rating: null, userRatingCount: 12 }),
    } })
    await wrapper.get('[data-action="recommend"]').trigger('click')
    await flushPromises()
    expect(wrapper.get('[data-place-rating]').text()).toBe('尚無評分')
    expect(wrapper.get('[data-review-count]').text()).toBe('（12 則評論）')
    wrapper.unmount()
  })

  it('retains measured feedback space across retries without stale results or scroll commands', async () => {
    let observeResize!: ResizeObserverCallback
    const disconnect = vi.fn()
    vi.stubGlobal('ResizeObserver', class {
      constructor(callback: ResizeObserverCallback) { observeResize = callback }
      observe = vi.fn()
      disconnect = disconnect
    })
    const scrollTo = vi.spyOn(window, 'scrollTo').mockImplementation(() => {})
    const focus = vi.spyOn(HTMLElement.prototype, 'focus')
    let locate!: PositionCallback
    let resolvePick!: (place: PlaceResult) => void
    const geolocation = { getCurrentPosition: vi.fn((success: PositionCallback) => { locate = success }) }
    const pick = vi.fn(() => new Promise<PlaceResult>(resolve => { resolvePick = resolve }))
    const wrapper = mount(App, { props: { geolocation, pick } })
    const feedback = wrapper.get('[data-feedback]').element as HTMLElement
    const content = wrapper.get('[data-feedback-content]').element as HTMLElement
    expect(feedback.style.minHeight).toBe('')
    let naturalHeight = 0
    vi.spyOn(content, 'getBoundingClientRect').mockImplementation(() => ({ height: naturalHeight }) as DOMRect)
    await wrapper.get('[data-action="recommend"]').trigger('click')
    naturalHeight = 80
    observeResize([], {} as ResizeObserver)
    expect(feedback.style.minHeight).toBe('80px')
    successfulGeolocation().getCurrentPosition(locate)
    await flushPromises()
    resolvePick(longFallbackPlace)
    await flushPromises()
    naturalHeight = 640
    // No observer notification: the next pre-update measurement must still preserve the old height.
    await wrapper.get('[data-action="recommend"]').trigger('click')
    expect(feedback.style.minHeight).toBe('640px')
    expect(wrapper.find('article').exists()).toBe(false)
    expect(wrapper.find('[data-action="navigation"]').exists()).toBe(false)
    expect(wrapper.get('[data-feedback]').element).toBe(feedback)
    naturalHeight = 80
    observeResize([], {} as ResizeObserver)
    successfulGeolocation().getCurrentPosition(locate)
    await flushPromises()
    resolvePick(selectedPlace)
    await flushPromises()
    naturalHeight = 400
    observeResize([], {} as ResizeObserver)
    observeResize([], {} as ResizeObserver)
    expect(feedback.style.minHeight).toBe('640px')
    expect(wrapper.findAll('article')).toHaveLength(1)
    expect(wrapper.get('article h2').text()).toBe(selectedPlace.name)
    naturalHeight = 720
    observeResize([], {} as ResizeObserver)
    expect(feedback.style.minHeight).toBe('720px')
    expect(scrollTo).not.toHaveBeenCalled()
    expect(focus).not.toHaveBeenCalled()
    wrapper.unmount()
    expect(disconnect).toHaveBeenCalledOnce()
  })

  it.each(['permission-denied', 'unsupported-geolocation', 'no-results', 'provider-error', 'rate-limited'] as const)(
    'keeps reserved feedback space and removes old navigation on %s', async (outcome) => {
      vi.stubGlobal('ResizeObserver', undefined)
      const geolocation: GeolocationPort = successfulGeolocation()
      const pick = vi.fn().mockResolvedValueOnce(selectedPlace)
      const wrapper = mount(App, { props: { geolocation, pick } })
      await wrapper.get('[data-action="recommend"]').trigger('click')
      await flushPromises()
      const feedback = wrapper.get('[data-feedback]').element as HTMLElement
      vi.spyOn(wrapper.get('[data-feedback-content]').element, 'getBoundingClientRect')
        .mockReturnValue({ height: 640 } as DOMRect)
      if (outcome === 'permission-denied') {
        geolocation.getCurrentPosition = vi.fn((_success, fail) => fail?.({ code: 1 } as GeolocationPositionError))
      } else if (outcome === 'unsupported-geolocation') {
        geolocation.getCurrentPosition = undefined as unknown as GeolocationPort['getCurrentPosition']
      } else {
        const status = outcome === 'no-results' ? 404 : outcome === 'rate-limited' ? 429 : 503
        pick.mockRejectedValueOnce(new ApiRequestError(status, outcome.replace('-', '_'), '請稍後再試。'))
      }
      await wrapper.get('[data-action="recommend"]').trigger('click')
      await flushPromises()
      expect(wrapper.get('[role="status"]').attributes('data-state')).toBe(outcome)
      expect(feedback.style.minHeight).toBe('640px')
      expect(wrapper.find('article').exists()).toBe(false)
      expect(wrapper.find('[data-action="navigation"]').exists()).toBe(false)
      wrapper.unmount()
    },
  )

  it('keeps the idle surface quiet while preserving the current conditions', () => {
    const wrapper = mount(App)

    for (const copy of [
      '附近的城市飲食指南',
      '今日附近',
      '先決定你願意走多遠，EatThis 只在你按下按鈕後取一次位置，替你選一間。',
      '選一種想吃的類型，或交給我們決定',
      '點星星左半選半星，右半選整星',
      '按下後才會使用目前位置；結果會交給 Google Maps 開啟路線。',
      '準備好了',
      'GPS 只在你要求時使用',
      '外部導覽',
    ]) {
      expect(wrapper.text()).not.toContain(copy)
    }
    expect(wrapper.get('header').text()).toBe('EatThis')
    expect(wrapper.get('h1').text()).toBe('今天，吃什麼？')
    expect(wrapper.get('#radius-help').text()).toBe('拖曳或使用方向鍵調整範圍')
    expect(wrapper.get('[data-action="recommend"]').text()).toContain('目前條件 · 100 公尺 · 不限類型 · 不限評分')
    expect(wrapper.get('[role="status"]').text()).toBe('')
    expect(wrapper.get('[role="status"]').attributes('aria-live')).toBe('polite')
    expect(wrapper.get('[role="status"]').attributes('aria-atomic')).toBe('true')
    expect(wrapper.find('.state-panel').exists()).toBe(false)
    expect(wrapper.find('.state-mark').exists()).toBe(false)
    expect(wrapper.find('footer').exists()).toBe(false)
  })

  it('keeps pending edits quiet without location or API requests', async () => {
    const geolocation = successfulGeolocation()
    const pick = vi.fn()
    const wrapper = mount(App, { props: { geolocation, pick } })
    await wrapper.get('input[type="range"]').setValue('700')
    await wrapper.get('input[name="restaurant-category"][value="japanese"]').setValue(true)
    await wrapper.get('input[name="minimum-rating"][value="4.5"]').setValue(true)
    expect(wrapper.get('[data-action="recommend"]').text()).toContain('目前條件 · 700 公尺 · 日式 · 4.5 星以上')
    expect(wrapper.get('[role="status"]').text()).toBe('')
    expect(wrapper.find('.state-panel').exists()).toBe(false)
    expect(geolocation.getCurrentPosition).not.toHaveBeenCalled()
    expect(pick).not.toHaveBeenCalled()
  })

  it('updates the same live region from idle through locating searching and selected', async () => {
    let locate!: PositionCallback
    let resolvePick!: (place: PlaceResult) => void
    const geolocation = { getCurrentPosition: vi.fn((success: PositionCallback) => { locate = success }) }
    const pick = vi.fn(() => new Promise<PlaceResult>(resolve => { resolvePick = resolve }))
    const wrapper = mount(App, { props: { geolocation, pick } })
    const live = wrapper.get('[role="status"]').element
    expect(live.textContent).toBe('')
    await wrapper.get('[data-action="recommend"]').trigger('click')
    expect(wrapper.get('[role="status"]').element).toBe(live)
    expect(live.textContent).toContain('正在取得目前位置')
    expect(wrapper.get('[data-action="recommend"]').attributes('disabled')).toBeDefined()
    await wrapper.get('[data-action="recommend"]').trigger('click')
    expect(geolocation.getCurrentPosition).toHaveBeenCalledTimes(1)
    expect(pick).not.toHaveBeenCalled()
    successfulGeolocation().getCurrentPosition(locate)
    await flushPromises()
    expect(live.textContent).toContain('正在附近搜尋')
    await wrapper.get('[data-action="recommend"]').trigger('click')
    expect(pick).toHaveBeenCalledTimes(1)
    resolvePick(selectedPlace)
    await flushPromises()
    expect(wrapper.get('[role="status"]').element).toBe(live)
    expect(live.textContent).toContain('已依 100 公尺、不限類型、不限評分')
    expect(wrapper.get('[data-action="recommend"]').attributes('disabled')).toBeUndefined()
    expect(wrapper.get('[data-attribution="google-maps"]').attributes('alt')).toBe('Google Maps')
    expect(wrapper.find('.action-note').exists()).toBe(false)
    expect(wrapper.find('footer').exists()).toBe(false)
  })

  it('announces unsupported location with recovery in the existing live region', async () => {
    const pick = vi.fn()
    // Model a browser without the getCurrentPosition capability.
    const wrapper = mount(App, { props: { geolocation: {} as GeolocationPort, pick } })
    const live = wrapper.get('[role="status"]').element
    expect(live.textContent).toBe('')
    await wrapper.get('[data-action="recommend"]').trigger('click')
    await flushPromises()
    expect(wrapper.get('[role="status"]').element).toBe(live)
    expect(live.textContent).toContain('此瀏覽器不支援定位功能')
    expect(wrapper.text()).toContain('EatThis 不會猜測或套用其他位置')
    expect(pick).not.toHaveBeenCalled()
    expect(wrapper.find('.action-note').exists()).toBe(false)
    expect(wrapper.find('footer').exists()).toBe(false)
  })

  it('keeps named filter groups and resolves every descriptive reference', () => {
    const wrapper = mount(App)

    expect(wrapper.findAll('fieldset legend').map(legend => legend.text())).toEqual(['餐廳類型', '最低評分'])
    for (const control of wrapper.findAll('[aria-describedby]')) {
      for (const id of control.attributes('aria-describedby')!.split(/\s+/)) {
        expect(wrapper.find(`[id="${id}"]`).exists()).toBe(true)
      }
    }
  })

  it('offers ten accessible half-star choices and an unrestricted default without side effects', async () => {
    const geolocation = successfulGeolocation()
    const pick = vi.fn()
    const wrapper = mount(App, { props: { geolocation, pick } })
    const choices = wrapper.findAll('input[name="minimum-rating"]')

    expect(choices).toHaveLength(11)
    expect((wrapper.get('#rating-unrestricted').element as HTMLInputElement).checked).toBe(true)
    expect(wrapper.get('[data-rating-value]').text()).toBe('不限評分')
    for (const [value, text] of [[0.5, '0.5'], [1, '1'], [1.5, '1.5'], [2, '2'], [2.5, '2.5'], [3, '3'], [3.5, '3.5'], [4, '4'], [4.5, '4.5'], [5, '5']] as const) {
      const radio = wrapper.get(`input[name="minimum-rating"][value="${value}"]`)
      expect(radio.attributes('aria-label')).toBe(`最低評分 ${text} 星以上`)
      await radio.setValue(true)
      expect((radio.element as HTMLInputElement).checked).toBe(true)
      expect(wrapper.get('[data-rating-value]').text()).toBe(`${text} 星以上`)
    }
    await wrapper.get('#rating-unrestricted').setValue(true)
    expect(wrapper.get('[data-rating-value]').text()).toBe('不限評分')
    expect(geolocation.getCurrentPosition).not.toHaveBeenCalled()
    expect(pick).not.toHaveBeenCalled()
  })

  it.each([3.5, 4, 4.5])('submits the selected half-star threshold %s', async (minRating) => {
    const pick = vi.fn().mockResolvedValue(selectedPlace)
    const wrapper = mount(App, { props: { geolocation: successfulGeolocation(), pick } })
    await wrapper.get('input[type="range"]').setValue('700')
    await wrapper.get(`input[name="minimum-rating"][value="${minRating}"]`).setValue(true)
    await wrapper.get('[data-action="recommend"]').trigger('click')
    await flushPromises()
    expect(pick).toHaveBeenCalledExactlyOnceWith({ latitude: 25.033, longitude: 121.5654, radiusMeters: 700, minRating, restaurantCategory: null })
  })

  it.each([4.3, null, undefined])('shows actual result rating %s without threshold rounding', async (rating) => {
    const wrapper = mount(App, { props: {
      geolocation: successfulGeolocation(),
      pick: vi.fn().mockResolvedValue({ ...selectedPlace, rating }),
    } })
    await wrapper.get('[data-action="recommend"]').trigger('click')
    await flushPromises()
    expect(wrapper.get('[data-place-rating]').text()).toBe(rating == null ? '尚無評分' : '4.3')
  })

  it('describes submitted conditions after no results while preserving later pending controls', async () => {
    let rejectPick!: (reason: unknown) => void
    const pick = vi.fn(() => new Promise<PlaceResult>((_resolve, reject) => { rejectPick = reject }))
    const geolocation = successfulGeolocation()
    const wrapper = mount(App, { props: { geolocation, pick } })
    await wrapper.get('input[type="range"]').setValue('700')
    await wrapper.get('input[name="minimum-rating"][value="4.5"]').setValue(true)
    await wrapper.get('[data-action="recommend"]').trigger('click')
    await flushPromises()
    await wrapper.get('input[type="range"]').setValue('1000')
    await wrapper.get('input[name="minimum-rating"][value="3.5"]').setValue(true)
    rejectPick(new ApiRequestError(404, 'no_results', '本次未找到合適的店家。'))
    await flushPromises()
    const recovery = wrapper.get('section[data-state="no-results"]')
    expect(recovery.text()).toContain('700 公尺')
    expect(recovery.text()).toContain('4.5 星以上')
    expect(recovery.text()).toContain('降低最低評分')
    expect(wrapper.get('[data-rating-value]').text()).toBe('3.5 星以上')
    expect(wrapper.get('[data-radius-value]').text()).toBe('1 公里')
    expect(pick).toHaveBeenCalledTimes(1)
    expect(geolocation.getCurrentPosition).toHaveBeenCalledTimes(1)
  })

  it('offers lowering the threshold at maximum radius without an expansion', async () => {
    const wrapper = mount(App, { props: {
      geolocation: successfulGeolocation(),
      pick: vi.fn().mockRejectedValue(new ApiRequestError(404, 'no_results', '本次未找到合適的店家。')),
    } })
    await wrapper.get('input[type="range"]').setValue('3000')
    await wrapper.get('input[name="minimum-rating"][value="4.5"]').setValue(true)
    await wrapper.get('[data-action="recommend"]').trigger('click')
    await flushPromises()
    const recovery = wrapper.get('section[data-state="no-results"]')
    expect(recovery.text()).toContain('3 公里')
    expect(recovery.text()).toContain('降低最低評分')
    expect(recovery.text()).not.toContain('把搜尋距離調大')
  })

  it('offers the primary recommendation action', () => {
    const wrapper = mount(App)

    expect(wrapper.get('button').text()).toContain('幫我決定')
  })

  it('renders a 100-to-3000-metre range control and formats pending values without side effects', async () => {
    const geolocation = successfulGeolocation()
    const pick = vi.fn().mockResolvedValue(selectedPlace)
    const wrapper = mount(App, { props: { geolocation, pick } })
    const radius = wrapper.get('input[type="range"]')

    expect(radius.attributes('min')).toBe('100')
    expect(radius.attributes('max')).toBe('3000')
    expect(radius.attributes('step')).toBe('100')
    expect((radius.element as HTMLInputElement).value).toBe('100')
    expect(wrapper.get('[data-radius-value]').text()).toBe('100 公尺')

    await radius.setValue('700')
    expect(wrapper.get('[data-radius-value]').text()).toBe('700 公尺')
    expect(geolocation.getCurrentPosition).not.toHaveBeenCalled()
    expect(pick).not.toHaveBeenCalled()

    await radius.setValue('3000')
    expect(wrapper.get('[data-radius-value]').text()).toBe('3 公里')
    expect(geolocation.getCurrentPosition).not.toHaveBeenCalled()
    expect(pick).not.toHaveBeenCalled()
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

  it('keeps the searched radius after no results and never offers a five-kilometre action', async () => {
    const geolocation = successfulGeolocation()
    const pick = vi.fn().mockRejectedValue(
      new ApiRequestError(404, 'no_results', '找不到附近地點。'),
    )
    const wrapper = mount(App, {
      props: { geolocation, pick },
    })

    await wrapper.get('input[type="range"]').setValue('700')
    await wrapper.get('[data-action="recommend"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('section[data-state="no-results"]').text()).toContain('700 公尺')
    expect((wrapper.get('input[type="range"]').element as HTMLInputElement).value).toBe('700')
    expect(wrapper.find('[data-action="retry-expanded"]').exists()).toBe(false)
    expect(wrapper.text()).not.toContain('5 公里')
    expect(pick).toHaveBeenCalledTimes(1)
    expect(pick).toHaveBeenCalledWith(expect.objectContaining({ radiusMeters: 700 }))
    expect(geolocation.getCurrentPosition).toHaveBeenCalledTimes(1)
  })

  it('renders Traditional Chinese place text unchanged', async () => {
    const wrapper = mount(App, {
      props: {
        geolocation: successfulGeolocation(),
        pick: vi.fn().mockResolvedValue(traditionalChinesePlace),
      },
    })

    await wrapper.get('[data-action="recommend"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('article[data-state="selected"] h2').text()).toBe('老地方牛肉麵')
    expect(wrapper.get('article[data-state="selected"]').text())
      .toContain('新北市板橋區文化路一段 1 號')
  })

  it('keeps a long Latin fallback visible inside the result structure', async () => {
    const wrapper = mount(App, {
      props: {
        geolocation: successfulGeolocation(),
        pick: vi.fn().mockResolvedValue(longFallbackPlace),
      },
    })

    await wrapper.get('[data-action="recommend"]').trigger('click')
    await flushPromises()

    const result = wrapper.get('article[data-state="selected"]')
    expect(result.get('h2').text()).toBe(longFallbackPlace.name)
    expect(result.get('.place-details div:first-child dd').text()).toBe(longFallbackPlace.address)
    expect(result.get('[data-action="navigation"]').attributes('href'))
      .toBe(longFallbackPlace.navigationUrl)
  })

  it('shows accessible Google Maps attribution without the raw provider label', async () => {
    const wrapper = mount(App, {
      props: {
        geolocation: successfulGeolocation(),
        pick: vi.fn().mockResolvedValue(selectedPlace),
      },
    })

    await wrapper.get('[data-action="recommend"]').trigger('click')
    await flushPromises()

    const attribution = wrapper.get('[data-attribution="google-maps"]')
    expect(attribution.element.tagName).toBe('IMG')
    expect(attribution.attributes('alt') ?? attribution.attributes('aria-label'))
      .toBe('Google Maps')
    expect(wrapper.find('.place-provider').exists()).toBe(false)
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

describe('restaurant category surface', () => {
  it('offers eleven labeled native radios with one default and no selection side effects', async () => {
    const geolocation = successfulGeolocation()
    const pick = vi.fn()
    const wrapper = mount(App, { props: { geolocation, pick } })
    const group = wrapper.get('fieldset[data-control="restaurant-category"]')
    expect(group.get('legend').text()).toBe('餐廳類型')
    const labels = ['不限類型', '台式／中式', '日式', '韓式', '火鍋', '燒烤', '義式', '早餐／早午餐', '速食', '素食', '咖啡／甜點']
    const radios = group.findAll('input[type="radio"][name="restaurant-category"]')
    expect(radios.map(radio => radio.attributes('aria-label'))).toEqual(labels)
    expect(radios.filter(radio => (radio.element as HTMLInputElement).checked)).toHaveLength(1)
    expect((radios[0]!.element as HTMLInputElement).checked).toBe(true)
    for (const radio of radios) {
      await radio.setValue(true)
      expect((radio.element as HTMLInputElement).checked).toBe(true)
      expect(radios.filter(choice => (choice.element as HTMLInputElement).checked)).toHaveLength(1)
      expect(wrapper.get('[data-action="recommend"]').text()).toContain(radio.attributes('aria-label'))
    }
    await radios[0]!.setValue(true)
    expect(wrapper.get('[data-action="recommend"]').text()).toContain('不限類型')
    expect(geolocation.getCurrentPosition).not.toHaveBeenCalled()
    expect(pick).not.toHaveBeenCalled()
  })

  it.each(['selected', 'no-results'] as const)('keeps submitted category in %s while pending edits wait for next action', async (outcome) => {
    let resolvePick!: (place: PlaceResult) => void
    let rejectPick!: (reason: unknown) => void
    const pick = vi.fn(() => new Promise<PlaceResult>((resolve, reject) => { resolvePick = resolve; rejectPick = reject }))
    const geolocation = successfulGeolocation()
    const wrapper = mount(App, { props: { geolocation, pick } })
    await wrapper.get('input[name="restaurant-category"][value="japanese"]').setValue(true)
    await wrapper.get('input[type="range"]').setValue('1000')
    await wrapper.get('input[name="minimum-rating"][value="4"]').setValue(true)
    await wrapper.get('[data-action="recommend"]').trigger('click')
    await flushPromises()
    await wrapper.get('input[name="restaurant-category"][value="hot-pot"]').setValue(true)
    expect(wrapper.get('[data-action="recommend"]').attributes('disabled')).toBeDefined()
    expect(wrapper.get('[data-action="recommend"]').text()).toContain('火鍋')
    if (outcome === 'selected') resolvePick(selectedPlace)
    else rejectPick(new ApiRequestError(404, 'no_results', '本次未找到合適的店家。'))
    await flushPromises()
    const status = wrapper.get('[aria-live="polite"]')
    expect(status.text()).toContain('日式')
    expect(status.text()).toContain('1 公里')
    expect(status.text()).toContain('4 星以上')
    expect(status.text()).not.toContain('火鍋')
    expect((wrapper.get('input[name="restaurant-category"][value="hot-pot"]').element as HTMLInputElement).checked).toBe(true)
    if (outcome === 'no-results') {
      const recovery = wrapper.get('section[data-state="no-results"]')
      expect(recovery.text()).toContain('日式')
      expect(recovery.text()).toContain('不限類型')
      expect(recovery.text()).toContain('降低最低評分')
      expect(recovery.text()).toContain('把搜尋距離調大')
    } else expect(wrapper.findAll('article[data-state="selected"]')).toHaveLength(1)
    expect(pick).toHaveBeenCalledExactlyOnceWith({ latitude: 25.033, longitude: 121.5654, radiusMeters: 1000, minRating: 4, restaurantCategory: 'japanese' })
    expect(geolocation.getCurrentPosition).toHaveBeenCalledTimes(1)
  })

  it('offers an explicit retry at maximum radius and unrestricted category without automatic search', async () => {
    const pick = vi.fn().mockRejectedValue(new ApiRequestError(404, 'no_results', '本次未找到合適的店家。'))
    const wrapper = mount(App, { props: { geolocation: successfulGeolocation(), pick } })
    await wrapper.get('input[type="range"]').setValue('3000')
    await wrapper.get('[data-action="recommend"]').trigger('click')
    await flushPromises()
    const recovery = wrapper.get('section[data-state="no-results"]')
    expect(recovery.text()).toContain('不限類型')
    expect(recovery.text()).toContain('不限評分')
    expect(recovery.text()).toContain('再找一次')
    expect(recovery.text()).not.toContain('改選其他餐廳類型')
    expect(recovery.text()).not.toContain('把搜尋距離調大')
    expect(pick).toHaveBeenCalledTimes(1)
  })
})
