#!/usr/bin/env python3
"""Aggregate eval run outputs into benchmark.json and benchmark.md."""

from __future__ import annotations

import argparse
import json
import math
from datetime import datetime, timezone
from pathlib import Path


def calculate_stats(values: list[float]) -> dict:
    if not values:
        return {"mean": 0.0, "stddev": 0.0, "min": 0.0, "max": 0.0}

    mean = sum(values) / len(values)
    if len(values) > 1:
        variance = sum((value - mean) ** 2 for value in values) / (len(values) - 1)
        stddev = math.sqrt(variance)
    else:
        stddev = 0.0

    return {
        "mean": round(mean, 4),
        "stddev": round(stddev, 4),
        "min": round(min(values), 4),
        "max": round(max(values), 4),
    }


def find_eval_dirs(root: Path) -> list[Path]:
    candidates: list[Path] = []
    if (root / "eval_metadata.json").exists():
        return [root]

    for path in sorted(root.rglob("eval_metadata.json")):
        candidates.append(path.parent)
    return candidates


def load_json(path: Path) -> dict | None:
    if not path.exists():
        return None
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError):
        return None


def extract_result(run_dir: Path) -> dict:
    grading = load_json(run_dir / "grading.json") or {}
    timing = load_json(run_dir / "timing.json") or {}
    metrics = load_json(run_dir / "outputs" / "metrics.json") or {}

    summary = grading.get("summary", {})
    execution = grading.get("execution_metrics", {}) or metrics
    timing_data = grading.get("timing", {}) or timing
    notes_summary = grading.get("user_notes_summary", {})

    notes: list[str] = []
    for key in ("uncertainties", "needs_review", "workarounds"):
        notes.extend(notes_summary.get(key, []))

    tokens = timing_data.get("total_tokens", 0)
    if not tokens:
        tokens = metrics.get("output_chars", 0)

    return {
        "pass_rate": summary.get("pass_rate", 0.0),
        "passed": summary.get("passed", 0),
        "failed": summary.get("failed", 0),
        "total": summary.get("total", 0),
        "time_seconds": timing_data.get("total_duration_seconds", 0.0),
        "tokens": tokens,
        "tool_calls": execution.get("total_tool_calls", 0),
        "errors": execution.get("errors_encountered", 0),
        "expectations": grading.get("expectations", []),
        "notes": notes,
    }


def iter_config_runs(eval_dir: Path) -> list[tuple[str, int, Path]]:
    rows: list[tuple[str, int, Path]] = []
    for config_dir in sorted(path for path in eval_dir.iterdir() if path.is_dir()):
        run_dirs = sorted(config_dir.glob("run-*"))
        if run_dirs:
            for run_dir in run_dirs:
                try:
                    run_number = int(run_dir.name.split("-")[1])
                except (IndexError, ValueError):
                    run_number = 1
                rows.append((config_dir.name, run_number, run_dir))
        elif (config_dir / "outputs").exists() or (config_dir / "grading.json").exists():
            rows.append((config_dir.name, 1, config_dir))
    return rows


def generate_benchmark(root: Path, skill_name: str, skill_path: str) -> dict:
    eval_dirs = find_eval_dirs(root)
    if not eval_dirs:
        raise FileNotFoundError(f"No eval directories with eval_metadata.json found under {root}")

    runs: list[dict] = []
    grouped: dict[str, list[dict]] = {}
    eval_ids: list[int] = []

    for eval_dir in eval_dirs:
        metadata = load_json(eval_dir / "eval_metadata.json") or {}
        eval_id = int(metadata.get("eval_id", len(eval_ids) + 1))
        eval_name = str(metadata.get("eval_name") or eval_dir.name)
        eval_ids.append(eval_id)

        for config, run_number, run_dir in iter_config_runs(eval_dir):
            result = extract_result(run_dir)
            row = {
                "eval_id": eval_id,
                "eval_name": eval_name,
                "configuration": config,
                "run_number": run_number,
                "result": {
                    "pass_rate": result["pass_rate"],
                    "passed": result["passed"],
                    "failed": result["failed"],
                    "total": result["total"],
                    "time_seconds": result["time_seconds"],
                    "tokens": result["tokens"],
                    "tool_calls": result["tool_calls"],
                    "errors": result["errors"],
                },
                "expectations": result["expectations"],
                "notes": result["notes"],
            }
            runs.append(row)
            grouped.setdefault(config, []).append(row)

    run_summary: dict[str, dict] = {}
    for config, items in grouped.items():
        run_summary[config] = {
            "pass_rate": calculate_stats([item["result"]["pass_rate"] for item in items]),
            "time_seconds": calculate_stats([item["result"]["time_seconds"] for item in items]),
            "tokens": calculate_stats([item["result"]["tokens"] for item in items]),
        }

    configs = list(grouped.keys())
    if len(configs) >= 2:
        primary = run_summary[configs[0]]
        baseline = run_summary[configs[1]]
    elif len(configs) == 1:
        primary = run_summary[configs[0]]
        baseline = {
            "pass_rate": {"mean": 0.0},
            "time_seconds": {"mean": 0.0},
            "tokens": {"mean": 0.0},
        }
    else:
        primary = baseline = {
            "pass_rate": {"mean": 0.0},
            "time_seconds": {"mean": 0.0},
            "tokens": {"mean": 0.0},
        }

    run_summary["delta"] = {
        "pass_rate": f"{primary['pass_rate']['mean'] - baseline['pass_rate']['mean']:+.2f}",
        "time_seconds": f"{primary['time_seconds']['mean'] - baseline['time_seconds']['mean']:+.1f}",
        "tokens": f"{primary['tokens']['mean'] - baseline['tokens']['mean']:+.0f}",
    }

    notes = []
    if len(configs) >= 2:
        notes.append(
            f"{configs[0]} mean pass rate {run_summary[configs[0]]['pass_rate']['mean']:.2f} "
            f"vs {configs[1]} {run_summary[configs[1]]['pass_rate']['mean']:.2f}"
        )

    return {
        "metadata": {
            "skill_name": skill_name,
            "skill_path": skill_path,
            "timestamp": datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
            "evals_run": sorted(set(eval_ids)),
            "runs_per_configuration": max((item["run_number"] for item in runs), default=1),
        },
        "runs": runs,
        "run_summary": run_summary,
        "notes": notes,
    }


