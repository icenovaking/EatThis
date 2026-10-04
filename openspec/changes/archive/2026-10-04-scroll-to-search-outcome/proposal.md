## Why

使用者希望按「幫我決定」後能直接看到查詢結果，已確認由查詢完成時自動捲至結果區，取代先前全程保持原本位置的偏好。

## What Changes

- 查詢成功且新餐廳完成渲染後，將餐廳卡片頂端帶入視窗，從店名開始閱讀。
- 查詢失敗或無結果後，將對應狀態提示區帶入視窗。
- 每次被接受的明確推薦操作完成後只發出一次捲動，不在定位、搜尋、條件編輯或一般重繪時重複觸發。
- 一般使用平滑捲動；偏好減少動態效果時立即定位，不移動鍵盤焦點。
- 搜尋期間沿用既有結果区高度保留機制，初始 idle 不預留空間，重試時不顯示可操作的舊餐廳。
- 以新的結果定位需求取代主規格中的 Preserve the viewport throughout recommendations，更新設計文件與驗收案例。

## Capabilities

### New Capabilities

無。

### Modified Capabilities

- `nearby-food-selection`: 推薦完成後自動定位成功卡片或失敗提示，取代全程禁止自動捲動的要求。

## Impact

- Affected specs: nearby-food-selection
- Modified:
  - `src/EatThis.Web/src/App.vue`
  - `src/EatThis.Web/src/app.spec.ts`
  - `src/EatThis.Web/src/styles.css`
  - `DESIGN.md`
  - `docs/ui/eatthis-mobile-web-direction.md`
  - `src/EatThis.Web/dist/index.html`
- 建置會更新 src/EatThis.Web/dist/assets 的雜湊資源。沒有 API、後端、資料模型或套件變更。
