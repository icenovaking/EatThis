<script setup lang="ts">
import { computed } from 'vue'
import { useNearbyFood, type NearbyFoodOptions } from './composables/useNearbyFood'
import type { NearbyFoodState } from './types'

const props = defineProps<NearbyFoodOptions>()
const {
  state,
  place,
  error,
  isBusy,
  recommend,
  retryExpandedRadius,
} = useNearbyFood(props)

const stateCopy: Record<NearbyFoodState, string> = {
  idle: '按下幫我決定，EatThis 只會使用你此刻的位置搜尋一次。',
  locating: '正在取得目前位置，請在瀏覽器提示中允許定位。',
  searching: '正在附近搜尋餐飲地點，請稍等。',
  selected: '已為你選出一間，接著可以開啟地圖。',
  'permission-denied': '需要允許定位，才能搜尋你附近的餐飲地點。',
  'unsupported-geolocation': '此瀏覽器不支援定位功能，請改用支援 GPS 的手機瀏覽器。',
  'no-results': '這個範圍沒有找到可推薦的地點。',
  'provider-error': '附近地點服務暫時無法使用，請稍後再試。',
  'rate-limited': '剛剛搜尋得太頻繁，請依提示稍候再試。',
}

const stateText = computed(() => stateCopy[state.value])

function formatDistance(distanceMeters: number): string {
  return distanceMeters < 1000
    ? `${Math.round(distanceMeters)} 公尺`
    : `${(distanceMeters / 1000).toFixed(1)} 公里`
}
</script>

<template>
  <main class="eatthis-shell">
    <div class="top-line" aria-label="EatThis product header">
      <span class="wordmark">EatThis</span>
      <span class="top-line-note">一個位置，一個決定</span>
    </div>

    <section class="timetable-rack" :class="{ 'is-busy': isBusy }">
      <div class="measurement-rail" aria-label="搜尋範圍">
        <span class="rail-tick rail-tick--active">3 KM</span>
        <span class="rail-line" aria-hidden="true"></span>
        <span class="rail-tick">5 KM</span>
      </div>

      <div class="title-slide">
        <h1>今天吃什麼？</h1>
        <p class="lead-copy">
          把選擇交給附近的餐飲地點。EatThis 會先取得一次 GPS，再替你挑出一間。
        </p>
      </div>

      <div class="action-slide">
        <button
          class="primary-plate"
          data-action="recommend"
          type="button"
          :disabled="isBusy"
          @click="recommend"
        >
          <span>{{ isBusy ? '正在決定' : '幫我決定' }}</span>
          <span class="plate-detail">{{ isBusy ? '請稍候' : '目前位置 · 3 公里' }}</span>
        </button>
        <p class="action-note">不嵌入地圖；選好後會交給 Google Maps 顯示路線。</p>
      </div>

      <div
        class="state-slide"
        :class="`state-slide--${state}`"
        :data-state="state"
        role="status"
        aria-live="polite"
        aria-atomic="true"
      >
        <span class="state-mark" aria-hidden="true"></span>
        <p class="state-copy">{{ stateText }}</p>
        <p v-if="error" class="state-detail">{{ error.message }}</p>
      </div>

      <section v-if="state === 'no-results'" class="recovery-slide" data-state="no-results">
        <p class="slide-label">搜尋範圍仍然有界線</p>
        <h2>要不要再往外找一點？</h2>
        <p>這次會沿用同一個位置，只把範圍明確擴大到 5 公里。</p>
        <button
          class="secondary-plate"
          data-action="retry-expanded"
          type="button"
          :disabled="isBusy"
          @click="retryExpandedRadius"
        >
          擴大到 5 公里再試
        </button>
      </section>

      <article v-if="place" class="place-slide" data-state="selected">
        <div class="place-heading">
          <span class="slide-label">已選出一間</span>
          <span class="place-provider">{{ place.provider }}</span>
        </div>
        <h2>{{ place.name }}</h2>
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
          class="navigation-plate"
          data-action="navigation"
          :href="place.navigationUrl"
          target="_blank"
          rel="noreferrer"
        >
          <span>在地圖中開啟</span>
          <span class="link-detail">交給 Google Maps web / app</span>
        </a>
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
      <span>GPS only when you ask</span>
      <span>外部導覽</span>
    </footer>
  </main>
</template>
