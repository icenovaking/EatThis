# 驗證紀錄

日期：2026-10-04。

## 自動驗證

工作目錄：F:/Website/EatThis/src/EatThis.Web。

- RED：npm run test:run -- src/app.spec.ts，4 項預期失敗、25 項通過。原因為 action-note 仍存在與 idle 狀態區仍有準備好了文案。
- GREEN：相同指令 29 項全部通過。
- npm run test:run：4 檔案、108 項通過。
- npm run typecheck：通過。
- npm run build：通過，dist 僅由 Vite 建置產生。

| 合約 | 測試證據 |
| --- | --- |
| 初始無靜態下方文案、頁尾與狀態裝飾，保留目前條件及空 live region | keeps the idle surface quiet while preserving the current conditions |
| 改成 700 公尺、日式、4.5 星仍不定位或呼叫 API | keeps pending edits quiet without location or API requests |
| 同一 live region 經 idle、locating、searching、selected 更新，busy 禁用重複提交，結果 attribution 保留 | updates the same live region from idle through locating searching and selected |
| 不支援定位時宣告失敗並提供復原提示 | announces unsupported location with recovery in the existing live region |
| permission-denied | renders an announced permission state without sending an API request |
| no-results | describes submitted conditions after no results while preserving later pending controls |
| provider-error | renders provider errors without exposing upstream details |
| rate-limited | renders rate-limit retry guidance with the server-provided wait time |

新增案例皆先因原本 idle 內容而失敗，修改後通過；既有非 idle 回歸沿用。單一介面的間距與分隔線外觀依 TDD Test scope 採視覺驗收，不以 CSS 字串斷言模擬瀏覽器結果。

## 待驗事項

本次重新查詢瀏覽器清單為空，未進行實際瀏覽器或輔助科技驗證。任務 3.3 保持未完成：320、390、768 CSS px 的底部留白、無占位、搜尋後回饋，以及螢幕閱讀器對空 live region 更新的宣告仍待驗。自動測試僅確認 DOM 行為，不能代替上述驗收。

## 範圍

變更限 App 模板、相關 CSS、App 測試、設計文件及前端產物；搜尋流程、API、套件與後端未變動。無 CodeGraph 索引，依專案例外直接檢視來源。設計 hook 提示既有 .impeccable/design.json 比 DESIGN.md 舊，本次未額外重建快取。未提交、部署或封存。
