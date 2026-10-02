export const RESTAURANT_CATEGORY_OPTIONS = [
  { value: null, label: '不限類型' },
  { value: 'taiwanese-chinese', label: '台式／中式' },
  { value: 'japanese', label: '日式' },
  { value: 'korean', label: '韓式' },
  { value: 'hot-pot', label: '火鍋' },
  { value: 'barbecue', label: '燒烤' },
  { value: 'italian', label: '義式' },
  { value: 'breakfast-brunch', label: '早餐／早午餐' },
  { value: 'fast-food', label: '速食' },
  { value: 'vegetarian', label: '素食' },
  { value: 'cafe-dessert', label: '咖啡／甜點' },
] as const

export type RestaurantCategory = NonNullable<typeof RESTAURANT_CATEGORY_OPTIONS[number]['value']>

export function isValidRestaurantCategory(value: unknown): value is RestaurantCategory | null {
  return RESTAURANT_CATEGORY_OPTIONS.some(option => option.value === value)
}

export function formatRestaurantCategory(value: RestaurantCategory | null): string {
  return RESTAURANT_CATEGORY_OPTIONS.find(option => option.value === value)?.label ?? '無效餐廳類型'
}
