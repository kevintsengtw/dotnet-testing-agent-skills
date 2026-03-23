---
name: skill-creator-advanced
description: 當使用者要建立、改版、測試、評估或發布 skill 時使用。涵蓋 description 優化、evals、benchmark、邊界管理與打包流程。
version: 2026.3.9
license: Complete terms in LICENSE.txt
metadata:
  author: Allan Yiin
  short-description: Skill 建立、評估、benchmark 與打包迭代流程
  openclaw:
    emoji: "🛠️"
---

# Skill Creator Advanced

此 skill 的目標是把「做 skill」變成可重複執行的工程流程，而不是一次性的 prompt 雜談。

它同時提供：
- 可操作的流程：從需求、設計、驗證、evals、benchmark、打包到迭代
- 可重用的腳本：初始化、格式檢查、驗證、測試計畫產生、workspace 準備、benchmark 彙整、regression gate 檢查、打包
- 可拆分的參考文件：把長內容放到 references/，維持 progressive disclosure
- 輕量 review viewer：把 with-skill / baseline 結果整理成可檢閱的 HTML

## 快速開始（你只要做一個新 skill）

1) 先從現有對話、repo、範例任務整理 2-3 個 use cases，不夠再補問。
2) 為每個 use case 寫 trigger 語句與 done looks like。
3) 建立 skill 資料夾：

```bash
python scripts/init_skill_advanced.py <skill-name> --path <output-dir>
```

4) 補完新 skill 的 `SKILL.md`，優先寫對 YAML 的 `description`。
5) 做格式與結構檢查：

```bash
python scripts/format_check.py <path/to/skill>
python scripts/quick_validate.py <path/to/skill>
```

6) 規劃真實測試案例，必要時產生測試計畫：

```bash
python scripts/generate_test_plan.py <path/to/skill> --out references/test_plan.md
```

7) 準備 eval workspace，讓 with-skill / baseline 能沿用固定目錄結構：

```bash
python scripts/prepare_eval_workspace.py <path/to/skill>
```

8) 打包成 `.skill`：

```bash
python scripts/package_skill.py <path/to/skill> <output-dir>
```

9) 若要優化 description 的觸發品質，另外準備 trigger eval set，再跑：

```bash
python scripts/run_eval.py --eval-set <path/to/trigger-evals.json> --skill-path <path/to/skill> --model <model-id>
python scripts/run_loop.py --eval-set <path/to/trigger-evals.json> --skill-path <path/to/skill> --model <model-id> --apply-best
```

## 操作方式

當使用者要建立或改版 skill 時，請用下列順序推進；可以跳步，但要明確說明原因。

1) Phase 0：從上下文萃取需求
- 先看對話歷史、現有檔案、既有流程，再決定要不要追問。
- 用使用者熟悉的術語溝通；如果對方不熟技術名詞，不要把 jargon 當前提。
- 先判斷是否真的值得做成 skill：如果問題一次性、沒有可重用流程，應直接指出不值得包成 skill。

2) Phase 1：需求與 use cases
- 先拿到 2-3 個具體 use cases。
- 每個 use case 至少要有：trigger 語句、必要輸入、主要步驟、輸出、done looks like。
- 若使用者只給模糊目標，應主動提出一組合理 use cases 讓對方確認。

3) Phase 2：架構與 SKILL.md
- 決定哪些內容要放 `scripts/`、`references/`、`assets/`。
- `description` 必須同時回答兩件事：這個 skill 做什麼、什麼情況下應該觸發。
- `description` 要用真實使用者語句，而不是作者自嗨式分類。
- 核心流程留在 `SKILL.md`，細節與變體移到 `references/`。

4) Phase 3：撰寫指令
- 優先寫會改變行為的指令，不要解釋模型本來就知道的常識。
- 能直接下命令就直接下命令；只有在理由能降低誤用時才補一句 why。
- 預設遵守 least surprise：讓 skill 的行為符合一般使用者直覺，不要偷偷改目標。

5) Phase 4：格式檢查與最小合規驗證
- 先跑 `format_check.py` 修掉結構與格式問題。
- 再跑 `quick_validate.py` 做最小合規確認。

6) Phase 5：測試、evals 與 benchmark
- Triggering tests：應觸發、近義改寫、near-miss、不應觸發。
- Multilingual tests：至少考慮 `zh`、`en`、`mixed`、縮寫/俗稱。
- Skill overlap tests：列出容易混淆的鄰近 skill 與 negative triggers。
- Functional tests：Given/When/Then，至少含 happy path、edge case、failure mode。
- 把核准過的測試 prompt 寫進 `assets/evals/evals.json`。
- 先建立 `<skill-name>-workspace/iteration-N/`，每個 eval 各自有 `with_skill/` 與 baseline 目錄。
- 若環境支援 subagents 或平行 workers，應在同一輪啟動 with-skill 與 baseline/old-skill；不支援時可序列執行，但保留相同目錄結構。
- Performance comparison：和 baseline 比較輪次、tool calls、失敗率、結果品質。
- ROI comparison：確認提升是否值得額外的 token、時間與維護成本。
- 執行後用 `scripts/aggregate_benchmark.py` 彙整 benchmark，再用 `scripts/generate_review.py` 產生 review viewer。
- 用 `scripts/check_regression_gates.py` 檢查是否達到發版門檻。
- 測試用語要接近真實使用者會講的話，不要只測教科書式 prompt。

