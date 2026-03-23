#!/usr/bin/env python3
"""Improve a skill description based on trigger-eval failures."""

from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
from pathlib import Path
from typing import Any

try:
    from scripts.utils import load_json, parse_skill_md, write_json
except ImportError:  # pragma: no cover - direct script execution fallback
    from utils import load_json, parse_skill_md, write_json


TAG_RE = re.compile(r"</?new_description>", re.IGNORECASE)


def _call_claude(prompt: str, model: str | None, timeout: int = 300) -> str:
    cmd = ["claude", "-p", "--output-format", "text"]
    if model:
        cmd.extend(["--model", model])

    env = {key: value for key, value in os.environ.items() if key != "CLAUDECODE"}
    try:
        result = subprocess.run(
            cmd,
            input=prompt,
            capture_output=True,
            text=True,
            encoding="utf-8",
            errors="replace",
            env=env,
            timeout=timeout,
        )
    except FileNotFoundError as exc:  # pragma: no cover - depends on local env
        raise RuntimeError(
            "Cannot find 'claude' CLI. Install/configure Claude Code before improving descriptions."
        ) from exc

    if result.returncode != 0:
        raise RuntimeError(f"claude -p exited {result.returncode}\nstderr: {result.stderr}")
    return result.stdout


def _normalize_description(text: str) -> str:
    text = TAG_RE.sub("", text)
    text = text.strip().strip('"').strip("'")
    text = text.replace("<", "").replace(">", "")
    text = re.sub(r"\s+", " ", text)
    return text.strip()


def _trim_description(text: str, max_chars: int) -> str:
    if len(text) <= max_chars:
        return text

    candidate = text[:max_chars].rstrip()
    boundaries = [
        candidate.rfind(". "),
        candidate.rfind("。"),
        candidate.rfind("; "),
        candidate.rfind("! "),
        candidate.rfind("? "),
        candidate.rfind(", "),
        candidate.rfind("，"),
    ]
    boundary = max(boundaries)
    if boundary >= int(max_chars * 0.6):
        candidate = candidate[: boundary + 1].rstrip(" ,;，")
    else:
        candidate = candidate.rsplit(" ", 1)[0].rstrip(" ,;，")

    return candidate.strip()


def _summarize_failures(results: list[dict[str, Any]], should_trigger: bool) -> list[str]:
    selected = [
        item for item in results if item.get("should_trigger") is should_trigger and not item.get("pass")
    ]
    lines = []
    for item in selected:
        expectation = "should trigger" if should_trigger else "should NOT trigger"
        lines.append(
            f'- "{item["query"]}" [{expectation}] '
            f"(triggered {item['triggers']}/{item['runs']} times)"
        )
    return lines


def _build_prompt(
    skill_name: str,
    skill_content: str,
    current_description: str,
    eval_results: dict[str, Any],
    history: list[dict[str, Any]],
    max_chars: int,
    test_results: dict[str, Any] | None = None,
) -> str:
    failed_to_trigger = _summarize_failures(eval_results["results"], should_trigger=True)
    false_triggers = _summarize_failures(eval_results["results"], should_trigger=False)

    train_score = f"{eval_results['summary']['passed']}/{eval_results['summary']['total']}"
    if test_results:
        test_score = f"{test_results['summary']['passed']}/{test_results['summary']['total']}"
        score_line = f"Train: {train_score}; Test: {test_score}"
    else:
        score_line = f"Train: {train_score}"

    prompt = f"""You are optimizing the description of a Claude Code skill named "{skill_name}".

The description is the main trigger surface. Claude sees the skill name plus this description before deciding whether to consult the skill.

Current description:
<current_description>
{current_description}
</current_description>

Current score:
{score_line}

Skill content for context:
<skill_content>
{skill_content}
</skill_content>

Guidance for the rewrite:
1. Prioritize obvious should-trigger queries before edge cases.
2. Use real user intent, work context, deliverables, file types, and platform names instead of internal taxonomy words.
3. Cover multilingual and mixed-language phrasing when it matters, but do not bloat the description.
4. If false positives cluster around neighboring workflows, narrow the boundary with one concise contrast or negative trigger.
5. Keep the description readable, imperative, and comfortably under {max_chars} characters.
6. Avoid angle brackets and do not turn the description into a giant keyword list.
7. Generalize from the failures; do not overfit to the exact example wording.

Failure analysis:
"""
    if failed_to_trigger:
        prompt += "FAILED TO TRIGGER:\n" + "\n".join(failed_to_trigger) + "\n\n"
    if false_triggers:
        prompt += "FALSE TRIGGERS:\n" + "\n".join(false_triggers) + "\n\n"
    if not failed_to_trigger and not false_triggers:
        prompt += "No failed queries in the provided results.\n\n"

    if history:
        prompt += "Previous attempts (avoid repeating the same structure):\n"
        for item in history:
            train = f"{item.get('train_passed', item.get('passed', 0))}/{item.get('train_total', item.get('total', 0))}"
            test = item.get("test_passed")
            test_total = item.get("test_total")
            score_suffix = f", test={test}/{test_total}" if test is not None and test_total is not None else ""
            prompt += f'- train={train}{score_suffix} :: "{item.get("description", "")}"\n'
        prompt += "\n"

    prompt += """Respond with only the new description wrapped in <new_description> tags.
Do not include explanations, bullet points, or extra XML tags."""
    return prompt


