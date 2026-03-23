#!/usr/bin/env python3
"""Generate an HTML report for description optimization runs."""

from __future__ import annotations

import argparse
import html
import json
import sys
from pathlib import Path


def _score_class(correct: int, total: int) -> str:
    if total <= 0:
        return "score-bad"
    ratio = correct / total
    if ratio >= 0.8:
        return "score-good"
    if ratio >= 0.5:
        return "score-ok"
    return "score-bad"


def _aggregate_runs(results: list[dict]) -> tuple[int, int]:
    correct = 0
    total = 0
    for result in results:
        runs = int(result.get("runs", 0))
        triggers = int(result.get("triggers", 0))
        total += runs
        if result.get("should_trigger", True):
            correct += triggers
        else:
            correct += runs - triggers
    return correct, total


def generate_html(data: dict, auto_refresh: bool = False, skill_name: str = "") -> str:
    """Render the optimization history as a standalone HTML page."""
    history = data.get("history", [])
    train_queries: list[dict] = []
    test_queries: list[dict] = []

    if history:
        first = history[0]
        for item in first.get("train_results", first.get("results", [])):
            train_queries.append(
                {"query": item["query"], "should_trigger": item.get("should_trigger", True)}
            )
        for item in first.get("test_results", []) or []:
            test_queries.append(
                {"query": item["query"], "should_trigger": item.get("should_trigger", True)}
            )

    if history:
        if test_queries:
            best_iteration = max(
                history,
                key=lambda item: (
                    (item.get("test_passed") or 0) / max(item.get("test_total") or 1, 1),
                    (item.get("train_passed") or 0) / max(item.get("train_total") or 1, 1),
                    -len(item.get("description", "")),
                ),
            ).get("iteration")
        else:
            best_iteration = max(
                history,
                key=lambda item: (
                    (item.get("train_passed") or item.get("passed", 0))
                    / max(item.get("train_total") or item.get("total", 1), 1),
                    -len(item.get("description", "")),
                ),
            ).get("iteration")
    else:
        best_iteration = None

    refresh_tag = '    <meta http-equiv="refresh" content="5">\n' if auto_refresh else ""
    title_prefix = html.escape(f"{skill_name} - ") if skill_name else ""
    html_parts = [
        "<!DOCTYPE html>\n",
        "<html>\n",
        "<head>\n",
        '    <meta charset="utf-8">\n',
        refresh_tag,
        f"    <title>{title_prefix}Description Optimization</title>\n",
        "    <style>\n",
        "        body { font-family: Segoe UI, Arial, sans-serif; margin: 24px; background: #f6f4ed; color: #1e1f22; }\n",
        "        h1, h2, h3 { margin-top: 0; }\n",
        "        .panel { background: #fff; border: 1px solid #d9d4c7; border-radius: 12px; padding: 16px; margin-bottom: 18px; }\n",
        "        .best { color: #365e32; font-weight: 700; }\n",
        "        .legend { display: flex; gap: 18px; flex-wrap: wrap; margin-bottom: 12px; }\n",
        "        .legend span { display: inline-flex; align-items: center; gap: 6px; }\n",
        "        .swatch { width: 14px; height: 14px; border-radius: 3px; display: inline-block; }\n",
        "        .swatch-positive { background: #204c2e; }\n",
        "        .swatch-negative { background: #963b3b; }\n",
        "        .swatch-test { background: #2f6db3; }\n",
        "        table { border-collapse: collapse; width: 100%; background: #fff; border: 1px solid #d9d4c7; }\n",
        "        th, td { border: 1px solid #d9d4c7; padding: 8px; vertical-align: top; }\n",
        "        th { background: #20242c; color: #faf8f0; }\n",
        "        th.query-positive { border-bottom: 4px solid #204c2e; }\n",
        "        th.query-negative { border-bottom: 4px solid #963b3b; }\n",
        "        th.query-test { background: #2f6db3; }\n",
        "        td.description { max-width: 420px; font-family: Consolas, monospace; font-size: 12px; }\n",
        "        td.result { text-align: center; min-width: 54px; }\n",
        "        td.test-result { background: #eff6ff; }\n",
        "        .pass { color: #204c2e; font-weight: 700; }\n",
        "        .fail { color: #963b3b; font-weight: 700; }\n",
        "        .rate { display: block; font-size: 11px; color: #5c6470; }\n",
        "        .score { display: inline-block; padding: 2px 8px; border-radius: 999px; font-weight: 700; }\n",
        "        .score-good { background: #e6f2e4; color: #204c2e; }\n",
        "        .score-ok { background: #fff2d8; color: #8a5a00; }\n",
        "        .score-bad { background: #fbe6e6; color: #963b3b; }\n",
        "        tr.best-row { background: #f3f8ee; }\n",
        "        .table-wrap { overflow-x: auto; }\n",
        "    </style>\n",
        "</head>\n",
        "<body>\n",
        f"    <h1>{title_prefix}Description Optimization</h1>\n",
        '    <div class="panel">\n',
        "        <p><strong>Original:</strong> "
        + html.escape(data.get("original_description", "N/A"))
        + "</p>\n",
        "        <p class=\"best\"><strong>Best:</strong> "
        + html.escape(data.get("best_description", "N/A"))
        + "</p>\n",
        "        <p><strong>Exit reason:</strong> "
        + html.escape(data.get("exit_reason", "unknown"))
        + "</p>\n",
        "        <p><strong>Best score:</strong> "
        + html.escape(data.get("best_score", "N/A"))
        + "</p>\n",
        "        <p><strong>Iterations:</strong> "
        + html.escape(str(data.get("iterations_run", 0)))
        + " | <strong>Train:</strong> "
        + html.escape(str(data.get("train_size", "?")))
        + " | <strong>Test:</strong> "
        + html.escape(str(data.get("test_size", "?")))
        + "</p>\n",
        "    </div>\n",
        '    <div class="panel">\n',
        "        <p><strong>How to read this:</strong> each row is one description candidate. Checkmarks mean the query behaved as expected. Numbers under each mark show how many repeated runs triggered.</p>\n",
        '        <div class="legend">\n',
        '            <span><i class="swatch swatch-positive"></i>Should trigger</span>\n',
        '            <span><i class="swatch swatch-negative"></i>Should not trigger</span>\n',
        '            <span><i class="swatch swatch-test"></i>Held-out test query</span>\n',
        "        </div>\n",
        "    </div>\n",
        '    <div class="table-wrap">\n',
        "    <table>\n",
        "        <thead>\n",
        "            <tr>\n",
        "                <th>Iter</th>\n",
        "                <th>Train</th>\n",
        "                <th>Test</th>\n",
        "                <th>Description</th>\n",
    ]

    for query_info in train_queries:
        polarity_class = "query-positive" if query_info["should_trigger"] else "query-negative"
        html_parts.append(
            f"                <th class=\"{polarity_class}\">{html.escape(query_info['query'])}</th>\n"
        )
    for query_info in test_queries:
        polarity_class = "query-positive" if query_info["should_trigger"] else "query-negative"
        html_parts.append(
            f"                <th class=\"query-test {polarity_class}\">{html.escape(query_info['query'])}</th>\n"
        )

    html_parts.extend(
        [
            "            </tr>\n",
            "        </thead>\n",
            "        <tbody>\n",
        ]
    )

    for item in history:
        train_results = item.get("train_results", item.get("results", []))
        test_results = item.get("test_results", []) or []
        train_by_query = {result["query"]: result for result in train_results}
        test_by_query = {result["query"]: result for result in test_results}
        train_correct, train_total = _aggregate_runs(train_results)
        test_correct, test_total = _aggregate_runs(test_results)
        row_class = "best-row" if item.get("iteration") == best_iteration else ""

        html_parts.append(
            f"            <tr class=\"{row_class}\">\n"
            f"                <td>{html.escape(str(item.get('iteration', '?')))}</td>\n"
            f"                <td><span class=\"score {_score_class(train_correct, train_total)}\">{train_correct}/{train_total}</span></td>\n"
            f"                <td><span class=\"score {_score_class(test_correct, test_total)}\">{test_correct}/{test_total}</span></td>\n"
            f"                <td class=\"description\">{html.escape(item.get('description', ''))}</td>\n"
        )

        for query_info in train_queries:
            result = train_by_query.get(query_info["query"], {})
            passed = bool(result.get("pass"))
            triggers = result.get("triggers", 0)
            runs = result.get("runs", 0)
            icon = "PASS" if passed else "FAIL"
            html_parts.append(
                "                <td class=\"result "
                + ("pass" if passed else "fail")
                + "\">"
                + icon
                + f"<span class=\"rate\">{triggers}/{runs}</span></td>\n"
            )

        for query_info in test_queries:
            result = test_by_query.get(query_info["query"], {})
            passed = bool(result.get("pass"))
            triggers = result.get("triggers", 0)
            runs = result.get("runs", 0)
            icon = "PASS" if passed else "FAIL"
            html_parts.append(
                "                <td class=\"result test-result "
                + ("pass" if passed else "fail")
                + "\">"
                + icon
                + f"<span class=\"rate\">{triggers}/{runs}</span></td>\n"
            )

        html_parts.append("            </tr>\n")

    html_parts.extend(
        [
            "        </tbody>\n",
            "    </table>\n",
            "    </div>\n",
            "</body>\n",
            "</html>\n",
        ]
    )
    return "".join(html_parts)


def main() -> int:
    parser = argparse.ArgumentParser(description="Generate an HTML report from run_loop output")
    parser.add_argument("input", help="Path to run_loop JSON output, or - for stdin")
    parser.add_argument("-o", "--output", help="Write HTML to this path instead of stdout")
    parser.add_argument("--skill-name", default="", help="Optional skill name for the title")
    args = parser.parse_args()

    if args.input == "-":
        data = json.load(sys.stdin)
    else:
        data = json.loads(Path(args.input).read_text(encoding="utf-8"))

    html_output = generate_html(data, skill_name=args.skill_name)
    if args.output:
        Path(args.output).write_text(html_output, encoding="utf-8")
        print(f"Report written to {args.output}", file=sys.stderr)
    else:
        print(html_output)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