7) Phase 6：打包與發布
- 用 `package_skill.py` 產生 `.skill`。
- 分享時，README、安裝說明、release notes 應放在 skill folder 外。

8) Phase 7：迭代與維護
- Under-trigger：補真實 trigger phrases、專有名詞、檔案類型。
- Over-trigger：加入 negative triggers、縮小範圍、移除模糊字眼。
- 執行不穩：補 validation、把脆弱步驟搬到 scripts。
- 內容過大：縮短 SKILL.md，把細節下放到 references。
- 迭代時優先收集具體失敗案例與使用者回饋，不要只憑感覺改 wording。

完整細節見：
- `references/lifecycle.md`
- `references/testing-playbook.md`
- `references/description-optimization.md`
- `references/eval-workflow.md`
- `references/eval-schemas.md`
- `references/multilingual-trigger-strategy.md`
- `references/skill-boundary-management.md`
- `references/regression-gates.md`
- `references/skill-roi-model.md`
- `references/distribution-playbook.md`
- `references/patterns-troubleshooting.md`

## 核心規則（請強制遵守）

1) **先把 description 寫對**
- 這是 skill 是否會被載入的主要因素。
- description 內要包含真實 trigger phrases、工作情境、必要時的檔案類型。
- 優先讓明顯 query 穩定命中，再處理邊角案例；不要為了少數怪句子把 description 寫得過寬。

2) **先從上下文學會，再提最少的問題**
- 先讀對話、檔案與現有 skill。
- 只有在高風險假設會害結果偏掉時，才追問使用者。

3) **把脆弱步驟移到 scripts**
- 只要是重複、易出錯、或需要 deterministic 的檢查/轉換，就寫成腳本。

4) **避免 context 膨脹**
- `SKILL.md` 放流程與導航。
- 細節放 `references/`，必要時再讀。

5) **測試要真實，不要只測漂亮案例**
- 用接近實際對話的 prompt。
- 比較 baseline，確認 skill 真的有幫助，而不是只是多了一堆指令。

6) **with-skill 與 baseline 要用同一批 evals 比**
- 盡量同一輪啟動，避免時間與上下文條件差太多。
- 若是改版既有 skill，baseline 應是舊版 skill snapshot，而不是「完全不用 skill」。

7) **先處理 skill 邊界，再處理 wording**
- 若多個 skill 搶同一類 query，先做 overlap matrix 與 in-scope / out-of-scope。
- 不要只靠把 description 寫得更長來硬解衝突。

8) **ROI 不成立的 skill 不值得硬留**
- 若提升太小、成本太高、維護太重，要直接考慮縮 scope、拆 skill，或退回一般 prompt。

9) **不要在 skill folder 放 README.md**
- README 是給人看的，應放在 repo root 或其他 skill folder 外的位置。

## 寫作與設計準則

- 用使用者懂的語言描述，不要預設對方知道你的內部名詞。
- 指令優先用明確動詞開頭，例如「先檢查」「若失敗就停止並回報」。
- 當某一步驟的理由能防止錯誤時，把理由寫出來；否則保持精簡。
- 技能不該偷偷改任務。若 workflow 需要做取捨，應明示取捨原則。
- 若某個任務其實不該做成 skill，要直接指出原因，而不是硬湊內容。

## 你可以用的腳本

- `scripts/init_skill_advanced.py`：建立帶測試/發布欄位的 SKILL.md 骨架。
- `scripts/format_check.py`：格式與結構檢查器（含 `--fix`）。
- `scripts/quick_validate.py`：最小合規驗證。
- `scripts/generate_test_plan.py`：產生測試計畫模板。
- `scripts/prepare_eval_workspace.py`：從 `assets/evals/evals.json` 建立 iteration workspace。
- `scripts/aggregate_benchmark.py`：彙整 with-skill / baseline run 結果，輸出 `benchmark.json` 與 `benchmark.md`。
- `scripts/check_regression_gates.py`：依 benchmark 與門檻設定判斷是否可發版。
- `scripts/run_eval.py`：跑 description trigger eval，輸出 query-level 與 run-level 診斷結果。
- `scripts/improve_description.py`：依 trigger eval 失敗型態重寫 description，保留 transcript。
- `scripts/run_loop.py`：把 eval 與 description 改寫串成多輪迭代，可選擇直接套用最佳 description。
- `scripts/generate_report.py`：產生 description optimization 的 HTML 報告。
- `scripts/utils.py`：共用的 `SKILL.md` / JSON 讀寫輔助。
- `scripts/package_skill.py`：驗證後打包成 `.skill`。

## 你可以用的 viewer / eval 結構

- `assets/evals/evals.json`：保存真實測試 prompt、預期輸出與 expectations。
- `assets/evals/regression_gates.json`：保存 benchmark 的發版門檻設定。
- `scripts/generate_review.py`：把 workspace 結果輸出成 review HTML。
- `<skill-name>-workspace/iteration-N/`：保存每輪 with-skill / baseline 的輸出、grading 與 benchmark。

## 常見交付物

交付給使用者時，通常包含：
- skill folder（`SKILL.md` + `scripts/` + `references/` + `assets/`）
- `assets/evals/evals.json`
- `<skill-name>-workspace/iteration-N/` 的 benchmark 與 review 輸出
- `.skill` 打包檔
- 放在 skill folder 外的 README、示例、release notes、安裝說明
