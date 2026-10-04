## Summary

精簡初始搜尋頁，移除主按鈕下方靜態說明、準備好了提示及頁尾，只在使用者搜尋後呈現操作回饋。

## Motivation

使用者在「去除文字.png」框選主按鈕以下區塊，認為重複說明沒有必要。這次需求取代先前保留該區塊的決定，讓初始畫面在主按鈕結束。

## Proposed Solution

- 移除「按下後才會使用目前位置；結果會交給 Google Maps 開啟路線。」及兩段頁尾「GPS 只在你要求時使用」、「外部導覽」。
- idle 時不顯示「準備好了…」狀態區塊、圖示、分隔線或占位空白。
- 保留主按鈕內目前條件，以及搜尋後定位、查詢、成功、錯誤與復原回饋、餐廳結果及 Google Maps attribution。
- 保留既有半星與餐廳類型互動；更新相關測試及設計說明。

## Capabilities

### New Capabilities

無。

### Modified Capabilities

- `nearby-food-selection`: 修改 Present concise search guidance while retaining action feedback，移除靜態下方文案並依搜尋狀態呈現回饋。

## Impact

- Affected specs: `openspec/specs/nearby-food-selection/spec.md`
- Modified: `src/EatThis.Web/src/App.vue`, `src/EatThis.Web/src/styles.css`, `src/EatThis.Web/src/app.spec.ts`, `DESIGN.md`, `docs/ui/eatthis-mobile-web-direction.md`
- 建置更新已追蹤的 `src/EatThis.Web/dist/` 產物。
- 無後端、API、依賴套件或儲存格式變更；不修改既有封存文件。
