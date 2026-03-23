#!/usr/bin/env python3
"""Shared helpers for skill-creator-advanced scripts."""

from __future__ import annotations

import json
import re
from pathlib import Path
from typing import Any

import yaml


FRONTMATTER_RE = re.compile(r"^---\n(.*?)\n---\n?", re.DOTALL)


def extract_frontmatter(skill_md_text: str) -> tuple[dict[str, Any], str]:
    """Return parsed frontmatter and body text from SKILL.md content."""
    match = FRONTMATTER_RE.match(skill_md_text)
    if not match:
        raise ValueError("SKILL.md missing YAML frontmatter")

    frontmatter = yaml.safe_load(match.group(1))
    if not isinstance(frontmatter, dict):
        raise ValueError("SKILL.md frontmatter must be a YAML mapping")

    body = skill_md_text[match.end() :]
    return frontmatter, body


def parse_skill_md(skill_path: Path | str) -> tuple[str, str, str]:
    """Parse SKILL.md and return (name, description, full_content)."""
    skill_path = Path(skill_path)
    skill_md = skill_path if skill_path.name == "SKILL.md" else skill_path / "SKILL.md"
    content = skill_md.read_text(encoding="utf-8")
    frontmatter, _ = extract_frontmatter(content)

    name = str(frontmatter.get("name", "")).strip() or skill_md.parent.name
    description = str(frontmatter.get("description", "")).strip()
    return name, description, content


def update_skill_description(skill_path: Path | str, new_description: str) -> Path:
    """Update the description field inside SKILL.md frontmatter."""
    skill_path = Path(skill_path)
    skill_md = skill_path if skill_path.name == "SKILL.md" else skill_path / "SKILL.md"
    content = skill_md.read_text(encoding="utf-8")
    frontmatter, body = extract_frontmatter(content)

    frontmatter["description"] = str(new_description).strip()
    dumped_frontmatter = yaml.safe_dump(
        frontmatter,
        allow_unicode=True,
        sort_keys=False,
        default_flow_style=False,
    ).strip()

    normalized_body = body.lstrip("\n").rstrip()
    rebuilt = f"---\n{dumped_frontmatter}\n---\n"
    if normalized_body:
        rebuilt += f"\n{normalized_body}\n"
    else:
        rebuilt += "\n"

    skill_md.write_text(rebuilt, encoding="utf-8")
    return skill_md


def load_json(path: Path | str) -> Any:
    """Load a UTF-8 JSON file."""
    return json.loads(Path(path).read_text(encoding="utf-8"))


def write_json(path: Path | str, data: Any) -> Path:
    """Write a JSON file with UTF-8 encoding."""
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    return path
