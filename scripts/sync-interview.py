#!/usr/bin/env python3
"""Mirror the public stock-ai interview handbook into MkDocs at build time.

Source repo is canonical. This script runs only against a checked-out source tree.
Mermaid snippets are converted to readable source links because website CI forbids
mermaid fences; no claims of rendered diagrams are made.
"""
from __future__ import annotations

import posixpath
import re
import shutil
import sys
from pathlib import Path
from urllib.parse import urlsplit

ROOT = Path(__file__).resolve().parents[1]
SOURCE_REPO = "https://github.com/Trinhduyet/stock-ai"
REF = "docs/senior-dotnet-ir-interview-20261015"
SOURCE_ROOT = Path(sys.argv[1]).resolve() if len(sys.argv) > 1 else ROOT / ".cache" / "stock-ai-interview"
SRC = SOURCE_ROOT / "docs" / "interview"
DEST = ROOT / "docs" / "interview"

if not SRC.is_dir() or not (SRC / "README.md").is_file():
    raise SystemExit(f"Missing source handbook: {SRC}")

def is_inside_interview(target: str) -> bool:
    return target == "docs/interview" or target.startswith("docs/interview/")

def rewrite_link(raw_url: str, source_file: Path) -> str:
    parts = urlsplit(raw_url)
    if parts.scheme or raw_url.startswith(("#", "//", "mailto:")):
        return raw_url
    if not parts.path:
        return raw_url
    source_rel = source_file.relative_to(SOURCE_ROOT).as_posix()
    # POSIX normalization is independent of Linux/Windows build agents.
    target = posixpath.normpath(posixpath.join(posixpath.dirname(source_rel), parts.path))
    if is_inside_interview(target):
        destination = target.removeprefix("docs/interview/")
        if destination == "README.md":
            destination = "index.md"
        elif destination.endswith("/README.md"):
            destination = destination[:-9] + "index.md"
        dst_relative = posixpath.relpath(destination, posixpath.dirname(source_rel.removeprefix("docs/interview/")))
        suffix = ("?" + parts.query if parts.query else "") + ("#" + parts.fragment if parts.fragment else "")
        return dst_relative + suffix
    # Assets and code outside the handbook are linked to original GitHub source.
    kind = "tree" if parts.path.endswith("/") or "." not in posixpath.basename(target) else "blob"
    suffix = ("#" + parts.fragment) if parts.fragment else ""
    return f"{SOURCE_REPO}/{kind}/{REF}/{target}{suffix}"

# Match ordinary Markdown URLs without changing HTTP links.
MARKDOWN_LINK = re.compile(r"(!?\[[^\]\n]*\]\()([^\s)]+)(\))")
MERMAID = re.compile(r"(?ms)^\s*```mermaid\s*\n(.*?)^\s*```\s*$")

def convert(source_file: Path, text: str) -> str:
    text = MARKDOWN_LINK.sub(lambda m: m.group(1) + rewrite_link(m.group(2), source_file) + m.group(3), text)

    def mermaid_to_text(match: re.Match[str]) -> str:
        original = source_file.relative_to(SOURCE_ROOT).as_posix()
        remote = f"{SOURCE_REPO}/blob/{REF}/{original}"
        return (
            f"\n> **Sơ đồ:** [Xem Mermaid gốc trong source handbook]({remote}). "
            "Trang này hiển thị cú pháp mô tả thay cho ảnh render.\n\n"
            "```text\n" + match.group(1).rstrip() + "\n```\n"
        )

    text = MERMAID.sub(mermaid_to_text, text)
    if re.search(r"(?m)^\s*```mermaid\b", text):
        raise RuntimeError(f"Unconverted Mermaid snippet: {source_file}")
    # Prevent publication of local infrastructure addresses/paths in the public site.
    text = re.sub(r"(?i)E:[/\\]EuroLand[/\\]", "[local-euroland]/", text)
    text = re.sub(r"(?i)E:[/\\]github[/\\]", "[local-github]/", text)
    return text

if DEST.exists():
    shutil.rmtree(DEST)
DEST.mkdir(parents=True)
count = 0
for path in sorted(SRC.rglob("*")):
    if not path.is_file():
        continue
    relative = path.relative_to(SRC)
    if ".git" in relative.parts or relative.suffix.lower() in {".env", ".pfx", ".key", ".pem", ".csv"}:
        continue
    if path.stat().st_size > 4_000_000:
        raise RuntimeError(f"Oversized handbook asset: {relative}")
    out = DEST / relative
    if relative.name == "README.md":
        out = out.with_name("index.md")
    out.parent.mkdir(parents=True, exist_ok=True)
    if path.suffix.lower() == ".md":
        out.write_text(convert(path, path.read_text(encoding="utf-8")), encoding="utf-8")
    else:
        shutil.copy2(path, out)
    count += 1

if not (DEST / "index.md").exists():
    raise RuntimeError("Site entry docs/interview/index.md not found")
print(f"Synced {count} interview files from {SRC} to {DEST}")
