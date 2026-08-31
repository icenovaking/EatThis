<script setup lang="ts">
import { computed } from 'vue'
import googleMapsLogo from './assets/google-maps-logo.svg'
import {
  MAXIMUM_RADIUS_METERS,
  MINIMUM_RADIUS_METERS,
  RADIUS_STEP_METERS,
  useNearbyFood,
  type NearbyFoodOptions,
} from './composables/useNearbyFood'
import type { NearbyFoodState } from './types'

const props = defineProps<NearbyFoodOptions>()
const {
  state,
  place,
  error,
  selectedRadiusMeters,
  isBusy,
  recommend,
} = useNearbyFood(props)

const stateCopy: Record<NearbyFoodState, string> = {
  idle: '準備好了。選好距離後，按下按鈕讓 EatThis 幫你挑一間。',
  locating: '正在取得目前位置，請在瀏覽器提示中允許定位。',
  searching: '正在附近搜尋餐飲地點，請稍等。',
  selected: '已為你選出一間，接著可以開啟地圖。',
  'permission-denied': '需要允許定位，才能搜尋你附近的餐飲地點。',
  'unsupported-geolocation': '此瀏覽器不支援定位功能，請改用支援 GPS 的手機瀏覽器。',
  'no-results': '這個範圍沒有找到可推薦的地點。',
  'provider-error': '附近地點服務暫時無法使用，請稍後再試。',
  'rate-limited': '剛剛搜尋得太頻繁，請依提示稍候再試。',
}

const radiusLabel = computed(() => formatRadius(selectedRadiusMeters.value))
const isGooglePlace = computed(() => place.value?.provider.toLowerCase() === 'google')
const stateText = computed(() => state.value === 'no-results'
  ? `在 ${radiusLabel.value} 內目前沒有找到可推薦的地點。`
  : stateCopy[state.value])

function formatDistance(distanceMeters: number): string {
  return distanceMeters < 1000
    ? `${Math.round(distanceMeters)} 公尺`
    : `${(distanceMeters / 1000).toFixed(1)} 公里`
}

function formatRadius(radiusMeters: number): string {
  if (radiusMeters >= 1000) {
    const kilometers = radiusMeters / 1000
    return `${Number.isInteger(kilometers) ? kilometers : kilometers.toFixed(1)} 公里`
  }

  return `${Math.round(radiusMeters)} 公尺`
}
</script>

<template>
  <main class="eatthis-shell">
    <header class="app-header" aria-label="EatThis product header">
      <div class="brand-lockup">
        <span class="wordmark">EatThis</span>
        <span class="brand-rule" aria-hidden="true"></span>
        <span class="top-line-note">附近的城市飲食指南</span>
      </div>
      <span class="header-status">今日附近</span>
    </header>

    <section class="search-surface" :class="{ 'is-busy': isBusy }" aria-labelledby="search-title">
      <div class="intro-block">
        <h1 id="search-title">今天，吃什麼？</h1>
        <p class="lead-copy">
          先決定你願意走多遠，EatThis 只在你按下按鈕後取一次位置，替你選一間。
        </p>
      </div>

      <div class="radius-control" data-control-group>
        <div class="control-heading">
          <div>
            <label for="radius-input">搜尋距離</label>
            <p id="radius-help">拖曳或使用方向鍵調整範圍</p>
          </div>
          <output class="radius-value" data-radius-value for="radius-input">
            {{ radiusLabel }}
          </output>
        </div>
        <input
          id="radius-input"
          data-control="radius"
          type="range"
          :min="MINIMUM_RADIUS_METERS"
          :max="MAXIMUM_RADIUS_METERS"
          :step="RADIUS_STEP_METERS"
          v-model.number="selectedRadiusMeters"
          :aria-valuetext="radiusLabel"
          aria-describedby="radius-help"
        >
        <div class="range-scale" aria-hidden="true">
          <span>100 公尺</span>
          <span>3 公里</span>
        </div>
      </div>

      <div class="action-block">
        <button
          class="primary-action"
          data-action="recommend"
          type="button"
          :disabled="isBusy"
          @click="recommend"
        >
          <span>{{ isBusy ? '正在搜尋' : state === 'no-results' ? '再找一次' : '幫我決定' }}</span>
          <span class="action-detail">{{ isBusy ? '請稍候' : `目前範圍 · ${radiusLabel}` }}</span>
        </button>
        <p class="action-note">按下後才會使用目前位置；結果會交給 Google Maps 開啟路線。</p>
      </div>

      <div
        class="state-panel"
        :class="`state-panel--${state}`"
        :data-state="state"
        role="status"
        aria-live="polite"
        aria-atomic="true"
      >
        <span class="state-mark" aria-hidden="true"></span>
        <p class="state-copy">{{ stateText }}</p>
        <p v-if="error" class="state-detail">{{ error.message }}</p>
      </div>

      <section v-if="state === 'no-results'" class="recovery-panel" data-state="no-results">
        <p class="recovery-radius">搜尋距離：<strong>{{ radiusLabel }}</strong></p>
        <h2>這一帶暫時沒有合適的店。</h2>
        <p v-if="selectedRadiusMeters < MAXIMUM_RADIUS_METERS">
          把搜尋距離調大，再按上方「幫我決定」重新搜尋。
        </p>
        <p v-else>
          已經搜尋到 3 公里；按上方「再找一次」重新取得位置。
        </p>
      </section>

      <article v-if="place" class="place-sheet" data-state="selected">
        <div class="place-heading">
          <div>
            <h2>{{ place.name }}</h2>
          </div>
        </div>
        <dl class="place-details">
          <div>
            <dt>地址</dt>
            <dd>{{ place.address }}</dd>
          </div>
          <div>
            <dt>距離</dt>
            <dd>{{ formatDistance(place.distanceMeters) }}</dd>
          </div>
        </dl>
        <a
          class="navigation-action"
          data-action="navigation"
          :href="place.navigationUrl"
          target="_blank"
          rel="noreferrer"
        >
          <span>在地圖中開啟</span>
          <span class="link-detail">交給 Google Maps web / app</span>
        </a>
        <div v-if="isGooglePlace" class="google-attribution" aria-label="Google Maps attribution">
          <img
            data-attribution="google-maps"
            :src="googleMapsLogo"
            alt="Google Maps"
            width="98"
            height="18"
          >
        </div>
      </article>

      <p v-if="state === 'permission-denied'" class="recovery-note">
        請在瀏覽器的網站權限中重新允許定位，再按一次「幫我決定」。
      </p>
      <p v-if="state === 'unsupported-geolocation'" class="recovery-note">
        請改用支援定位功能的手機瀏覽器；EatThis 不會猜測或套用其他位置。
      </p>
      <p v-if="state === 'provider-error' || state === 'rate-limited'" class="recovery-note">
        {{ error?.retryAfterSeconds ? `約 ${error.retryAfterSeconds} 秒後可以再試。` : '確認網路後可以再按一次。' }}
      </p>
    </section>

    <footer class="footer-note">
      <span>GPS 只在你要求時使用</span>
      <span>外部導覽</span>
    </footer>
  </main>
</template>