def improve_description(
    skill_name: str,
    skill_content: str,
    current_description: str,
    eval_results: dict[str, Any],
    history: list[dict[str, Any]],
    model: str,
    max_chars: int = 900,
    test_results: dict[str, Any] | None = None,
    log_dir: Path | None = None,
    iteration: int | None = None,
) -> dict[str, Any]:
    """Call Claude to propose a better description."""
    prompt = _build_prompt(
        skill_name=skill_name,
        skill_content=skill_content,
        current_description=current_description,
        eval_results=eval_results,
        history=history,
        max_chars=max_chars,
        test_results=test_results,
    )

    raw_response = _call_claude(prompt, model=model)
    match = re.search(r"<new_description>(.*?)</new_description>", raw_response, re.DOTALL | re.IGNORECASE)
    candidate = match.group(1) if match else raw_response
    candidate = _normalize_description(candidate)

    transcript: dict[str, Any] = {
        "iteration": iteration,
        "prompt": prompt,
        "raw_response": raw_response,
        "initial_candidate": candidate,
        "initial_length": len(candidate),
    }

    if len(candidate) > max_chars:
        shorten_prompt = (
            f"{prompt}\n\n"
            f"The previous candidate was {len(candidate)} characters long and exceeds the {max_chars} character limit.\n"
            f"Rewrite this candidate to fit the limit while preserving the most important trigger cues:\n"
            f"\"{candidate}\"\n\n"
            "Reply only with <new_description>...</new_description>."
        )
        shortened_response = _call_claude(shorten_prompt, model=model)
        match = re.search(
            r"<new_description>(.*?)</new_description>",
            shortened_response,
            re.DOTALL | re.IGNORECASE,
        )
        candidate = _normalize_description(match.group(1) if match else shortened_response)
        transcript["shorten_prompt"] = shorten_prompt
        transcript["shorten_response"] = shortened_response

    candidate = _trim_description(candidate, max_chars=max_chars)
    if not candidate:
        raise RuntimeError("Description optimizer returned an empty description")

    transcript["final_description"] = candidate
    transcript["final_length"] = len(candidate)

    if log_dir:
        log_dir.mkdir(parents=True, exist_ok=True)
        write_json(log_dir / f"improve_iter_{iteration or 'unknown'}.json", transcript)

    return {
        "description": candidate,
        "transcript": transcript,
    }


def main() -> int:
    parser = argparse.ArgumentParser(description="Improve a skill description from trigger-eval results")
    parser.add_argument("--eval-results", required=True, help="Path to run_eval JSON output")
    parser.add_argument("--skill-path", required=True, help="Path to the skill directory")
    parser.add_argument("--history", default=None, help="Path to previous iteration history JSON")
    parser.add_argument("--test-results", default=None, help="Optional held-out test result JSON")
    parser.add_argument("--model", required=True, help="Model passed to claude -p")
    parser.add_argument("--max-chars", type=int, default=900, help="Soft maximum description length")
    parser.add_argument("--output", default=None, help="Write result JSON to this path")
    parser.add_argument("--log-dir", default=None, help="Directory for transcripts/logs")
    parser.add_argument("--verbose", action="store_true", help="Print diagnostics to stderr")
    args = parser.parse_args()

    skill_path = Path(args.skill_path).resolve()
    if not (skill_path / "SKILL.md").exists():
        print(f"Error: No SKILL.md found at {skill_path}", file=sys.stderr)
        return 1

    eval_results = load_json(args.eval_results)
    history = load_json(args.history) if args.history else []
    if not isinstance(history, list):
        print("Error: --history must point to a JSON list", file=sys.stderr)
        return 1
    test_results = load_json(args.test_results) if args.test_results else None

    name, _, content = parse_skill_md(skill_path)
    current_description = str(eval_results.get("description", "")).strip()
    if not current_description:
        print("Error: eval results do not contain a description", file=sys.stderr)
        return 1

    improved = improve_description(
        skill_name=name,
        skill_content=content,
        current_description=current_description,
        eval_results=eval_results,
        history=history,
        model=args.model,
        max_chars=args.max_chars,
        test_results=test_results,
        log_dir=Path(args.log_dir).resolve() if args.log_dir else None,
    )

    output = {
        "description": improved["description"],
        "history": history
        + [
            {
                "description": current_description,
                "passed": eval_results["summary"]["passed"],
                "failed": eval_results["summary"]["failed"],
                "total": eval_results["summary"]["total"],
                "results": eval_results["results"],
            }
        ],
        "transcript": improved["transcript"],
    }

    if args.verbose:
        print(
            f"Improved description ({len(improved['description'])} chars): {improved['description']}",
            file=sys.stderr,
        )

    rendered = json.dumps(output, indent=2, ensure_ascii=False)
    print(rendered)
    if args.output:
        write_json(args.output, output)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
