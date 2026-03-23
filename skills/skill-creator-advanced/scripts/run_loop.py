#!/usr/bin/env python3
"""Run trigger-eval and description-improvement loops for a skill."""

from __future__ import annotations

import argparse
import json
import random
import sys
import time
import webbrowser
from pathlib import Path

try:
    from scripts.generate_report import generate_html
    from scripts.improve_description import improve_description
    from scripts.run_eval import find_project_root, load_trigger_eval_set, run_eval
    from scripts.utils import parse_skill_md, update_skill_description, write_json
except ImportError:  # pragma: no cover - direct script execution fallback
    from generate_report import generate_html
    from improve_description import improve_description
    from run_eval import find_project_root, load_trigger_eval_set, run_eval
    from utils import parse_skill_md, update_skill_description, write_json


def _stratified_split(items: list[dict], holdout: float, seed: int) -> tuple[list[dict], list[dict]]:
    if not 0 <= holdout < 1:
        raise ValueError("--holdout must be between 0 and 1 (1 excluded)")
    if holdout == 0:
        return items, []

    rng = random.Random(seed)
    positives = [item for item in items if item["should_trigger"]]
    negatives = [item for item in items if not item["should_trigger"]]

    def split_group(group: list[dict]) -> tuple[list[dict], list[dict]]:
        if len(group) <= 1:
            return group[:], []

        shuffled = group[:]
        rng.shuffle(shuffled)
        test_count = int(round(len(shuffled) * holdout))
        test_count = max(1, test_count)
        test_count = min(test_count, len(shuffled) - 1)
        return shuffled[test_count:], shuffled[:test_count]

    train_pos, test_pos = split_group(positives)
    train_neg, test_neg = split_group(negatives)

    train = train_pos + train_neg
    test = test_pos + test_neg
    rng.shuffle(train)
    rng.shuffle(test)
    return train, test


def _score_tuple(item: dict, use_test: bool) -> tuple[float, float, int]:
    if use_test:
        test_ratio = (item.get("test_passed") or 0) / max(item.get("test_total") or 1, 1)
    else:
        test_ratio = -1.0
    train_ratio = (item.get("train_passed") or item.get("passed", 0)) / max(
        item.get("train_total") or item.get("total", 1), 1
    )
    return (test_ratio, train_ratio, -len(item.get("description", "")))


def _resolve_results_dir(skill_path: Path, skill_name: str, results_dir_arg: str) -> Path | None:
    if results_dir_arg == "none":
        return None

    timestamp = time.strftime("%Y-%m-%d_%H%M%S")
    if results_dir_arg == "auto":
        base_dir = skill_path.parent / f"{skill_name}-workspace" / "description-optimization"
    else:
        base_dir = Path(results_dir_arg).resolve()

    results_dir = base_dir / timestamp
    results_dir.mkdir(parents=True, exist_ok=False)
    return results_dir


