# Testing playbook（觸發 / 功能 / 效能）

這份文件提供更具體的測試方法、判讀方式與迭代節奏。

## 1) Triggering tests

目標：確保 skill 在正確的時機自動載入，而且不會亂載入。

測試集建議：
- 8-10 個 `should-trigger`
  - 明顯說法
  - 近義/改寫
  - 真實口語句
  - 相關檔案類型或工具名稱
- 8-10 個 `should-not-trigger`
  - 完全無關
  - 近似但不在範圍內（near-miss）
  - 容易混淆但應該由別的 skill 接手

判定：
- 是否自動載入
- 是否只在很特定 wording 才會載入
- 是否因為 description 太寬而誤觸發

如果 should-trigger 常 miss，優先改 `description`，不要先往 body 加更多規則。
更細的 description 調整方式見 `references/description-optimization.md`。

## 1.5) Multilingual trigger tests

如果 skill 可能面對多語言或混合語料，請額外建立：
- `zh`：純中文、台灣常見口語、縮寫
- `en`：純英文、技術詞、常見簡寫
- `mixed`：中文句子 + 英文工具名 / 檔名 / 技術詞

檢查點：
- 只有英文才會觸發，中文不會
- 純中文會觸發，但 mixed prompt 失敗
- 工具名稱、檔案類型、deliverable 名稱在不同語言下是否仍穩定

## 1.6) Skill overlap tests

對每個 skill，至少列 2-3 個鄰近 skill 或容易混淆的任務類型。

對每個鄰近 skill 都要測：
- 這句 query 為什麼應該是我接
- 這句 query 為什麼應該是別的 skill 接
- 若兩邊都可能有用，該怎麼定主次

不要只寫「不相關」案例，因為真實 over-trigger 通常來自相鄰技能，而不是完全無關的句子。

## 2) Functional tests

目標：確保 skill 輸出正確、工具呼叫成功、錯誤處理可用。

用 Given/When/Then 寫測試案例：

範例：Create project with 5 tasks
- Given: project name、5 個 task 描述
- When: 執行 workflow
- Then:
  - project 已建立
  - 5 tasks 屬性正確
  - 無 API error

覆蓋面：
- Happy path：最常見使用流程
- Edge cases：空輸入、重複名稱、缺必填欄位
- Failure modes：MCP 連線失敗、rate limit、permission 不足
- Recovery path：失敗後是否有可執行的補救指引

測例來源優先順序：
1. 真實使用者需求
2. 近期失敗案例
3. 作者補出的邊界案例

## 3) Performance comparison

目標：證明 skill 相對 baseline 有明確改善，而不是只是多寫一堆規則。

建議紀錄：
- 對話輪次（messages）
- 工具呼叫數（tool calls）
- 失敗/重試（failed calls, retries）
- token（如可得）
- 達成任務的完整度
- 使用者是否還需要額外糾正

模板：
- Baseline（不開 skill）
  - 結果品質：
  - 成本：
  - 主要失敗點：
- With skill
  - 結果品質：
  - 成本：
  - 主要失敗點：

如果可以，請做盲比或至少在看不到版本資訊時評估輸出，避免作者偏見。

## 3.5) ROI review

除了比較誰比較好，也要比較值不值得：
- pass rate 提升多少
- 速度變慢多少
- token 增加多少
- 維護成本是否提高
- 是否需要額外 scripts / docs / 人工審查

常見判斷：
- 小幅品質提升，但 token / 時間暴增：未必值得
- 明顯降低錯誤率或人類糾正次數：通常值得
- 只有作者自己覺得變好，但 benchmark 與 reviewer 都看不出差異：通常不值得

## 3.6) Regression gates

在發版前先定門檻，而不是跑完 benchmark 再憑感覺決定。

常見 gate：
- with-skill pass rate 不得低於 baseline
- pass rate delta 至少要達某值
- time / token 增幅不得超標
- under-trigger / over-trigger 失敗數不得超過上限

建議把門檻寫成機械可判斷的設定，並用 `scripts/check_regression_gates.py` 驗證。

## 4) Operationalizing evals

當測試案例穩定後，不要只留在文件裡，應把它們正式化：
- 寫入 `assets/evals/evals.json`
- 用 `prepare_eval_workspace.py` 建立 iteration workspace
- 將 with-skill 與 baseline / old-skill 的輸出放回對應目錄
- 跑 `aggregate_benchmark.py` 產生 `benchmark.json` / `benchmark.md`
- 跑 `scripts/generate_review.py` 產生 `review.html`

若環境支援 subagents / parallel workers：
- 優先同一輪啟動 with-skill 與 baseline
- 不要先做完整個 with-skill 再回頭補 baseline

若環境不支援：
- 可以序列執行
- 但仍要維持 paired run 的資料夾結構，避免後續無法比較

## 5) 迭代回路

每次測試後至少回答三件事：
1. 這次失敗是觸發問題、流程問題，還是資源問題？
2. 若修正後重新測，失敗型態有沒有變少？
3. 這個 skill 是否真的比 baseline 好，還是只是更囉唆？

常見對策：
- 觸發問題：改 description、補真實 trigger phrases
- 流程問題：重寫步驟順序、加入 validation gate
- 資源問題：新增 scripts、拆 references、補模板或樣本

## 6) 自動化方向（可選）

- 互動式測試：最快，用來先找到大問題
- 腳本化測試：固定 prompt 集，適合回歸
- API evaluation suite：最系統化，適合持續 benchmark

建議先把一個難 case 做穩，再擴大 coverage；不要一開始就追求很大的測試量卻沒有明確判準。
