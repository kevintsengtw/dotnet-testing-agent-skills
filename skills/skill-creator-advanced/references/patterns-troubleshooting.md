# Patterns & troubleshooting

此文件提供常見 skill 設計 pattern，以及可直接使用的 troubleshooting 診斷矩陣。

## Design patterns

### Pattern 1：Sequential workflow orchestration

適用：
- 流程必須照順序跑
- 每步有依賴關係
- 跳步會造成後續失敗

關鍵：
- 清楚的 step ordering
- 每步都有 validation gate
- 失敗時有 rollback / retry 指引

### Pattern 2：Multi-MCP coordination

適用：
- 跨多個服務協作，例如 Figma → Drive → Linear → Slack
- 需要在不同工具間傳遞 IDs、links、artifacts

關鍵：
- Phase 分段
- 明確資料傳遞格式
- 集中 error handling

### Pattern 3：Iterative refinement

適用：
- 第一次輸出通常不夠好
- 需要檢查 → 修正 → 再檢查的迴圈

關鍵：
- 明確 quality criteria
- 用 scripts 做 deterministic validation
- 定義停止條件

### Pattern 4：Context-aware tool selection

適用：
- 同一 outcome 可能有多種工具路徑
- 工具選擇依檔案大小、格式、權限、協作需求而定

關鍵：
- Decision tree
- fallback
- 對使用者透明說明取捨

### Pattern 5：Domain-specific intelligence

適用：
- skill 的價值在合規、風險、業務規則或專業判準

關鍵：
- 先檢查再動作
- 留 audit trail
- 清楚治理與例外處理

### Pattern 6：Trigger-optimization loop

適用：
- skill 是否會正確載入，是主要風險之一
- 需要持續修正 under-trigger 或 over-trigger

關鍵：
- 維護 should-trigger / should-not-trigger 測試集
- 先修 `description`，再考慮 body
- 用真實使用者語句，不要只用作者自造 prompt

### Pattern 7：Evaluation-driven iteration

適用：
- 你想證明這個 skill 比 baseline 更好
- 問題不是「有沒有做」，而是「是否真的改善」

關鍵：
- 比較 baseline vs with-skill
- 記錄失敗型態，而不只記錄成功率
- 每次迭代只改少數變數，避免無法定位效果來源

## Troubleshooting workflow

遇到問題時，先按這個順序判斷：

1) 是「沒載入 skill」還是「skill 載入後執行失敗」？
2) 若已載入，是 description 問題、workflow 問題，還是 resources 問題？
3) 問題是否能用最小測例穩定重現？
4) 修正後是否有用同一組測例重跑？

不要在還沒分清問題類型前，同時亂改 description、body、scripts。那會讓你不知道哪個修正真的有效。

## Diagnostic matrix

### Skill 無法上傳：找不到 SKILL.md

- Symptoms：
  - 上傳或驗證時顯示找不到 `SKILL.md`
- Likely causes：
  - 檔名不是精確的 `SKILL.md`
  - 放錯資料夾層級
- First checks：
  1. 確認檔名大小寫完全正確
  2. 確認檔案位於 skill root
- Fix：
  - 改名為 `SKILL.md`
  - 移回 skill root

### Invalid frontmatter

- Symptoms：
  - 驗證時出現 YAML 或 frontmatter 格式錯誤
- Likely causes：
  - 少 `---`
  - YAML 缩排錯誤
  - 欄位型別錯誤
  - `description` 中有不允許字元
- First checks：
  1. 跑 `format_check.py`
  2. 跑 `quick_validate.py`
- Fix：
  - 修正 frontmatter 結構與欄位型別

### Invalid skill name

- Symptoms：
  - 驗證失敗，指出名稱格式不合法
- Likely causes：
  - 有空格、大寫、底線
  - 開頭或結尾是連字號
- First checks：
  1. 檢查 folder name
  2. 檢查 frontmatter 的 `name`
- Fix：
  - 改成 kebab-case

### Skill 不會觸發（under-trigger）

- Symptoms：
  - 明明相關 query 卻沒載入
  - 使用者要手動指定 skill
  - 只在非常特定 wording 才會成功
- Likely causes：
  - `description` 太抽象
  - 缺少真實 trigger phrases
  - 沒寫常見任務名稱、檔案類型、工具名詞
  - skill 範圍寫得太學術，不像使用者真實說法
- First checks：
  1. 用 should-trigger 測試句重跑
  2. 比對失敗句是否只是改寫或口語句
  3. 檢查 `description` 是否只有「做什麼」卻沒寫「何時用」
- First fixes：
  - 補使用者常說的動詞與任務名稱
  - 補檔案類型、工具名稱、工作情境
  - 把抽象分類詞換成真實 query 語言
- Deeper fixes：
  - 參考 `references/description-optimization.md` 重寫整段 `description`
  - 重新建立 should-trigger / should-not-trigger 測試集

