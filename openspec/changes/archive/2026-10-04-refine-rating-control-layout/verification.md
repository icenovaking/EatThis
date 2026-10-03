# 驗證紀錄

日期：2026-10-04。範圍：refine-rating-control-layout 前端 UI。

## 自動驗證

工作目錄：F:/Website/EatThis/src/EatThis.Web。

- RED：npm run test:run -- src/app.spec.ts，新增文案測試因「附近的城市飲食指南」仍存在而失敗；25 通過、1 失敗。
- GREEN：修改後同一指令 26 項全部通過。
- Mutation：暫時替換 action-note 文案及將 aria-describedby 指向不存在的 ID，兩個新增測試各自失敗。已在 finally 還原，沒有保留任何 mutation。
- 最終 npm run test:run：4 個檔案、105 項全部通過。
- npm run typecheck：通過。
- npm run build：通過；dist 由 Vite 產生，未手動修改產物。
- 專案根目錄 git diff --check：通過。

| 合約 | 證據 |
| --- | --- |
| 五段指定文案移除，第 4 項文字保留 | removes redundant search guidance while preserving action and status copy |
| 群組名稱及 ARIA 描述引用仍有效 | keeps named filter groups and resolves every descriptive reference |
| 十個半星選項、重設及控制項無定位／API 副作用 | offers ten accessible half-star choices and an unrestricted default without side effects |
| 搜尋及錯誤、類型與提交快照 | 原有 App 與 useNearbyFood 測試，包含在 105 項通過結果 |

## 視覺與輸入裝置驗證

瀏覽器工具回報 No browser is available；依疑難排解查詢瀏覽器清單為空。沒有進行實際瀏覽器、手機或螢幕閱讀器驗證。

以下不以 DOM 測試或建置結果代替：

- 320、390、768、1280 CSS px 下，null、0.5、4.5、5 狀態的無水平溢出與完整點擊範圍。
- 評分文字盒與不限評分按鈕外框左右邊界誤差不超過 1 CSS px，窄螢幕右側整組換行。
- 手機觸控後沒有半星外框殘留，滑鼠 hover 不畫半星矩形。
- 真實鍵盤 radio 導覽與焦點外框可見。

CSS 幾何及 hover 屬單一介面的視覺屬性，依 TDD Test scope 採 Excluded workflow，沒有新增 CSS 字串斷言。程式已使用共用 intrinsic-width grid 欄位、flex wrapping 與既有星尺寸 token；實際視覺驗收仍待完成，任務 2.3 保持未勾選。任務 2.2 依其「無瀏覽器時明記未驗證」條款記錄完成實作及 DOM 回歸，觸控與鍵盤驗證仍列於此處。

## 範圍檢查

只變更 App 模板、樣式、App 測試、兩份設計文件及前端建置產物。未修改 API、搜尋流程、套件或秘密設定。工作目錄無 CodeGraph 索引，依專案無索引例外直接檢視相關來源與引用。未提交、部署或封存本變更。
