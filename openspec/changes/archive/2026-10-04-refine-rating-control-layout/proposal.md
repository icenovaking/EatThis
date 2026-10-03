## Summary

精簡搜尋頁指定的五段文案，修正半星外框及最低評分右側欄位對齊，保留主按鈕與下方所有既有文字。

## Motivation

手機截圖顯示第五顆星左半出現方框，「不限評分」與上方評分文字未對齊。頁首與控制項的重複說明增加閱讀負擔；使用者已明確取消刪除主按鈕下方文字的建議。

## Proposed Solution

- 移除半星 hover 外框，維持原生 radio 的半星點選及鍵盤可見焦點。
- 評分文字與「不限評分」共用靠右欄位，按鈕外框與文字欄等寬；空間不足時整組換行。
- 移除「附近的城市飲食指南」、「今日附近」、「先決定你願意走多遠，EatThis 只在你按下按鈕後取一次位置，替你選一間。」、「選一種想吃的類型，或交給我們決定」、「點星星左半選半星，右半選整星」。
- 保留按鈕內目前條件、按鈕外定位說明、初始狀態提示、操作中及錯誤回饋、頁尾文字和結果資訊。

## Capabilities

### New Capabilities

無。

### Modified Capabilities

- `nearby-food-selection`: 增加精簡文案、評分對齊、觸控無半星外框及保留回饋的展示要求。

## Impact

- Affected specs: `openspec/specs/nearby-food-selection/spec.md`
- Affected code:
  - Modified: `src/EatThis.Web/src/App.vue`, `src/EatThis.Web/src/styles.css`, `src/EatThis.Web/src/app.spec.ts`
- 設計文件同步：`DESIGN.md`, `docs/ui/eatthis-mobile-web-direction.md`
- 前端建置可能更新已追蹤的 `src/EatThis.Web/dist/` 產物。
- 無 API、套件、資料儲存或後端變更。