def run_loop(
    eval_set: list[dict],
    skill_path: Path,
    description_override: str | None,
    num_workers: int,
    timeout: int,
    max_iterations: int,
    runs_per_query: int,
    trigger_threshold: float,
    holdout: float,
    seed: int,
    model: str,
    verbose: bool,
    max_chars: int,
    results_dir: Path | None = None,
    report_path: Path | None = None,
) -> dict:
    """Run iterative description optimization."""
    project_root = find_project_root()
    skill_name, original_description, skill_content = parse_skill_md(skill_path)
    current_description = description_override or original_description

    train_set, test_set = _stratified_split(eval_set, holdout=holdout, seed=seed)
    history: list[dict] = []
    exit_reason = "unknown"
    log_dir = results_dir / "logs" if results_dir else None
    if log_dir:
        log_dir.mkdir(parents=True, exist_ok=True)

    for iteration in range(1, max_iterations + 1):
        if verbose:
            print(f"\n{'=' * 60}", file=sys.stderr)
            print(f"Iteration {iteration}/{max_iterations}", file=sys.stderr)
            print(f"Description ({len(current_description)} chars): {current_description}", file=sys.stderr)

        eval_started = time.time()
        all_results = run_eval(
            eval_set=train_set + test_set,
            skill_name=skill_name,
            description=current_description,
            num_workers=num_workers,
            timeout=timeout,
            project_root=project_root,
            runs_per_query=runs_per_query,
            trigger_threshold=trigger_threshold,
            model=model,
        )
        eval_elapsed = round(time.time() - eval_started, 3)

        train_queries = {item["query"] for item in train_set}
        train_results = [row for row in all_results["results"] if row["query"] in train_queries]
        test_results = [row for row in all_results["results"] if row["query"] not in train_queries]

        train_summary = {
            "passed": sum(1 for row in train_results if row["pass"]),
            "failed": sum(1 for row in train_results if not row["pass"]),
            "total": len(train_results),
        }
        test_summary = {
            "passed": sum(1 for row in test_results if row["pass"]),
            "failed": sum(1 for row in test_results if not row["pass"]),
            "total": len(test_results),
        }

        history_entry = {
            "iteration": iteration,
            "description": current_description,
            "train_passed": train_summary["passed"],
            "train_failed": train_summary["failed"],
            "train_total": train_summary["total"],
            "train_results": train_results,
            "test_passed": test_summary["passed"] if test_results else None,
            "test_failed": test_summary["failed"] if test_results else None,
            "test_total": test_summary["total"] if test_results else None,
            "test_results": test_results if test_results else None,
            "eval_seconds": eval_elapsed,
            "passed": train_summary["passed"],
            "failed": train_summary["failed"],
            "total": train_summary["total"],
            "results": train_results,
        }
        history.append(history_entry)

        if results_dir:
            write_json(results_dir / f"iteration-{iteration:02d}-eval.json", all_results)
            write_json(results_dir / "history.json", history)

        if report_path:
            interim_output = {
                "exit_reason": "in_progress",
                "original_description": original_description,
                "best_description": current_description,
                "best_score": "in progress",
                "iterations_run": len(history),
                "train_size": len(train_set),
                "test_size": len(test_set),
                "history": history,
            }
            report_path.write_text(
                generate_html(interim_output, auto_refresh=True, skill_name=skill_name),
                encoding="utf-8",
            )

        if verbose:
            test_suffix = (
                f" | test {test_summary['passed']}/{test_summary['total']}" if test_results else ""
            )
            print(
                f"Train {train_summary['passed']}/{train_summary['total']}{test_suffix} "
                f"in {eval_elapsed:.1f}s",
                file=sys.stderr,
            )

        if train_summary["failed"] == 0:
            exit_reason = f"all_train_queries_passed@iteration_{iteration}"
            break

        if iteration == max_iterations:
            exit_reason = f"max_iterations_reached@{max_iterations}"
            break

        improve_started = time.time()
        blind_history = [{key: value for key, value in item.items() if not key.startswith("test_")} for item in history]
        improved = improve_description(
            skill_name=skill_name,
            skill_content=skill_content,
            current_description=current_description,
            eval_results={
                "description": current_description,
                "results": train_results,
                "summary": train_summary,
            },
            history=blind_history,
            model=model,
            max_chars=max_chars,
            test_results={
                "results": test_results,
                "summary": test_summary,
            }
            if test_results
            else None,
            log_dir=log_dir,
            iteration=iteration,
        )
        improve_elapsed = round(time.time() - improve_started, 3)
        history_entry["improve_seconds"] = improve_elapsed

        next_description = improved["description"]
        if verbose:
            print(
                f"Proposed next description ({len(next_description)} chars, {improve_elapsed:.1f}s)",
                file=sys.stderr,
            )

        if next_description == current_description or any(
            previous["description"] == next_description for previous in history
        ):
            exit_reason = f"stalled_repeated_description@iteration_{iteration}"
            break

        current_description = next_description

    use_test = bool(test_set)
    best = max(history, key=lambda item: _score_tuple(item, use_test=use_test)) if history else None
    best_score = None
    if best:
        if use_test:
            best_score = f"{best.get('test_passed', 0)}/{best.get('test_total', 0)}"
        else:
            best_score = f"{best.get('train_passed', 0)}/{best.get('train_total', 0)}"

    return {
        "exit_reason": exit_reason,
        "original_description": original_description,
        "best_description": best["description"] if best else current_description,
        "best_score": best_score or "0/0",
        "best_train_score": (
            f"{best.get('train_passed', 0)}/{best.get('train_total', 0)}" if best else "0/0"
        ),
        "best_test_score": (
            f"{best.get('test_passed', 0)}/{best.get('test_total', 0)}" if best and use_test else None
        ),
        "final_description": current_description,
        "iterations_run": len(history),
        "holdout": holdout,
        "train_size": len(train_set),
        "test_size": len(test_set),
        "project_root": str(project_root),
        "results_dir": str(results_dir) if results_dir else None,
        "history": history,
    }


