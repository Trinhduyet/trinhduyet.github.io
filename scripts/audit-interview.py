#!/usr/bin/env python3
"""Validate the public Interview area before publishing to GitHub Pages."""
from __future__ import annotations

import re
import sys
from pathlib import Path
from urllib.parse import unquote, urlsplit

ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "docs"
INTERVIEW = DOCS / "interview"
MKDOCS = ROOT / "mkdocs.yml"

expected_question_counts = {
    "dotnet-api.md": ("NET", 9),
    "sql-data.md": ("DB", 9),
    "distributed-cloud.md": ("DIST", 9),
    "architecture-leadership.md": ("ARCH", 9),
}
pattern = re.compile(r"!?\[[^\]\n]*\]\(([^)\s]+)(?:\s+\"[^\"]+\")?\)")
prohibited = [
    (re.compile(r"(?i)E:[/\\]EuroLand[/\\]"), "local Euroland path"),
    (re.compile(r"\b10\.10\.15\.8\b"), "private database address"),
    (re.compile(r"(?i)(Password|ClientSecret)\s*="), "possible credential literal"),
]
errors: list[str] = []
files = sorted(INTERVIEW.rglob("*.md"))
if len(files) < 18:
    errors.append(f"Expected >=18 interview pages, found {len(files)}")

for file in files:
    content = file.read_text(encoding="utf-8")
    for regex, label in prohibited:
        if regex.search(content):
            errors.append(f"{file.relative_to(ROOT)} contains {label}")
    for match in pattern.finditer(content):
        raw = match.group(1)
        if raw.startswith(("#", "//")):
            continue
        url = urlsplit(raw)
        if url.scheme or url.netloc:
            continue
        path = unquote(url.path)
        if not path:
            continue
        target = (file.parent / path).resolve()
        if not target.is_relative_to(ROOT):
            errors.append(f"{file.relative_to(ROOT)}: link escapes checkout: {raw}")
        elif not (
            target.is_file()
            or (target.is_dir() and ((target / "index.md").is_file() or (target / "README.md").is_file()))
        ):
            errors.append(f"{file.relative_to(ROOT)}: missing local link: {raw}")

base = INTERVIEW / "05-interview-preparation"
for filename, (prefix, expected) in expected_question_counts.items():
    target = base / filename
    if not target.is_file():
        errors.append(f"Missing interview answer file: {filename}")
        continue
    content = target.read_text(encoding="utf-8")
    ids = re.findall(r"^##\s+(" + prefix + r"-\d{2})\.", content, flags=re.MULTILINE)
    if len(ids) != expected or len(set(ids)) != expected:
        errors.append(f"{filename}: expected {expected} unique IDs, got {len(ids)} / {len(set(ids))}")

site = MKDOCS.read_text(encoding="utf-8")
for target in set(re.findall(r":\s+(interview/[0-9a-zA-Z_\-/.]+\.md)\s*$", site, re.MULTILINE)):
    if not (DOCS / target).is_file():
        errors.append(f"mkdocs.yml missing Interview nav target: {target}")

if errors:
    for issue in errors:
        print("::error::" + issue, file=sys.stderr)
    raise SystemExit(1)

print(f"Interview audit passed: {len(files)} pages; 36 unique answers; local links and nav targets verified.")
