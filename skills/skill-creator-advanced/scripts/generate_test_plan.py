#!/usr/bin/env python3
"""Generate a skill test plan scaffold.

This is a convenience tool for producing a repeatable testing checklist and a
starter set of test cases.

Inputs:
  - skill directory path (must contain SKILL.md)

Outputs:
  - Prints a Markdown test plan to stdout, or writes to a file if --out is set.

Usage:
  ./generate_test_plan.py <path/to/skill>
  ./generate_test_plan.py <path/to/skill> --out references/test_plan.md

Notes:
  - This script does not execute tests; it creates a structured plan.
"""

from __future__ import annotations

import argparse
import re
from pathlib import Path

import yaml


def extract_frontmatter(skill_md_text: str) -> dict:
    lines = skill_md_text.splitlines()
    if not lines or lines[0].strip() != "---":
        raise ValueError("SKILL.md must start with YAML frontmatter delimiter '---'")

    end = None
    for i in range(1, len(lines)):
        if lines[i].strip() == "---":
            end = i
            break
    if end is None:
        raise ValueError("Could not find closing frontmatter delimiter '---'")

    fm_text = "\n".join(lines[1:end]) + "\n"
    fm = yaml.safe_load(fm_text)
    if not isinstance(fm, dict):
        raise ValueError("Frontmatter must be a YAML mapping")
    return fm


def guess_trigger_phrases(description: str) -> list[str]:
    # Collect quoted phrases from common English and CJK quote styles.
    patterns = [
        r"\"([^\"]{2,60})\"",
        r"「([^」]{2,60})」",
        r"『([^』]{2,60})』",
        r"'([^']{2,60})'",
    ]
    out: list[str] = []
    for pattern in patterns:
        for q in re.findall(pattern, description):
            q = q.strip()
            if q and q not in out:
                out.append(q)

    # Add a few keyword-like fragments when quotes are not available.
    if not out:
        keywords = re.findall(r"[A-Za-z0-9][A-Za-z0-9./_-]{2,}|[\u4e00-\u9fff]{2,12}", description)
        for kw in keywords:
            if kw not in out and kw not in {"適用於", "建立", "修改", "測試", "打包", "發布", "技能"}:
                out.append(kw)
    return out[:12]


def bullet_list(items: list[str], fallback: str) -> str:
    filtered = [item.strip() for item in items if item.strip()]
    if not filtered:
        return f"- {fallback}"
    return "\n".join(f"- {item}" for item in filtered)