def main() -> int:
    parser = argparse.ArgumentParser(description="Run eval + description-improvement loop")
    parser.add_argument("--eval-set", required=True, help="Path to trigger eval JSON")
    parser.add_argument("--skill-path", required=True, help="Path to the skill directory")
    parser.add_argument("--description", default=None, help="Override starting description")
    parser.add_argument("--num-workers", type=int, default=6, help="Parallel worker count")
    parser.add_argument("--timeout", type=int, default=45, help="Per-query timeout in seconds")
    parser.add_argument("--max-iterations", type=int, default=5, help="Maximum optimization iterations")
    parser.add_argument("--runs-per-query", type=int, default=3, help="Repeated runs per query")
    parser.add_argument("--trigger-threshold", type=float, default=0.5, help="Pass threshold for should-trigger queries")
    parser.add_argument("--holdout", type=float, default=0.4, help="Held-out fraction for test queries")
    parser.add_argument("--seed", type=int, default=42, help="Random seed for train/test split")
    parser.add_argument("--model", required=True, help="Model passed to claude -p")
    parser.add_argument("--max-chars", type=int, default=900, help="Soft max length for candidate descriptions")
    parser.add_argument(
        "--results-dir",
        default="auto",
        help="Base directory for artifacts, 'auto', or 'none'",
    )
    parser.add_argument(
        "--report",
        default="auto",
        help="HTML report path, 'auto' to place under results dir, or 'none'",
    )
    parser.add_argument(
        "--open-report",
        action="store_true",
        help="Open the HTML report in the default browser",
    )
    parser.add_argument(
        "--apply-best",
        action="store_true",
        help="Write the best description back to SKILL.md at the end",
    )
    parser.add_argument("--verbose", action="store_true", help="Print progress to stderr")
    args = parser.parse_args()

    skill_path = Path(args.skill_path).resolve()
    if not (skill_path / "SKILL.md").exists():
        print(f"Error: No SKILL.md found at {skill_path}", file=sys.stderr)
        return 1

    eval_set = load_trigger_eval_set(Path(args.eval_set).resolve())
    skill_name, _, _ = parse_skill_md(skill_path)
    results_dir = _resolve_results_dir(skill_path, skill_name, args.results_dir)

    if results_dir:
        write_json(results_dir / "trigger_eval_set.json", eval_set)

    if args.report == "none":
        report_path = None
    elif args.report == "auto":
        report_path = (results_dir / "report.html") if results_dir else None
    else:
        report_path = Path(args.report).resolve()
        report_path.parent.mkdir(parents=True, exist_ok=True)

    output = run_loop(
        eval_set=eval_set,
        skill_path=skill_path,
        description_override=args.description,
        num_workers=args.num_workers,
        timeout=args.timeout,
        max_iterations=args.max_iterations,
        runs_per_query=args.runs_per_query,
        trigger_threshold=args.trigger_threshold,
        holdout=args.holdout,
        seed=args.seed,
        model=args.model,
        verbose=args.verbose,
        max_chars=args.max_chars,
        results_dir=results_dir,
        report_path=report_path,
    )

    if args.apply_best:
        update_skill_description(skill_path, output["best_description"])
        output["applied_to"] = str(skill_path / "SKILL.md")

    if report_path:
        report_path.write_text(
            generate_html(output, auto_refresh=False, skill_name=skill_name),
            encoding="utf-8",
        )
        if args.open_report:
            webbrowser.open(report_path.resolve().as_uri())

    rendered = json.dumps(output, indent=2, ensure_ascii=False)
    print(rendered)

    if results_dir:
        write_json(results_dir / "results.json", output)
        if args.verbose:
            print(f"Artifacts: {results_dir}", file=sys.stderr)
            if report_path:
                print(f"Report: {report_path}", file=sys.stderr)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
