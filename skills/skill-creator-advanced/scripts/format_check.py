#!/usr/bin/env python3
"""Skill format checker (linter)

This script performs *format* and *structure* checks that are commonly the
cause of skill upload/trigger problems.

It is intentionally dependency-light (PyYAML only) and safe to run locally or
in CI.

Exit codes:
  0: no errors (warnings may exist)
  1: errors found

Usage:
  python format_check.py <path/to/skill-folder>
  python format_check.py <path/to/skill-folder> --strict
  python format_check.py <path/to/skill-folder> --fix

Checks (errors):
  - SKILL.md exists (case-sensitive)
  - Root folder name is kebab-case
  - README.md is not present in the skill root
  - YAML frontmatter exists and parses
  - Required fields: name, description
  - name is kebab-case, <= 64 chars, not reserved
  - description is string, <= 1024 chars, no angle brackets
  - no angle brackets (< or >) anywhere in YAML frontmatter

Checks (warnings):
  - folder name != frontmatter.name
  - description missing obvious trigger language ("Use when" / "適用於" etc.)
  - missing blank line after frontmatter
  - trailing whitespace / tabs / missing final newline in common text files
  - python scripts fail to compile
  - SKILL.md body is empty
  - SKILL.md word count > 5000

The goal is practical signal, not perfect linting.
"""

from __future__ import annotations

import argparse
import re
import sys
from dataclasses import dataclass
from pathlib import Path

import yaml


ALLOWED_PROPERTIES = {
    "name",
    "description",
    "license",
    "allowed-tools",
    "metadata",
    "compatibility",
    "version",
}

RESERVED_NAME_FRAGMENTS = ("claude", "anthropic")

# Heuristic trigger language check (warning-only)
TRIGGER_HINT_PATTERNS = (
    r"\\bUse when\\b",
    r"\\bUse for\\b",
    r"\\bwhen user\\b",
    r"\\btrigger\\b",
    r"適用於",
    r"當使用者",
    r"用於",
    r"觸發",
)

TEXT_EXTS = {
    ".md",
    ".py",
    ".sh",
    ".txt",
    ".yaml",
    ".yml",
    ".json",
    ".toml",
    ".js",
    ".ts",
    ".tsx",
    ".jsx",
}


@dataclass
class Finding:
    level: str  # "ERROR" | "WARN"
    message: str
    path: Path | None = None
    line: int | None = None

    def format(self) -> str:
        loc = ""
        if self.path is not None:
            loc += str(self.path)
            if self.line is not None:
                loc += f":{self.line}"
            loc += ": "
        return f"[{self.level}] {loc}{self.message}"


def is_kebab_case(name: str) -> bool:
    # Strict kebab-case: segments separated by single hyphen, lower/num only.
    return re.fullmatch(r"[a-z0-9]+(?:-[a-z0-9]+)*", name) is not None


def extract_frontmatter(skill_md_text: str) -> tuple[str, str, int] | None:
    """Return (frontmatter_text, body_text, end_line_idx_1based) or None."""
    lines = skill_md_text.splitlines()
    if not lines or lines[0].strip() != "---":
        return None

    end_idx = None
    for i in range(1, len(lines)):
        if lines[i].strip() == "---":
            end_idx = i
            break

    if end_idx is None:
        return None

    frontmatter = "\n".join(lines[1:end_idx]) + "\n"
    body = "\n".join(lines[end_idx + 1 :])
    return frontmatter, body, end_idx + 1  # 1-based line number


def read_text_safely(path: Path) -> str | None:
    try:
        return path.read_text(encoding="utf-8")
    except UnicodeDecodeError:
        # Not a UTF-8 text file; skip.
        return None


def check_text_file_format(path: Path, text: str, fix: bool) -> list[Finding]:
    findings: list[Finding] = []

    lines = text.splitlines(keepends=True)
    changed = False
    new_lines = []

    for idx, raw in enumerate(lines, start=1):
        line = raw.rstrip("\n")

        if "\t" in line:
            findings.append(Finding("WARN", "Contains tab character(s)", path, idx))

        # trailing whitespace (space or tab)
        if re.search(r"[ \t]+$", line):
            findings.append(Finding("WARN", "Trailing whitespace", path, idx))
            if fix:
                line = re.sub(r"[ \t]+$", "", line)
                changed = True

        # Preserve original newline if present
        newline = "\n" if raw.endswith("\n") else ""
        new_lines.append(line + newline)

    # Ensure final newline
    if text and not text.endswith("\n"):
        findings.append(Finding("WARN", "Missing final newline", path))
        if fix:
            new_lines.append("\n")
            changed = True

    if fix and changed:
        try:
            path.write_text("".join(new_lines), encoding="utf-8")
        except Exception as e:
            findings.append(Finding("ERROR", f"Failed to apply --fix: {e}", path))

    return findings


