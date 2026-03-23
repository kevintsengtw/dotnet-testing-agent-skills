# Eval schemas

這份文件整理 `skill-creator-advanced` 的 eval / benchmark / viewer 相關結構。欄位設計參考 upstream `skill-creator`，但保留本地實作可落地的最小集合。

## 1) `assets/evals/evals.json`

位置：
- `skill-folder/assets/evals/evals.json`

用途：
- 保存核准過的 eval prompts
- 作為 workspace scaffold 的輸入

範例：

```json
{
  "skill_name": "example-skill",
  "evals": [
    {
      "id": 1,
      "name": "new-skill-draft",
      "prompt": "Help me create a new skill for rotating PDFs.",
      "expected_output": "A draft SKILL.md plus recommended scripts and references.",
      "files": [],
      "expectations": [
        "The response defines 2-3 use cases.",
        "The response proposes at least one deterministic script."
      ]
    }
  ]
}
```

欄位：
- `skill_name`: 應與 skill frontmatter 的 `name` 對應
- `evals[].id`: 唯一整數 ID
- `evals[].name`: 人類可讀名稱，會用在 workspace 目錄名
- `evals[].prompt`: 測試 prompt
- `evals[].expected_output`: 成功結果的自然語言描述
- `evals[].files`: 相對於 skill root 的輸入檔列表
- `evals[].expectations`: 可驗證的期望列表

## 2) `eval_metadata.json`

位置：
- `<workspace>/iteration-N/<eval-dir>/eval_metadata.json`

用途：
- 保存單一 eval 的 metadata
- 供 review viewer 與 benchmark 聚合器讀取

範例：

```json
{
  "eval_id": 1,
  "eval_name": "new-skill-draft",
  "prompt": "Help me create a new skill for rotating PDFs.",
  "expected_output": "A draft SKILL.md plus recommended scripts and references.",
  "files": [],
  "expectations": [
    "The response defines 2-3 use cases.",
    "The response proposes at least one deterministic script."
  ]
}
```

## 3) `grading.json`

位置：
- `<run-dir>/grading.json`

用途：
- 保存 grader 或人工審查後的結構化結果

本地工具接受的最小結構：

```json
{
  "expectations": [
    {
      "text": "The response defines 2-3 use cases.",
      "passed": true,
      "evidence": "Section 'Primary use cases' contains 3 entries."
    }
  ],
  "summary": {
    "passed": 1,
    "failed": 0,
    "total": 1,
    "pass_rate": 1.0
  },
  "execution_metrics": {
    "total_tool_calls": 8,
    "errors_encountered": 0
  },
  "timing": {
    "total_duration_seconds": 42.5
  },
  "user_notes_summary": {
    "uncertainties": [],
    "needs_review": [],
    "workarounds": []
  }
}
```

## 4) `outputs/metrics.json`

位置：
- `<run-dir>/outputs/metrics.json`

用途：
- 保存 executor 的工具與輸出統計

範例：

```json
{
  "tool_calls": {
    "Read": 5,
    "Write": 2
  },
  "total_tool_calls": 7,
  "total_steps": 4,
  "files_created": [
    "draft-skill.md"
  ],
  "errors_encountered": 0,
  "output_chars": 2800,
  "transcript_chars": 1900
}
```

## 5) `timing.json`

位置：
- `<run-dir>/timing.json`

用途：
- 保存 run 的 wall clock time 與 token 資訊

範例：

```json
{
  "total_tokens": 12000,
  "duration_ms": 23332,
  "total_duration_seconds": 23.3
}
```

## 6) `benchmark.json`

位置：
- `<iteration-dir>/benchmark.json`

用途：
- 聚合多個 eval / config / run 的統計結果
- 給 review viewer 與 benchmark markdown 使用

範例：

```json
{
  "metadata": {
    "skill_name": "example-skill",
    "skill_path": "/path/to/skill",
    "timestamp": "2026-03-07T12:00:00Z",
    "evals_run": [1, 2],
    "runs_per_configuration": 1
  },
  "runs": [
    {
      "eval_id": 1,
      "eval_name": "new-skill-draft",
      "configuration": "with_skill",
      "run_number": 1,
      "result": {
        "pass_rate": 1.0,
        "passed": 2,
        "failed": 0,
        "total": 2,
        "time_seconds": 42.5,
        "tokens": 12000,
        "tool_calls": 8,
        "errors": 0
      },
      "expectations": [],
      "notes": []
    }
  ],
  "run_summary": {
    "with_skill": {
      "pass_rate": {"mean": 1.0, "stddev": 0.0, "min": 1.0, "max": 1.0},
      "time_seconds": {"mean": 42.5, "stddev": 0.0, "min": 42.5, "max": 42.5},
      "tokens": {"mean": 12000, "stddev": 0.0, "min": 12000, "max": 12000}
    },
    "without_skill": {
      "pass_rate": {"mean": 0.5, "stddev": 0.0, "min": 0.5, "max": 0.5},
      "time_seconds": {"mean": 31.0, "stddev": 0.0, "min": 31.0, "max": 31.0},
      "tokens": {"mean": 8000, "stddev": 0.0, "min": 8000, "max": 8000}
    },
    "delta": {
      "pass_rate": "+0.50",
      "time_seconds": "+11.5",
      "tokens": "+4000"
    }
  },
  "notes": []
}
```

## 7) viewer 輸入

`scripts/generate_review.py` 會讀：
- `eval_metadata.json`
- 各 config 的 `outputs/`
- 各 config 的 `grading.json`
- 同層 `benchmark.json`（如果有）

因此最少要有：
- `eval_metadata.json`
- `with_skill/outputs/`
- baseline config 的 `outputs/`

沒有 `grading.json` 也能產生 viewer，但比較資訊會變少。