def make_plan(skill_name: str, description: str) -> str:
    triggers = guess_trigger_phrases(description)
    trigger_seed = triggers[:6]
    trigger_md = bullet_list(
        trigger_seed + ["[TODO: add paraphrases from real user wording]"],
        "[TODO: add trigger phrases users actually say]",
    )
    negative_seed = [
        "[TODO: unrelated request that should stay off]",
        "[TODO: near-miss that looks similar but belongs to another skill]",
        "[TODO: broad generic query that should not trigger this skill]",
    ]
    near_miss_seed = [
        "[TODO: similar wording but different outcome]",
        "[TODO: same tool name but different workflow]",
    ]

    return f"""# Test Plan: {skill_name}

## 1) Triggering tests

Goal: the skill loads when it should, and stays off when it should not.

### Should trigger (8-10)

Use obvious requests, paraphrases, real user wording, and file-type/tool variants.

{trigger_md}

For each item, record:
- Expected: trigger
- Actual:
- Notes:

### Should NOT trigger (8-10)

{bullet_list(negative_seed, "[TODO: add should-not-trigger cases]")}

For each item, record:
- Expected: no trigger
- Actual:
- Notes:

### Near-miss / confusing cases

{bullet_list(near_miss_seed, "[TODO: add near-miss cases]")}

### Multilingual coverage

- zh:
  - [TODO: pure Chinese phrasing]
- en:
  - [TODO: pure English phrasing]
- mixed:
  - [TODO: Chinese + English tool/file phrasing]

### Neighboring skills / overlap map

- Closest competing skill:
  - [TODO]
- Why this skill should win:
  - [TODO]
- Why another skill should win in adjacent cases:
  - [TODO]

### Trigger diagnostics

#### Under-trigger signals
- [ ] Obvious request failed to load the skill
- [ ] Only one specific wording works
- [ ] Real user phrasing fails but clean paraphrase works

#### Over-trigger signals
- [ ] Unrelated requests load the skill
- [ ] Broad generic keywords cause false positives
- [ ] Requests that belong to another skill still load this one

#### Likely fix direction
- If under-trigger dominates: revise `description`
- If over-trigger dominates: narrow scope or add negative triggers
- If both happen: split scope or rewrite `description` from scratch

## 2) Functional tests

Goal: outputs and tool usage are correct end-to-end.

Create test cases in Given/When/Then form. Prefer real tasks over invented textbook prompts.

### Test case A (happy path)
- Given:
- When:
- Then:

### Test case B (edge case)
- Given:
- When:
- Then:

### Test case C (failure mode / error handling)
- Given:
- When:
- Then:

### Test case D (recovery path)
- Given:
- When:
- Then:

### Failure classification
- If a test fails, label the primary cause:
  - Trigger problem
  - Workflow / instruction problem
  - Resource / script problem
  - External tool / MCP problem

## 3) Performance comparison

Goal: prove the skill improves baseline.

### Baseline (without skill)
- Result quality:
- Total messages / back-and-forth:
- Tool calls:
- Failed tool calls / retries:
- Token usage (if available):
- User corrections required:
- Main failure points:

### With skill
- Result quality:
- Total messages / back-and-forth:
- Tool calls:
- Failed tool calls / retries:
- Token usage (if available):
- User corrections required:
- Main failure points:

### Benchmark summary
- Better than baseline?
- If yes, why?
- If no, what got worse?
- Is the skill helping, or just adding more instructions?

### ROI review
- Is the quality gain worth the extra time/tokens?
- Does this skill reduce user corrections?
- Does this skill reduce operational risk or only add complexity?
- If this were maintained for 6 months, would it still be worth keeping?

## 4) Iteration log

### Round 1
- Change made:
- Hypothesis:
- Result:
- Next move:

### Round 2
- Change made:
- Hypothesis:
- Result:
- Next move:

## 5) Operationalize these cases

- Mirror approved prompts into `assets/evals/evals.json`
- Create an iteration workspace with `python scripts/prepare_eval_workspace.py <path/to/skill>`
- Save with-skill and baseline outputs in the paired workspace layout
- Generate `benchmark.json` / `benchmark.md`
- Generate `review.html` for human review
- Define release gates in `assets/evals/regression_gates.json`
- Validate release gates with `python scripts/check_regression_gates.py <benchmark.json> --config <gates.json>`

## 6) Release readiness checklist

- [ ] format_check.py has 0 errors
- [ ] quick_validate.py passes
- [ ] All functional tests pass
- [ ] Multilingual trigger coverage reviewed
- [ ] Neighboring skill overlap reviewed
- [ ] Under-trigger risks addressed
- [ ] Over-trigger risks addressed
- [ ] Baseline comparison completed
- [ ] ROI review completed
- [ ] Regression gates pass
- [ ] Real-user or near-real test prompts included
- [ ] Version bumped (top-level version: YYYY.M.D)
- [ ] Distribution instructions updated (repo-level docs outside skill folder)
"""


def main() -> int:
    ap = argparse.ArgumentParser(description="Generate a Markdown test plan scaffold for a skill")
    ap.add_argument("skill_dir", help="Path to skill folder")
    ap.add_argument("--out", help="Write output to a file (relative to skill_dir if relative)")
    args = ap.parse_args()

    skill_dir = Path(args.skill_dir).resolve()
    skill_md = skill_dir / "SKILL.md"
    if not skill_md.exists():
        raise SystemExit("SKILL.md not found")

    fm = extract_frontmatter(skill_md.read_text(encoding="utf-8"))
    name = str(fm.get("name", skill_dir.name)).strip() or skill_dir.name
    desc = str(fm.get("description", "")).strip()

    plan = make_plan(name, desc)

    if args.out:
        out_path = Path(args.out)
        if not out_path.is_absolute():
            out_path = (skill_dir / out_path).resolve()
        out_path.parent.mkdir(parents=True, exist_ok=True)
        out_path.write_text(plan, encoding="utf-8")
        print(f"✅ Wrote: {out_path}")
    else:
        print(plan)

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
