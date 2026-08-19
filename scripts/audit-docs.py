from __future__ import annotations

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "docs"

# Documentation commands must be portable from repository root.
# We deliberately target shell navigation commands so normal prose about
# Windows paths is not rejected.
WINDOWS_CD = re.compile(r"(?im)^\s*(?:cd|set-location)\s+[A-Za-z]:[\\/]")
MACHINE_SPECIFIC_DEV = re.compile(r"(?i)E:[\\/]Documents[\\/]Dev[\\/]")

violations: list[str] = []

for path in sorted(DOCS.rglob("*.md")):
    text = path.read_text(encoding="utf-8")
    for match in WINDOWS_CD.finditer(text):
        line = text.count("\n", 0, match.start()) + 1
        violations.append(
            f"{path.relative_to(ROOT)}:{line}: absolute Windows cd/Set-Location command"
        )

    # Catch the exact machine-specific workspace that caused the current bug
    # when it appears in an executable command line.
    for line_no, line_text in enumerate(text.splitlines(), start=1):
        if MACHINE_SPECIFIC_DEV.search(line_text) and re.search(
            r"\b(?:cd|set-location)\b", line_text, re.IGNORECASE
        ):
            violations.append(
                f"{path.relative_to(ROOT)}:{line_no}: machine-specific E:/Documents/Dev path"
            )

if violations:
    print("Documentation portability audit failed:\n", file=sys.stderr)
    for item in violations:
        print(f"- {item}", file=sys.stderr)
    print(
        "\nUse a repository-relative command such as "
        "`cd labs/03-dotnet/runtime-lab` instead.",
        file=sys.stderr,
    )
    raise SystemExit(1)

print("Documentation portability audit passed: no absolute local cd commands found.")
