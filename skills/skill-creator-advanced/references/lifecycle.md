# Skills 開發生命週期（端到端）

此文件提供一個可重複執行的 skills 開發流程（從需求到發布與迭代），適合作為 skill-creator-advanced 的第二層細節。

## Phase 0：需求與定位（先觀察，再追問）

先看現有上下文：
- 對話歷史
- 現有 skill / scripts / references
- 使用者已提過的限制、術語、輸出格式

判斷這個任務是否值得做成 skill：
- 適合：會重複發生、流程可重用、容易出錯、需要固定品質
- 不適合：一次性任務、沒有穩定 workflow、只差一個臨時 prompt

定位方式通常有兩類：
1) 問題導向（Problem-first）
- 使用者描述想完成什麼，skill 負責選工具、排順序、做檢查。
2) 工具導向（Tool-first）
- 使用者已經有某 MCP/工具，skill 教怎麼用得穩、快、少踩坑。

產出物（最少）：
- 2-3 個主要 use case
- in-scope / out-of-scope
- 對使用者熟悉語言的觀察筆記

## Phase 1：規格化 use cases

針對每個 use case，寫清楚：
- Trigger：使用者會怎麼說，包含明顯說法、近義句、口語改寫
- Inputs：需要的輸入、檔案類型、必填欄位、前置條件
- Steps：多步流程、依賴關係、validation gates
- Outputs：最終交付物（檔案、連結、報表、結構化結果）
- Done looks like：怎樣算完成，避免只寫「成功處理」

如果使用者沒有給例子，先提出一組合理 use cases 讓對方修正，通常比一連串抽象提問更有效。

## Phase 2：技能架構設計

關鍵決策：
- 是否需要 scripts：步驟重複、易出錯、需 deterministic
- 是否需要 references：長文規格、API docs、schema、style guide
- 是否需要 assets：模板、字體、icon、樣式檔、範本專案
- 是否有鄰近 skill：需要明確 overlap matrix、handoff 規則、negative triggers
- 是否需要 multilingual support：要不要為 zh / en / mixed 各自測試

核心原則：
- Progressive disclosure：前置欄位（name/description）決定何時載入；細節移到 body / references
- Composability：不要假設只會載入你這個 skill
- Least surprise：預設行為要符合一般使用者直覺，不要偷偷換目標
- Token discipline：只寫會改變行為的內容

## Phase 3：撰寫 SKILL.md（高影響）

### Description

重點放在 YAML frontmatter 的 `description`：
- 必須同時包含做什麼 + 何時用
- 用使用者真實語句、任務名稱、檔案類型、工具名稱
- 如有 over-trigger 風險，可加入 negative triggers
- 先讓 obvious queries 穩定命中，再處理少數邊角 case
- 若 skill 會和其他 skill 競爭，需把界線寫清楚，不要只堆砌關鍵詞
- 若使用者語料可能中英混用，要為 multilingual query 設計 wording

詳細調整流程見 `references/description-optimization.md`。

### Body

Body 建議結構：
- Quick start：最短成功路徑
- Workflow overview / decision tree：有分支就明寫
- Step-by-step：每步有成功條件與失敗處理
- Examples：輸入/輸出對照，盡量接近真實使用場景
- Troubleshooting：錯誤訊息 → 原因 → 解法

寫法原則：
- 指令用動詞開頭，避免模糊建議句
- 若某規則的理由能降低誤用，就把 why 補一句
- 不要把模型已知常識寫成長篇背景說明

## Phase 4：格式與結構檢查（上線前必做）

建議把驗證分成兩層：
1) **格式檢查（format_check.py）**：檔名、前置欄位、禁用檔案、常見格式問題。
2) **快速驗證（quick_validate.py）**：最小合規檢查。

這一步的目標不是求全，而是先排除低階結構錯誤，避免把時間浪費在壞掉的封裝上。

## Phase 5：測試（Trigger / Functional / Evals / Performance）

測試資料要盡量接近真實對話，而不是只用作者發明的乾淨 prompt。

- Triggering tests：應觸發 vs 不應觸發，包含 paraphrase、near-miss、模糊說法
- Multilingual tests：zh、en、mixed、縮寫、俗稱
- Overlap tests：與鄰近 skill 的界線案例
- Functional tests：Given/When/Then，覆蓋 happy path、edge cases、failure modes
- 將核准過的 prompt 存入 `assets/evals/evals.json`
- 為每輪建立 `<skill-name>-workspace/iteration-N/`
- 若環境支援 subagents / parallel workers，with-skill 與 baseline 應同回合啟動
- Performance comparison：與 baseline 比較，記錄對話輪次、tool calls、錯誤率、輸出品質
- Feedback loop：收集實際失敗案例與使用者回饋，再反推是 description、workflow、還是 resources 有問題
- ROI review：檢查提升是否值得額外成本與維護負擔
- Regression gates：定義沒有過門檻就不發布的規則

執行結果應盡量保存成可重看的 artifacts：
- outputs/
- grading.json
- timing.json
- benchmark.json
- review.html

## Phase 6：打包與發布

- 打包：`package_skill.py` 產生 `.skill`
- Repo：README、安裝說明、release notes 放在 skill folder 外
- Release：提供 `.skill` 或 zip，並附最短安裝/驗證方式
- 發布前至少重跑一次格式檢查與最小驗證

## Phase 7：迭代與維護

訊號 → 對策：
- Under-trigger：補 triggers、加入專有名詞、提到檔案類型
- Over-trigger：加 negative triggers、縮小範圍、刪除過度泛化 wording
- 執行不穩：增加 validation、把脆弱步驟放入 scripts
- 內容過大：把細節移到 references，保留導航與關鍵流程在 SKILL.md
- 使用者常抱怨結果不符預期：回頭檢查是否違反 least surprise 或 use case 定義太模糊