def generate_markdown(benchmark: dict) -> str:
    summary = benchmark["run_summary"]
    configs = [key for key in summary.keys() if key != "delta"]
    lines = [
        f"# Skill Benchmark: {benchmark['metadata']['skill_name']}",
        "",
        f"**Date**: {benchmark['metadata']['timestamp']}",
        f"**Evals**: {', '.join(str(item) for item in benchmark['metadata']['evals_run'])}",
        "",
        "## Summary",
        "",
    ]

    if len(configs) >= 2:
        a, b = configs[:2]
        lines.extend(
            [
                f"| Metric | {a} | {b} | Delta |",
                "|--------|---|---|---|",
                (
                    f"| Pass Rate | {summary[a]['pass_rate']['mean']:.2f} ± {summary[a]['pass_rate']['stddev']:.2f} "
                    f"| {summary[b]['pass_rate']['mean']:.2f} ± {summary[b]['pass_rate']['stddev']:.2f} "
                    f"| {summary['delta']['pass_rate']} |"
                ),
                (
                    f"| Time (s) | {summary[a]['time_seconds']['mean']:.1f} ± {summary[a]['time_seconds']['stddev']:.1f} "
                    f"| {summary[b]['time_seconds']['mean']:.1f} ± {summary[b]['time_seconds']['stddev']:.1f} "
                    f"| {summary['delta']['time_seconds']} |"
                ),
                (
                    f"| Tokens | {summary[a]['tokens']['mean']:.0f} ± {summary[a]['tokens']['stddev']:.0f} "
                    f"| {summary[b]['tokens']['mean']:.0f} ± {summary[b]['tokens']['stddev']:.0f} "
                    f"| {summary['delta']['tokens']} |"
                ),
            ]
        )
    else:
        for config in configs:
            lines.append(
                f"- {config}: pass_rate={summary[config]['pass_rate']['mean']:.2f}, "
                f"time={summary[config]['time_seconds']['mean']:.1f}s, "
                f"tokens={summary[config]['tokens']['mean']:.0f}"
            )

    if benchmark.get("notes"):
        lines.extend(["", "## Notes", ""])
        for note in benchmark["notes"]:
            lines.append(f"- {note}")

    return "\n".join(lines) + "\n"


def main() -> int:
    parser = argparse.ArgumentParser(description="Aggregate run outputs into benchmark.json and benchmark.md")
    parser.add_argument("root", help="Iteration directory or any directory containing eval_metadata.json files")
    parser.add_argument("--skill-name", default="", help="Skill name override")
    parser.add_argument("--skill-path", default="", help="Skill path override")
    parser.add_argument("--output", help="Output benchmark.json path (default: <root>/benchmark.json)")
    args = parser.parse_args()

    root = Path(args.root).resolve()
    output_json = Path(args.output).resolve() if args.output else (root / "benchmark.json")
    output_md = output_json.with_suffix(".md")

    benchmark = generate_benchmark(
        root=root,
        skill_name=args.skill_name or root.name.replace("-workspace", ""),
        skill_path=args.skill_path or str(root),
    )
    output_json.write_text(json.dumps(benchmark, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    output_md.write_text(generate_markdown(benchmark), encoding="utf-8")

    print(f"Generated: {output_json}")
    print(f"Generated: {output_md}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
