<script setup lang="ts">
import { computed } from 'vue'
import { RESTAURANT_CATEGORY_OPTIONS, formatRestaurantCategory } from './restaurantCategories'
import googleMapsLogo from './assets/google-maps-logo.svg'
import {
  MAXIMUM_RADIUS_METERS,
  MINIMUM_RADIUS_METERS,
  RADIUS_STEP_METERS,
  MAXIMUM_RATING,
  RATING_STEP,
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
  selectedMinRating,
  selectedRestaurantCategory,
  submittedConditions,
  isBusy,
  recommend,
} = useNearbyFood(props)

const stateCopy: Record<NearbyFoodState, string> = {
  idle: '',
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
const categoryLabel = computed(() => formatRestaurantCategory(selectedRestaurantCategory.value))
const searchedCategoryLabel = computed(() => formatRestaurantCategory(submittedConditions.value?.restaurantCategory ?? null))
const ratingLabel = computed(() => formatRating(selectedMinRating.value))
const searchedRadiusLabel = computed(() => formatRadius(submittedConditions.value?.radiusMeters ?? selectedRadiusMeters.value))
const searchedRatingLabel = computed(() => formatRating(submittedConditions.value?.minRating ?? null))
const searchedRadius = computed(() => submittedConditions.value?.radiusMeters ?? selectedRadiusMeters.value)
const stars = Array.from({ length: MAXIMUM_RATING }, (_, index) => ({
  number: index + 1,
  values: [index + 1 - RATING_STEP, index + 1],
}))
const starPath = 'M12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26Z'
const isGooglePlace = computed(() => place.value?.provider.toLowerCase() === 'google')
const stateText = computed(() => state.value === 'no-results'
  ? `本次在 ${searchedRadiusLabel.value} 內未找到符合「${searchedCategoryLabel.value} · ${searchedRatingLabel.value}」的店家。`
  : state.value === 'selected'
    ? `已依 ${searchedRadiusLabel.value}、${searchedCategoryLabel.value}、${searchedRatingLabel.value} 為你選出一間。`
    : stateCopy[state.value])

function formatRating(minRating: number | null): string {
  return minRating === null ? '不限評分' : `${minRating} 星以上`
}

function starClip(number: number): string {
  const fill = Math.max(0, Math.min(1, (selectedMinRating.value ?? 0) - (number - 1)))
  return `inset(0 ${(1 - fill) * 100}% 0 0)`
}

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
      <span class="wordmark">EatThis</span>
    </header>

    <section class="search-surface" :class="{ 'is-busy': isBusy }" aria-labelledby="search-title">
      <div class="intro-block">
        <h1 id="search-title">今天，吃什麼？</h1>
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

      <fieldset class="category-control" data-control="restaurant-category">
        <legend>餐廳類型</legend>
        <div class="category-options">
          <label v-for="option in RESTAURANT_CATEGORY_OPTIONS" :key="option.value ?? 'unrestricted'" class="category-option">
            <input type="radio" name="restaurant-category" :value="option.value" v-model="selectedRestaurantCategory" :aria-label="option.label">
            <svg class="category-check" viewBox="0 0 16 16" aria-hidden="true"><path d="m3 8 3 3 7-7" /></svg>
            <span>{{ option.label }}</span>
          </label>
        </div>
      </fieldset>

      <fieldset class="rating-control">
        <legend>最低評分</legend>
        <div class="rating-options">
          <div class="rating-stars">
            <div v-for="star in stars" :key="star.number" class="rating-star">
              <svg class="rating-star-icon" viewBox="0 0 24 24" aria-hidden="true">
                <path :d="starPath" />
              </svg>
              <svg class="rating-star-icon rating-star-filled" :style="{ clipPath: starClip(star.number) }" viewBox="0 0 24 24" aria-hidden="true">
                <path :d="starPath" />
              </svg>
              <label v-for="(value, half) in star.values" :key="value" class="rating-half" :class="{ 'rating-half--right': half === 1 }">
                <input type="radio" name="minimum-rating" :value="value" v-model="selectedMinRating" :aria-label="`最低評分 ${value} 星以上`">
              </label>
            </div>
          </div>
          <div class="rating-summary">
            <span class="rating-value" data-rating-value>{{ ratingLabel }}</span>
            <label class="rating-unrestricted">
              <input id="rating-unrestricted" type="radio" name="minimum-rating" :value="null" v-model="selectedMinRating" aria-label="不限評分">
              <span>不限評分</span>
            </label>
          </div>
        </div>
      </fieldset>

      <div class="action-block">
        <button
          class="primary-action"
          data-action="recommend"
          type="button"
          :disabled="isBusy"
          @click="recommend"
        >
          <span>{{ isBusy ? '正在搜尋' : state === 'no-results' ? '再找一次' : '幫我決定' }}</span>
          <span class="action-detail">{{ `目前條件 · ${radiusLabel} · ${categoryLabel} · ${ratingLabel}` }}</span>
        </button>
      </div>

      <div
        :class="state === 'idle' ? undefined : ['state-panel', `state-panel--${state}`]"
        :data-state="state"
        role="status"
        aria-live="polite"
        aria-atomic="true"
      >
        <template v-if="state !== 'idle'">
          <span class="state-mark" aria-hidden="true"></span>
          <p class="state-copy">{{ stateText }}</p>
          <p v-if="error" class="state-detail">{{ error.message }}</p>
        </template>
      </div>

      <section v-if="state === 'no-results'" class="recovery-panel" data-state="no-results">
        <p class="recovery-radius">本次條件：<strong>{{ searchedRadiusLabel }} · {{ searchedCategoryLabel }} · {{ searchedRatingLabel }}</strong></p>
        <h2>本次還沒找到合適的店。</h2>
        <p v-if="submittedConditions?.restaurantCategory != null">可以改選其他餐廳類型，或選擇「不限類型」，再搜尋一次。</p>
        <p v-if="submittedConditions?.minRating != null">可以降低最低評分，或選擇「不限評分」，再搜尋一次。</p>
        <p v-if="searchedRadius < MAXIMUM_RADIUS_METERS">
          把搜尋距離調大，再按上方「再找一次」重新搜尋。
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
          <div>
            <dt>評分</dt>
            <dd class="place-rating">
              <svg v-if="place.rating != null" class="result-rating-star" viewBox="0 0 24 24" aria-hidden="true"><path :d="starPath" /></svg>
              <span data-place-rating>{{ place.rating ?? '尚無評分' }}</span>
            </dd>
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

  </main>
</template>
