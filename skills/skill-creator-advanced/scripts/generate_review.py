#!/usr/bin/env python3
"""Generate a standalone HTML review page for eval outputs."""

from __future__ import annotations

import argparse
import base64
import html
import json
from pathlib import Path


TEXT_EXTENSIONS = {
    ".txt", ".md", ".json", ".csv", ".py", ".js", ".ts", ".tsx", ".jsx",
    ".yaml", ".yml", ".xml", ".html", ".css", ".sh", ".toml",
}
IMAGE_EXTENSIONS = {".png", ".jpg", ".jpeg", ".gif", ".svg", ".webp"}


def load_json(path: Path) -> dict | None:
    if not path.exists():
        return None
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError):
        return None


def discover_eval_dirs(root: Path) -> list[Path]:
    if (root / "eval_metadata.json").exists():
        return [root]
    return sorted(path.parent for path in root.rglob("eval_metadata.json"))


def embed_file(path: Path) -> dict:
    ext = path.suffix.lower()
    if ext in TEXT_EXTENSIONS:
        content = path.read_text(encoding="utf-8", errors="replace")
        return {"name": path.name, "kind": "text", "content": content}
    if ext in IMAGE_EXTENSIONS:
        raw = path.read_bytes()
        mime = "image/svg+xml" if ext == ".svg" else f"image/{ext.lstrip('.')}"
        return {
            "name": path.name,
            "kind": "image",
            "src": f"data:{mime};base64,{base64.b64encode(raw).decode('ascii')}",
        }

    raw = path.read_bytes()
    return {
        "name": path.name,
        "kind": "binary",
        "href": f"data:application/octet-stream;base64,{base64.b64encode(raw).decode('ascii')}",
    }


def load_outputs(outputs_dir: Path) -> list[dict]:
    if not outputs_dir.exists():
        return []
    files = []
    for path in sorted(outputs_dir.iterdir()):
        if path.is_file():
            files.append(embed_file(path))
    return files


def collect_runs(eval_dir: Path) -> list[dict]:
    rows: list[dict] = []
    metadata = load_json(eval_dir / "eval_metadata.json") or {}
    for config_dir in sorted(path for path in eval_dir.iterdir() if path.is_dir()):
        run_dirs = sorted(config_dir.glob("run-*"))
        if run_dirs:
            for run_dir in run_dirs:
                rows.append(
                    {
                        "config": config_dir.name,
                        "run_label": run_dir.name,
                        "grading": load_json(run_dir / "grading.json"),
                        "outputs": load_outputs(run_dir / "outputs"),
                    }
                )
        elif (config_dir / "outputs").exists() or (config_dir / "grading.json").exists():
            rows.append(
                {
                    "config": config_dir.name,
                    "run_label": "run-1",
                    "grading": load_json(config_dir / "grading.json"),
                    "outputs": load_outputs(config_dir / "outputs"),
                }
            )
    return [
        {
            "eval_name": metadata.get("eval_name", eval_dir.name),
            "eval_id": metadata.get("eval_id", eval_dir.name),
            "prompt": metadata.get("prompt", ""),
            "expected_output": metadata.get("expected_output", ""),
            "expectations": metadata.get("expectations", []),
            "runs": rows,
        }
    ]


def render_output(file_info: dict) -> str:
    name = html.escape(file_info["name"])
    if file_info["kind"] == "text":
        return f"<h5>{name}</h5><pre>{html.escape(file_info['content'])}</pre>"
    if file_info["kind"] == "image":
        return f"<h5>{name}</h5><img src=\"{file_info['src']}\" alt=\"{name}\" />"
    return f"<h5>{name}</h5><p><a download=\"{name}\" href=\"{file_info['href']}\">Download file</a></p>"


def render_grading(grading: dict | None) -> str:
    if not grading:
        return "<p class=\"muted\">No grading.json found.</p>"

    summary = grading.get("summary", {})
    lines = [
        "<div class=\"grading\">",
        f"<p><strong>Pass rate:</strong> {summary.get('pass_rate', 0):.2f}</p>",
        f"<p><strong>Passed:</strong> {summary.get('passed', 0)} / {summary.get('total', 0)}</p>",
    ]
    expectations = grading.get("expectations", [])
    if expectations:
        lines.append("<ul>")
        for item in expectations:
            status = "PASS" if item.get("passed") else "FAIL"
            lines.append(
                "<li>"
                f"<strong>{status}</strong> {html.escape(str(item.get('text', '')))}"
                f"<br /><span class=\"muted\">{html.escape(str(item.get('evidence', '')))}</span>"
                "</li>"
            )
        lines.append("</ul>")
    lines.append("</div>")
    return "\n".join(lines)


