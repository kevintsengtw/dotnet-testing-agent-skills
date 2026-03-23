#!/usr/bin/env python3
"""Skill Packager - Creates a distributable .skill file from a skill folder.

This script intentionally performs validation before packaging.

Usage:
  python scripts/package_skill.py <path/to/skill-folder> [output-directory]

Notes:
  - The .skill file is a zip archive with a .skill extension.
  - __pycache__/ and *.pyc are excluded from the package.
"""

from __future__ import annotations

import sys

# Avoid generating __pycache__/ during import and runtime.
sys.dont_write_bytecode = True

import zipfile
from pathlib import Path

from quick_validate import validate_skill


def _safe_print(*args: object, sep: str = " ", end: str = "\n", file=None) -> None:
    """Print without crashing on Windows consoles that cannot encode emoji."""

    stream = sys.stdout if file is None else file
    if stream is None:
        return

    text = sep.join(str(arg) for arg in args)
    try:
        print(text, end=end, file=stream)
    except UnicodeEncodeError:
        encoding = getattr(stream, "encoding", None) or "utf-8"
        safe_text = text.encode(encoding, errors="backslashreplace").decode(encoding)
        print(safe_text, end=end, file=stream)


def _should_exclude(file_path: Path) -> bool:
    parts = set(file_path.parts)
    if "__pycache__" in parts:
        return True
    if file_path.suffix == ".pyc":
        return True
    if file_path.name in {".DS_Store"}:
        return True
    return False


def package_skill(skill_path: str | Path, output_dir: str | Path | None = None) -> Path | None:
    """Package a skill folder into a .skill file."""

    skill_path = Path(skill_path).resolve()

    # Validate skill folder exists
    if not skill_path.exists():
        _safe_print(f"❌ Error: Skill folder not found: {skill_path}")
        return None

    if not skill_path.is_dir():
        _safe_print(f"❌ Error: Path is not a directory: {skill_path}")
        return None

    # Validate SKILL.md exists
    skill_md = skill_path / "SKILL.md"
    if not skill_md.exists():
        _safe_print(f"❌ Error: SKILL.md not found in {skill_path}")
        return None

    # Run validation before packaging
    _safe_print("🔍 Validating skill...")
    valid, message = validate_skill(skill_path)
    if not valid:
        _safe_print(f"❌ Validation failed: {message}")
        _safe_print("   Please fix the validation errors before packaging.")
        return None
    _safe_print(f"✅ {message}\n")

    # Determine output location
    skill_name = skill_path.name
    if output_dir:
        output_path = Path(output_dir).resolve()
        output_path.mkdir(parents=True, exist_ok=True)
    else:
        output_path = Path.cwd()

    skill_filename = output_path / f"{skill_name}.skill"

    # Create the .skill file (zip format)
    try:
        with zipfile.ZipFile(skill_filename, "w", zipfile.ZIP_DEFLATED) as zipf:
            for file_path in skill_path.rglob("*"):
                if not file_path.is_file():
                    continue
                if _should_exclude(file_path):
                    continue

                # relative path within the zip must include the skill folder name
                arcname = file_path.relative_to(skill_path.parent)
                zipf.write(file_path, arcname)
                _safe_print(f"  Added: {arcname}")

        _safe_print(f"\n✅ Successfully packaged skill to: {skill_filename}")
        return skill_filename

    except Exception as e:
        _safe_print(f"❌ Error creating .skill file: {e}")
        return None


def main() -> int:
    if len(sys.argv) < 2:
        _safe_print("Usage: python scripts/package_skill.py <path/to/skill-folder> [output-directory]")
        _safe_print("\nExample:")
        _safe_print("  python scripts/package_skill.py skills/public/my-skill")
        _safe_print("  python scripts/package_skill.py skills/public/my-skill ./dist")
        return 1

    skill_path = sys.argv[1]
    output_dir = sys.argv[2] if len(sys.argv) > 2 else None

    _safe_print(f"📦 Packaging skill: {skill_path}")
    if output_dir:
        _safe_print(f"   Output directory: {output_dir}")
    _safe_print()

    result = package_skill(skill_path, output_dir)
    return 0 if result else 1


if __name__ == "__main__":
    raise SystemExit(main())
