#!/usr/bin/env python3
"""
Quick validation script for skills - minimal version
"""

import sys
import re
from pathlib import Path

try:
    import yaml
except ModuleNotFoundError:  # pragma: no cover - exercised via package_skill tests
    yaml = None


class FrontmatterParseError(ValueError):
    """Raised when frontmatter cannot be parsed."""


def _parse_scalar(value: str) -> str:
    stripped = value.strip()
    if not stripped:
        return ""
    if len(stripped) >= 2 and stripped[0] == stripped[-1] and stripped[0] in {'"', "'"}:
        return stripped[1:-1]
    return stripped


def _parse_frontmatter_without_yaml(frontmatter_text: str) -> dict[str, object]:
    frontmatter: dict[str, object] = {}
    current_key: str | None = None

    for raw_line in frontmatter_text.splitlines():
        if not raw_line.strip() or raw_line.lstrip().startswith("#"):
            continue

        stripped = raw_line.strip()
        is_indented = raw_line[:1].isspace()
        if is_indented:
            if current_key is None:
                raise FrontmatterParseError(f"Unexpected indentation: {raw_line}")
            container = frontmatter[current_key]
            if stripped.startswith("- "):
                if not isinstance(container, list):
                    frontmatter[current_key] = []
                    container = frontmatter[current_key]
                container.append(_parse_scalar(stripped[2:]))
                continue
            if ":" not in stripped:
                raise FrontmatterParseError(f"Invalid nested line: {raw_line}")
            if not isinstance(container, dict):
                frontmatter[current_key] = {}
                container = frontmatter[current_key]
            nested_key, nested_value = stripped.split(":", 1)
            container[nested_key.strip()] = _parse_scalar(nested_value)
            continue

        current_key = None
        if ":" not in stripped:
            raise FrontmatterParseError(f"Invalid frontmatter line: {raw_line}")

        key, raw_value = stripped.split(":", 1)
        key = key.strip()
        value = raw_value.strip()
        if not key:
            raise FrontmatterParseError(f"Missing key in line: {raw_line}")
        if value:
            frontmatter[key] = _parse_scalar(value)
            continue

        current_key = key
        frontmatter[key] = {}

    return frontmatter


def _load_frontmatter(frontmatter_text: str) -> dict[str, object]:
    if yaml is not None:
        frontmatter = yaml.safe_load(frontmatter_text)
    else:
        frontmatter = _parse_frontmatter_without_yaml(frontmatter_text)

    if not isinstance(frontmatter, dict):
        raise FrontmatterParseError("Frontmatter must be a YAML dictionary")
    return frontmatter


def validate_skill(skill_path):
    """Basic validation of a skill"""
    skill_path = Path(skill_path)

    # Check SKILL.md exists
    skill_md = skill_path / 'SKILL.md'
    if not skill_md.exists():
        return False, "SKILL.md not found"

    # Read and validate frontmatter
    try:
        content = skill_md.read_text(encoding="utf-8")
    except UnicodeDecodeError as exc:
        return False, f"SKILL.md must be valid UTF-8 text: {exc}"
    if not content.startswith('---'):
        return False, "No YAML frontmatter found"

    # Extract frontmatter
    match = re.match(r'^---\n(.*?)\n---', content, re.DOTALL)
    if not match:
        return False, "Invalid frontmatter format"

    frontmatter_text = match.group(1)

    # Parse YAML frontmatter
    try:
        frontmatter = _load_frontmatter(frontmatter_text)
    except FrontmatterParseError as e:
        return False, f"Invalid YAML in frontmatter: {e}"
    except Exception as e:
        yaml_error = getattr(yaml, "YAMLError", None)
        if yaml_error is not None and isinstance(e, yaml_error):
            return False, f"Invalid YAML in frontmatter: {e}"
        raise

    # Define allowed properties
    ALLOWED_PROPERTIES = {'name', 'description', 'license', 'allowed-tools', 'metadata', 'compatibility', 'version'}

    # Check for unexpected properties (excluding nested keys under metadata)
    unexpected_keys = set(frontmatter.keys()) - ALLOWED_PROPERTIES
    if unexpected_keys:
        return False, (
            f"Unexpected key(s) in SKILL.md frontmatter: {', '.join(sorted(unexpected_keys))}. "
            f"Allowed properties are: {', '.join(sorted(ALLOWED_PROPERTIES))}"
        )

    # Check required fields
    if 'name' not in frontmatter:
        return False, "Missing 'name' in frontmatter"
    if 'description' not in frontmatter:
        return False, "Missing 'description' in frontmatter"

    # Extract name for validation
    name = frontmatter.get('name', '')
    if not isinstance(name, str):
        return False, f"Name must be a string, got {type(name).__name__}"
    name = name.strip()
    if name:
        # Check naming convention (kebab-case: lowercase with hyphens)
        if not re.match(r'^[a-z0-9-]+$', name):
            return False, f"Name '{name}' should be kebab-case (lowercase letters, digits, and hyphens only)"
        if name.startswith('-') or name.endswith('-') or '--' in name:
            return False, f"Name '{name}' cannot start/end with hyphen or contain consecutive hyphens"
        # Check name length (max 64 characters per spec)
        if len(name) > 64:
            return False, f"Name is too long ({len(name)} characters). Maximum is 64 characters."

    # Extract and validate description
    description = frontmatter.get('description', '')
    if not isinstance(description, str):
        return False, f"Description must be a string, got {type(description).__name__}"
    description = description.strip()
    if description:
        # Check for angle brackets
        if '<' in description or '>' in description:
            return False, "Description cannot contain angle brackets (< or >)"
        # Check description length (max 1024 characters per spec)
        if len(description) > 1024:
            return False, f"Description is too long ({len(description)} characters). Maximum is 1024 characters."

    # Validate compatibility field if present (optional)
    compatibility = frontmatter.get('compatibility', '')
    if compatibility:
        if not isinstance(compatibility, str):
            return False, f"Compatibility must be a string, got {type(compatibility).__name__}"
        if len(compatibility) > 500:
            return False, f"Compatibility is too long ({len(compatibility)} characters). Maximum is 500 characters."

    return True, "Skill is valid!"

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: python quick_validate.py <skill_directory>")
        sys.exit(1)

    valid, message = validate_skill(sys.argv[1])
    print(message)
    sys.exit(0 if valid else 1)