def render_html(title: str, evals: list[dict], benchmark: dict | None) -> str:
    benchmark_html = ""
    if benchmark:
        summary = benchmark.get("run_summary", {})
        rows = []
        for config, stats in summary.items():
            if config == "delta":
                continue
            rows.append(
                "<tr>"
                f"<td>{html.escape(config)}</td>"
                f"<td>{stats.get('pass_rate', {}).get('mean', 0):.2f}</td>"
                f"<td>{stats.get('time_seconds', {}).get('mean', 0):.1f}</td>"
                f"<td>{stats.get('tokens', {}).get('mean', 0):.0f}</td>"
                "</tr>"
            )
        benchmark_html = (
            "<section><h2>Benchmark Summary</h2>"
            "<table><thead><tr><th>Config</th><th>Pass rate</th><th>Time (s)</th><th>Tokens</th></tr></thead>"
            f"<tbody>{''.join(rows)}</tbody></table></section>"
        )

    sections = []
    for eval_item in evals:
        run_blocks = []
        for run in eval_item["runs"]:
            outputs_html = "".join(render_output(item) for item in run["outputs"]) or "<p class=\"muted\">No output files.</p>"
            run_blocks.append(
                "<div class=\"run-card\">"
                f"<h4>{html.escape(run['config'])} / {html.escape(run['run_label'])}</h4>"
                f"{render_grading(run['grading'])}"
                f"{outputs_html}"
                "</div>"
            )

        expectations_html = "".join(f"<li>{html.escape(str(item))}</li>" for item in eval_item["expectations"])
        sections.append(
            "<section class=\"eval-section\">"
            f"<h2>{html.escape(str(eval_item['eval_name']))}</h2>"
            f"<p><strong>Prompt:</strong> {html.escape(str(eval_item['prompt']))}</p>"
            f"<p><strong>Expected output:</strong> {html.escape(str(eval_item['expected_output']))}</p>"
            f"<ul>{expectations_html}</ul>"
            f"<div class=\"run-grid\">{''.join(run_blocks)}</div>"
            "</section>"
        )

    return f"""<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <title>{html.escape(title)}</title>
  <style>
    body {{ font-family: Arial, sans-serif; margin: 24px; background: #f7f5ef; color: #222; }}
    h1, h2, h3, h4, h5 {{ margin-top: 0; }}
    section {{ background: #fff; border: 1px solid #ddd4c3; border-radius: 12px; padding: 18px; margin-bottom: 18px; }}
    table {{ border-collapse: collapse; width: 100%; }}
    th, td {{ border: 1px solid #ddd4c3; padding: 8px 10px; text-align: left; }}
    .run-grid {{ display: grid; grid-template-columns: repeat(auto-fit, minmax(320px, 1fr)); gap: 16px; }}
    .run-card {{ border: 1px solid #e1dbcf; border-radius: 10px; padding: 14px; background: #fcfbf8; }}
    .muted {{ color: #666; }}
    pre {{ background: #1f2430; color: #f8f8f2; padding: 12px; border-radius: 8px; overflow-x: auto; }}
    img {{ max-width: 100%; border: 1px solid #ddd4c3; border-radius: 8px; }}
  </style>
</head>
<body>
  <h1>{html.escape(title)}</h1>
  {benchmark_html}
  {''.join(sections)}
</body>
</html>
"""


def main() -> int:
    parser = argparse.ArgumentParser(description="Generate a standalone HTML review page from eval outputs")
    parser.add_argument("root", help="Iteration directory or any directory containing eval_metadata.json files")
    parser.add_argument("--benchmark", help="Optional benchmark.json path")
    parser.add_argument("--output", help="Output HTML path (default: <root>/review.html)")
    parser.add_argument("--title", default="", help="Custom page title")
    args = parser.parse_args()

    root = Path(args.root).resolve()
    eval_dirs = discover_eval_dirs(root)
    if not eval_dirs:
        raise SystemExit(f"No eval_metadata.json files found under {root}")

    evals: list[dict] = []
    for eval_dir in eval_dirs:
        evals.extend(collect_runs(eval_dir))

    benchmark_path = Path(args.benchmark).resolve() if args.benchmark else (root / "benchmark.json")
    benchmark = load_json(benchmark_path) if benchmark_path.exists() else None

    output_path = Path(args.output).resolve() if args.output else (root / "review.html")
    title = args.title or f"Eval Review: {root.name}"
    output_path.write_text(render_html(title, evals, benchmark), encoding="utf-8")
    print(f"Generated: {output_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
