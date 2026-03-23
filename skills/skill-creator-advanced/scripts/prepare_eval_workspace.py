#!/usr/bin/env python3
"""Prepare a paired eval workspace from assets/evals/evals.json.

Creates an iteration directory with one folder per eval and paired configuration
subdirectories such as with_skill/ and without_skill/ (or old_skill/).
"""

from __future__ import annotations

import argparse
import json
import re
from pathlib import Path

import yaml


EVALS_PATH_CANDIDATES = (
    Path("assets/evals/evals.json"),
    Path("evals/evals.json"),
)


def load_skill_name(skill_dir: Path) -> str:
    skill_md = skill_dir / "SKILL.md"
    if not skill_md.exists():
        raise FileNotFoundError(f"SKILL.md not found: {skill_md}")

    text = skill_md.read_text(encoding="utf-8")
    match = re.match(r"^---\n(.*?)\n---", text, re.DOTALL)
    if not match:
        return skill_dir.name

    frontmatter = yaml.safe_load(match.group(1))
    if isinstance(frontmatter, dict):
        name = str(frontmatter.get("name", "")).strip()
        if name:
            return name
    return skill_dir.name


def load_evals(skill_dir: Path) -> tuple[str, list[dict]]:
    evals_path = next(
        (skill_dir / candidate for candidate in EVALS_PATH_CANDIDATES if (skill_dir / candidate).exists()),
        None,
    )
    if evals_path is None:
        expected = ", ".join(str(skill_dir / candidate) for candidate in EVALS_PATH_CANDIDATES)
        raise FileNotFoundError(f"No eval definitions found. Checked: {expected}")

    data = json.loads(evals_path.read_text(encoding="utf-8"))
    if not isinstance(data, dict):
        raise ValueError(f"{evals_path.name} must contain a JSON object")

    evals = data.get("evals")
    if not isinstance(evals, list) or not evals:
        raise ValueError(f"{evals_path.name} must contain a non-empty 'evals' list")

    skill_name = str(data.get("skill_name") or load_skill_name(skill_dir)).strip() or skill_dir.name
    return skill_name, evals


def slugify(text: str) -> str:
    text = text.lower()
    text = re.sub(r"[^a-z0-9]+", "-", text)
    text = re.sub(r"-{2,}", "-", text).strip("-")
    return text or "eval"


def next_iteration_name(workspace_dir: Path) -> str:
    max_n = 0
    for path in workspace_dir.glob("iteration-*"):
        if not path.is_dir():
            continue
        try:
            max_n = max(max_n, int(path.name.split("-")[1]))
        except (IndexError, ValueError):
            continue
    return f"iteration-{max_n + 1}"


def build_eval_dir_name(eval_item: dict, index: int) -> str:
    eval_id = eval_item.get("id", index + 1)
    try:
        prefix = f"eval-{int(eval_id):03d}"
    except (TypeError, ValueError):
        prefix = f"eval-{index + 1:03d}"
    label = str(eval_item.get("name") or eval_item.get("eval_name") or prefix).strip()
    slug = slugify(label)[:40]
    return f"{prefix}-{slug}"


def prepare_workspace(
    skill_dir: Path,
    workspace_dir: Path | None,
    iteration_name: str | None,
    baseline_label: str,
    runs_per_config: int,
) -> Path:
    skill_name, evals = load_evals(skill_dir)

    if workspace_dir is None:
        workspace_dir = skill_dir.parent / f"{skill_name}-workspace"
    workspace_dir.mkdir(parents=True, exist_ok=True)

    if iteration_name is None:
        iteration_name = next_iteration_name(workspace_dir)

    iteration_dir = workspace_dir / iteration_name
    iteration_dir.mkdir(parents=True, exist_ok=False)

    configs = ["with_skill", baseline_label]
    for index, eval_item in enumerate(evals):
        eval_dir = iteration_dir / build_eval_dir_name(eval_item, index)
        eval_dir.mkdir(parents=True, exist_ok=False)

        metadata = {
            "eval_id": eval_item.get("id", index + 1),
            "eval_name": str(eval_item.get("name") or eval_item.get("eval_name") or eval_dir.name),
            "prompt": str(eval_item.get("prompt", "")),
            "expected_output": str(eval_item.get("expected_output", "")),
            "files": eval_item.get("files", []),
            "expectations": eval_item.get("expectations", []),
        }
        (eval_dir / "eval_metadata.json").write_text(
            json.dumps(metadata, indent=2, ensure_ascii=False) + "\n",
            encoding="utf-8",
        )

        for config in configs:
            config_dir = eval_dir / config
            if runs_per_config == 1:
                (config_dir / "outputs").mkdir(parents=True, exist_ok=True)
            else:
                for run_number in range(1, runs_per_config + 1):
                    (config_dir / f"run-{run_number}" / "outputs").mkdir(parents=True, exist_ok=True)

    return iteration_dir


def main() -> int:
    parser = argparse.ArgumentParser(description="Prepare paired eval workspace directories for a skill")
    parser.add_argument("skill_dir", help="Path to the skill folder")
    parser.add_argument("--workspace", help="Override workspace path (default: sibling <skill-name>-workspace)")
    parser.add_argument("--iteration", help="Iteration directory name (default: next iteration-N)")
    parser.add_argument(
        "--baseline-label",
        default="without_skill",
        help="Baseline configuration label, e.g. without_skill or old_skill",
    )
    parser.add_argument(
        "--runs-per-config",
        type=int,
        default=1,
        help="Number of run-N directories to pre-create for each configuration",
    )
    args = parser.parse_args()

    if args.runs_per_config < 1:
        raise SystemExit("--runs-per-config must be >= 1")

    skill_dir = Path(args.skill_dir).resolve()
    workspace_dir = Path(args.workspace).resolve() if args.workspace else None

    iteration_dir = prepare_workspace(
        skill_dir=skill_dir,
        workspace_dir=workspace_dir,
        iteration_name=args.iteration,
        baseline_label=args.baseline_label,
        runs_per_config=args.runs_per_config,
    )
    print(f"Created eval workspace: {iteration_dir}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
