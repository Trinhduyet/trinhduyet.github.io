from __future__ import annotations

import os
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "docs"

# Documentation commands should be portable from repository root.
# Legacy docs may still contain machine-specific Windows paths. During the
# Pages build we normalize lab paths inside the runner workspace so readers
# never see local machine paths. The audit remains strict-capable for cleanup.
WINDOWS_CD_LINE = re.compile(
    r"(?im)^(?P<indent>\s*)(?P<cmd>cd|set-location)\s+(?P<path>[A-Za-z]:[\\/][^\r\n]+)$"
)
MACHINE_SPECIFIC_DEV = re.compile(r"(?i)E:[\\/]Documents[\\/]Dev[\\/]")
STRICT = os.getenv("DOCS_PORTABILITY_STRICT", "0") == "1"

violations: set[tuple[str, int, str]] = set()
normalized: list[tuple[str, int, str, str]] = []


def portable_lab_path(raw_path: str) -> str | None:
    """Return repo-relative labs/... path when an absolute path points at labs."""
    cleaned = raw_path.strip().strip('"').strip("'").replace("\\", "/")
    lowered = cleaned.lower()
    marker = "/labs/"
    index = lowered.find(marker)
    if index < 0:
        return None
    return cleaned[index + 1 :]


for path in sorted(DOCS.rglob("*.md")):
    text = path.read_text(encoding="utf-8")
    relative = str(path.relative_to(ROOT))
    changed = False

    def replace_command(match: re.Match[str]) -> str:
        nonlocal_changed = False
        raw_path = match.group("path")
        target = portable_lab_path(raw_path)
        if target is None:
            return match.group(0)

        line = text.count("\n", 0, match.start()) + 1
        command = "cd" if match.group("cmd").lower() == "cd" else "Set-Location"
        replacement = f"{match.group('indent')}{command} {target}"
        normalized.append((relative, line, match.group(0).strip(), replacement.strip()))
        return replacement

    new_text = WINDOWS_CD_LINE.sub(replace_command, text)
    if new_text != text:
        path.write_text(new_text, encoding="utf-8")
        text = new_text
        changed = True

    # Report any absolute Windows navigation command that could not be safely
    # normalized to a repository-relative labs/... path.
    for match in WINDOWS_CD_LINE.finditer(text):
        line = text.count("\n", 0, match.start()) + 1
        violations.add((relative, line, "absolute Windows cd/Set-Location command"))

    for line_no, line_text in enumerate(text.splitlines(), start=1):
        if MACHINE_SPECIFIC_DEV.search(line_text) and re.search(
            r"\b(?:cd|set-location)\b", line_text, re.IGNORECASE
        ):
            violations.add((relative, line_no, "machine-specific E:/Documents/Dev path"))

if normalized:
    print(f"Normalized {len(normalized)} legacy lab command(s) for this build:")
    for path, line, before, after in normalized:
        print(f"- {path}:{line}: {before} -> {after}")

if violations:
    print("Documentation portability debt detected:\n", file=sys.stderr)
    for path, line, message in sorted(violations):
        print(f"::warning file={path},line={line}::{message}", file=sys.stderr)

    print(
        "\nPrefer repository-relative commands such as "
        "`cd labs/03-dotnet/runtime-lab`. "
        "Unresolved legacy commands are warnings for Pages and can be made "
        "blocking with DOCS_PORTABILITY_STRICT=1.",
        file=sys.stderr,
    )

    if STRICT:
        raise SystemExit(1)

    print(f"Portability audit completed with {len(violations)} warning(s).")
else:
    print("Documentation portability audit passed after normalization.")
