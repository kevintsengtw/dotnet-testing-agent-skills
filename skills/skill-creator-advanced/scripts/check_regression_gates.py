#!/usr/bin/env python3
"""Validate benchmark results against regression gates."""

from __future__ import annotations

import argparse
import json
from pathlib import Path


DEFAULT_GATES = {
    "min_pass_rate_delta": 0.0,
    "max_time_increase_seconds": None,
    "max_token_increase": None,
    "require_non_negative_pass_rate": True,
}


def load_json(path: Path) -> dict:
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        raise ValueError(f"Failed to read JSON from {path}: {exc}") from exc


def get_config_names(summary: dict) -> tuple[str, str]:
    configs = [key for key in summary.keys() if key != "delta"]
    if len(configs) < 2:
        raise ValueError("benchmark.json must contain at least two configurations for gate checking")
    return configs[0], configs[1]


def evaluate_gates(benchmark: dict, gates: dict) -> tuple[bool, list[str]]:
    summary = benchmark.get("run_summary", {})
    primary_name, baseline_name = get_config_names(summary)
    primary = summary[primary_name]
    baseline = summary[baseline_name]

    pass_delta = primary["pass_rate"]["mean"] - baseline["pass_rate"]["mean"]
    time_delta = primary["time_seconds"]["mean"] - baseline["time_seconds"]["mean"]
    token_delta = primary["tokens"]["mean"] - baseline["tokens"]["mean"]

    messages: list[str] = []
    ok = True

    min_pass_rate_delta = gates.get("min_pass_rate_delta", DEFAULT_GATES["min_pass_rate_delta"])
    if pass_delta < min_pass_rate_delta:
        ok = False
        messages.append(
            f"pass rate delta {pass_delta:+.2f} is below required minimum {min_pass_rate_delta:+.2f}"
        )
    else:
        messages.append(
            f"pass rate delta {pass_delta:+.2f} meets minimum {min_pass_rate_delta:+.2f}"
        )

    if gates.get("require_non_negative_pass_rate", DEFAULT_GATES["require_non_negative_pass_rate"]) and pass_delta < 0:
        ok = False
        messages.append("with-skill pass rate is worse than baseline")

    max_time_increase = gates.get("max_time_increase_seconds", DEFAULT_GATES["max_time_increase_seconds"])
    if max_time_increase is not None:
        if time_delta > max_time_increase:
            ok = False
            messages.append(
                f"time increase {time_delta:+.1f}s exceeds limit {max_time_increase:+.1f}s"
            )
        else:
            messages.append(
                f"time increase {time_delta:+.1f}s is within limit {max_time_increase:+.1f}s"
            )

    max_token_increase = gates.get("max_token_increase", DEFAULT_GATES["max_token_increase"])
    if max_token_increase is not None:
        if token_delta > max_token_increase:
            ok = False
            messages.append(
                f"token increase {token_delta:+.0f} exceeds limit {max_token_increase:+.0f}"
            )
        else:
            messages.append(
                f"token increase {token_delta:+.0f} is within limit {max_token_increase:+.0f}"
            )

    messages.insert(0, f"primary={primary_name}, baseline={baseline_name}")
    return ok, messages


def main() -> int:
    parser = argparse.ArgumentParser(description="Check benchmark.json against regression gates")
    parser.add_argument("benchmark", help="Path to benchmark.json")
    parser.add_argument("--config", required=True, help="Path to regression_gates.json")
    args = parser.parse_args()

    benchmark = load_json(Path(args.benchmark).resolve())
    gates = DEFAULT_GATES | load_json(Path(args.config).resolve())
    ok, messages = evaluate_gates(benchmark, gates)

    for message in messages:
        print(message)

    if ok:
        print("Regression gates: PASS")
        return 0

    print("Regression gates: FAIL")
    return 1


if __name__ == "__main__":
    raise SystemExit(main())