### Skill 觸發太頻繁（over-trigger）

- Symptoms：
  - 不相關 query 也載入
  - 只要碰到廣義關鍵字就誤觸發
  - 使用者常關閉或抱怨被搶流程
- Likely causes：
  - `description` 寫得太寬
  - 使用了「通用」「任何」「所有」等過度泛化詞
  - 缺少 negative triggers
- First checks：
  1. 用 should-not-trigger 測試句重跑
  2. 找出是哪個詞讓不相關 query 也命中
  3. 看 `description` 是否混入多個不同 use cases
- First fixes：
  - 縮小 skill 範圍
  - 拿掉過度泛化字眼
  - 補 negative triggers，例如不適用於哪些情況
- Deeper fixes：
  - 若任務其實是兩種 workflow，拆成兩個 skill
  - 回頭重寫 in-scope / out-of-scope

### Skill 有載入，但步驟常漏做

- Symptoms：
  - 某些步驟時常被跳過
  - 順序不穩
  - 一遇到多步流程就開始遺漏 validation
- Likely causes：
  - 指令太散、太長，關鍵步驟埋太深
  - workflow 沒有明確 phase 或 gate
  - 同一步混了太多子任務
- First checks：
  1. 找出最常被漏掉的是哪一步
  2. 看該步驟是否有獨立標題與完成條件
  3. 看是否需要拆成 sequential workflow
- Fix：
  - 把 critical instructions 提前
  - 加 step ordering 與 validation gate
  - 把多個隱含步驟拆開寫

### 指令不被遵守 / 輸出不穩

- Symptoms：
  - 同一任務輸出風格飄動很大
  - 使用者需要反覆糾正
  - 回答看似合理，但和 skill 目標不一致
- Likely causes：
  - 指令語句含糊
  - 缺少成功條件或停止條件
  - deterministic 工作交給自然語言臨場發揮
- First checks：
  1. 檢查每個主要步驟是否有 success criteria
  2. 檢查是否把格式驗證、欄位驗證、名稱正規化留給模型猜
  3. 看是否違反 least surprise
- Fix：
  - 改成明確動詞指令
  - 補成功條件與失敗處理
  - 把 deterministic 檢查移到 scripts

### MCP 連線 / 工具呼叫失敗

- Symptoms：
  - skill 已載入，但工具呼叫報錯
  - 結果卡在某個外部服務
- Likely causes：
  - MCP server 未連線
  - 權限或憑證問題
  - 工具名稱錯誤
  - skill 把工具前提假設寫得太樂觀
- First checks：
  1. 確認 MCP server 已連線
  2. 檢查權限 / 憑證 / token
  3. 不靠 skill，獨立測試該工具呼叫
  4. 確認工具名稱與參數格式
- Fix：
  - 在 skill 中加入 preflight checks
  - 對常見 failure mode 給明確 fallback 或回報格式

### Context 太大造成品質下降

- Symptoms：
  - 輸出變慢且不穩
  - 重要指令被忽略
  - 回答混入不相關細節
- Likely causes：
  - `SKILL.md` 塞太多背景知識
  - references 沒拆分，導致每次都讀太多
  - 一次啟用太多 skills
- First checks：
  1. 檢查 `SKILL.md` 是否可只保留流程與導航
  2. 檢查 references 是否可按主題拆分
  3. 檢查是否有重複說明
- Fix：
  - `SKILL.md` 瘦身
  - 細節下放到 `references/`
  - 控制同時啟用的 skills 數量

### 測試看起來都過，但真實使用仍不穩

- Symptoms：
  - 作者自測感覺不錯，使用者卻常失敗
  - benchmark 漂亮，但落地抱怨很多
- Likely causes：
  - 測試句太乾淨，不像真實 query
  - 只測 happy path
  - 測試與 baseline 比較方式偏心
- First checks：
  1. 測試集是否包含真實使用者說法
  2. 是否有 near-miss、edge case、failure mode
  3. 是否記錄使用者需要額外糾正的次數
- Fix：
  - 用真實失敗案例更新測試集
  - 重新做 baseline vs with-skill 比較
  - 每次只改少數變數再重測

## 快速判斷：到底該改哪裡？

- 主要是「沒載入」：先改 `description`
- 主要是「有載入但做錯」：先改 workflow / instructions
- 主要是「有載入也知道要做什麼，但細節常錯」：先補 scripts 或 validation
- 主要是「越修越亂」：先縮 scope，必要時拆 skill

## 何時應該拆 skill

有下列訊號時，不要再硬撐單一 skill：
- 同一份 `description` 無法同時避免 under-trigger 和 over-trigger
- 兩組 use cases 幾乎沒有共用 workflow
- 一組使用者要高自由度，另一組需要強約束
- 同一 skill 同時扮演工具教學、任務執行、格式轉換三種角色

## 參考搭配

- `references/description-optimization.md`
- `references/testing-playbook.md`
- `references/lifecycle.md`