def compile_python(path: Path) -> str | None:
    import py_compile

    try:
        py_compile.compile(str(path), doraise=True)
        return None
    except py_compile.PyCompileError as e:
        return str(e)


def main() -> int:
    ap = argparse.ArgumentParser(description="Skill format checker")
    ap.add_argument("skill_dir", help="Path to skill folder")
    ap.add_argument(
        "--strict",
        action="store_true",
        help="Treat some warnings (name mismatch, missing trigger hints) as errors",
    )
    ap.add_argument(
        "--fix",
        action="store_true",
        help="Auto-fix safe whitespace/newline issues in common text files",
    )
    args = ap.parse_args()

    skill_dir = Path(args.skill_dir).resolve()
    findings: list[Finding] = []

    if not skill_dir.exists():
        print(f"[ERROR] Skill folder not found: {skill_dir}")
        return 1
    if not skill_dir.is_dir():
        print(f"[ERROR] Path is not a directory: {skill_dir}")
        return 1

    # Folder name checks
    folder_name = skill_dir.name
    if not is_kebab_case(folder_name):
        findings.append(Finding("ERROR", "Skill folder name must be kebab-case (lowercase letters/digits separated by single hyphens)", skill_dir))

    # Forbidden root files
    if (skill_dir / "README.md").exists():
        findings.append(Finding("ERROR", "README.md must NOT be inside the skill folder (keep repo-level README outside the skill directory)", skill_dir / "README.md"))

    # Required file
    skill_md = skill_dir / "SKILL.md"
    if not skill_md.exists():
        findings.append(Finding("ERROR", "SKILL.md not found (must be exactly 'SKILL.md', case-sensitive)", skill_dir))
        # If SKILL.md missing, further checks are less useful
        for f in findings:
            print(f.format())
        return 1

    skill_md_text = read_text_safely(skill_md)
    if skill_md_text is None:
        findings.append(Finding("ERROR", "SKILL.md is not valid UTF-8 text", skill_md))
        for f in findings:
            print(f.format())
        return 1

    # Frontmatter
    fm = extract_frontmatter(skill_md_text)
    if fm is None:
        findings.append(Finding("ERROR", "Missing or invalid YAML frontmatter. SKILL.md must start with '---' and include a closing '---' delimiter", skill_md, 1))
        for f in findings:
            print(f.format())
        return 1

    frontmatter_text, body_text, fm_end_line = fm

    if "<" in frontmatter_text or ">" in frontmatter_text:
        findings.append(Finding("ERROR", "Angle brackets (< or >) are forbidden in YAML frontmatter", skill_md))

    # Parse YAML
    try:
        frontmatter = yaml.safe_load(frontmatter_text)
    except yaml.YAMLError as e:
        findings.append(Finding("ERROR", f"Invalid YAML in frontmatter: {e}", skill_md))
        frontmatter = None

    if not isinstance(frontmatter, dict):
        findings.append(Finding("ERROR", "Frontmatter must be a YAML mapping (dictionary)", skill_md))
        frontmatter = None

    if frontmatter is not None:
        # Allowed keys
        unexpected = set(frontmatter.keys()) - ALLOWED_PROPERTIES
        if unexpected:
            findings.append(
                Finding(
                    "ERROR",
                    f"Unexpected key(s) in frontmatter: {', '.join(sorted(unexpected))}. Allowed: {', '.join(sorted(ALLOWED_PROPERTIES))}",
                    skill_md,
                )
            )

        # Required fields
        if "name" not in frontmatter:
            findings.append(Finding("ERROR", "Missing required field 'name' in frontmatter", skill_md))
        if "description" not in frontmatter:
            findings.append(Finding("ERROR", "Missing required field 'description' in frontmatter", skill_md))

        # name validation
        name = frontmatter.get("name")
        if name is not None and not isinstance(name, str):
            findings.append(Finding("ERROR", f"Frontmatter 'name' must be a string, got {type(name).__name__}", skill_md))
        if isinstance(name, str):
            name_str = name.strip()
            if not name_str:
                findings.append(Finding("ERROR", "Frontmatter 'name' cannot be empty", skill_md))
            else:
                if not is_kebab_case(name_str):
                    findings.append(Finding("ERROR", f"Frontmatter 'name' must be kebab-case. Got: {name_str!r}", skill_md))
                if len(name_str) > 64:
                    findings.append(Finding("ERROR", f"Frontmatter 'name' is too long ({len(name_str)} > 64)", skill_md))
                lowered = name_str.lower()
                if any(r in lowered for r in RESERVED_NAME_FRAGMENTS):
                    findings.append(Finding("ERROR", "Skill names containing 'claude' or 'anthropic' are reserved and not allowed", skill_md))

                # name vs folder name
                if name_str != folder_name:
                    msg = f"Folder name ({folder_name!r}) does not match frontmatter name ({name_str!r}). Recommended to match exactly."
                    findings.append(Finding("ERROR" if args.strict else "WARN", msg, skill_dir))

        # description validation
        desc = frontmatter.get("description")
        if desc is not None and not isinstance(desc, str):
            findings.append(Finding("ERROR", f"Frontmatter 'description' must be a string, got {type(desc).__name__}", skill_md))
        if isinstance(desc, str):
            desc_str = desc.strip()
            if not desc_str:
                findings.append(Finding("ERROR", "Frontmatter 'description' cannot be empty", skill_md))
            else:
                if len(desc_str) > 1024:
                    findings.append(Finding("ERROR", f"Frontmatter 'description' is too long ({len(desc_str)} > 1024)", skill_md))
                if "<" in desc_str or ">" in desc_str:
                    findings.append(Finding("ERROR", "Frontmatter 'description' cannot contain angle brackets (< or >)", skill_md))

                # trigger language heuristic
                if not any(re.search(p, desc_str, flags=re.IGNORECASE) for p in TRIGGER_HINT_PATTERNS):
                    msg = "Description may be missing explicit trigger language (e.g., 'Use when user ...' / '適用於…'). This can cause under-triggering."
                    findings.append(Finding("ERROR" if args.strict else "WARN", msg, skill_md))

    # Blank line after frontmatter
    # Find the exact line after closing ---
    skill_lines = skill_md_text.splitlines()
    if fm_end_line < len(skill_lines):
        next_line = skill_lines[fm_end_line].strip("\r\n")
        if next_line != "":
            findings.append(Finding("WARN", "Recommended: add a blank line after the closing frontmatter delimiter (---)", skill_md, fm_end_line + 1))

    # Body non-empty
    if not body_text.strip():
        findings.append(Finding("WARN", "SKILL.md body appears empty (instructions missing)", skill_md))

    # Word count heuristic
    words = re.findall(r"\S+", skill_md_text)
    if len(words) > 5000:
        findings.append(Finding("WARN", f"SKILL.md is large ({len(words)} words). Consider moving detail to references/ for progressive disclosure", skill_md))

    # Text-format checks across common text files
    for fp in skill_dir.rglob("*"):
        if not fp.is_file():
            continue
        if fp.suffix.lower() not in TEXT_EXTS:
            continue
        text = read_text_safely(fp)
        if text is None:
            continue
        findings.extend(check_text_file_format(fp, text, fix=args.fix))

    # Python script compile checks (warning)
    scripts_dir = skill_dir / "scripts"
    if scripts_dir.exists() and scripts_dir.is_dir():
        for py in scripts_dir.rglob("*.py"):
            err = compile_python(py)
            if err:
                findings.append(Finding("WARN", f"Python script does not compile: {err}", py))

    # Emit results
    errors = [f for f in findings if f.level == "ERROR"]
    warns = [f for f in findings if f.level == "WARN"]

    # Stable ordering
    for f in sorted(findings, key=lambda x: (x.level, str(x.path or ""), x.line or 0, x.message)):
        print(f.format())

    print(
        "\nSummary: "
        f"{len(errors)} error(s), {len(warns)} warning(s). "
        + ("(strict mode)" if args.strict else "")
        + (" (fixed)" if args.fix else "")
    )

    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main())
