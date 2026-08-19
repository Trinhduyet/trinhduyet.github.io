from __future__ import annotations

import os
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "docs"

# Documentation commands should be portable from repository root.
# Existing legacy docs still contain a few machine-specific commands, so this
# audit reports them as warnings by default instead of blocking Pages deploy.
# Set DOCS_PORTABILITY_STRICT=1 when running the cleanup gate intentionally.
WINDOWS_CD = re.compile(r"(?im)^\s*(?:cd|set-location)\s+[A-Za-z]:[\\/]")
MACHINE_SPECIFIC_DEV = re.compile(r"(?i)E:[\\/]Documents[\\/]Dev[\\/]")
STRICT = os.getenv("DOCS_PORTABILITY_STRICT", "0") == "1"

violations: set[tuple[str, int, str]] = set()

for path in sorted(DOCS.rglob("*.md")):
    text = path.read_text(encoding="utf-8")
    relative = str(path.relative_to(ROOT))

    for match in WINDOWS_CD.finditer(text):
        line = text.count("\n", 0, match.start()) + 1
        violations.add((relative, line, "absolute Windows cd/Set-Location command"))

    for line_no, line_text in enumerate(text.splitlines(), start=1):
        if MACHINE_SPECIFIC_DEV.search(line_text) and re.search(
            r"\b(?:cd|set-location)\b", line_text, re.IGNORECASE
        ):
            violations.add((relative, line_no, "machine-specific E:/Documents/Dev path"))

if violations:
    print("Documentation portability debt detected:\n", file=sys.stderr)
    for path, line, message in sorted(violations):
        print(f"::warning file={path},line={line}::{message}", file=sys.stderr)

    print(
        "\nPrefer repository-relative commands such as "
        "`cd labs/03-dotnet/runtime-lab`. "
        "This audit is non-blocking for Pages until the legacy docs are migrated.",
        file=sys.stderr,
    )

    if STRICT:
        raise SystemExit(1)

    print(f"Portability audit completed with {len(violations)} warning(s).")
else:
    print("Documentation portability audit passed: no absolute local cd commands found.")
