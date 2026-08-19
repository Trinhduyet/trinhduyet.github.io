from __future__ import annotations

import html
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DIAGRAMS = ROOT / "docs" / "assets" / "diagrams"

# Legacy editorial SVGs mostly use 120px nodes with classes `node-title`
# and `sub`. A few generated labels are wider than their boxes. This
# build-time safety net constrains only legacy single-line labels; new SVGs
# should solve wrapping explicitly with wider boxes and/or <tspan> lines.
TEXT_RE = re.compile(
    r'<text(?P<attrs>[^>]*)class="(?P<class>[^"]+)"(?P<attrs2>[^>]*)>'
    r'(?P<body>[^<]+)</text>'
)


def normalize(match: re.Match[str]) -> str:
    before_class = match.group("attrs")
    after_class = match.group("attrs2")
    attrs = before_class + after_class
    classes = set(match.group("class").split())
    body = match.group("body")
    plain = html.unescape(body).strip()

    if "textLength=" in attrs or not plain:
        return match.group(0)

    # Only legacy generated box text is normalized automatically.
    # New hand-authored diagrams use `.title` and explicit <tspan> wrapping.
    if not ({"node-title", "sub"} & classes):
        return match.group(0)

    if "sub" in classes:
        threshold = 26
        target = 106
    else:
        threshold = 20
        target = 106

    if len(plain) <= threshold:
        return match.group(0)

    # Always emit whitespace before class so both of these inputs are valid:
    #   <text class="node-title" ...>
    #   <text x="..." class="node-title" ...>
    return (
        f'<text{before_class} class="{match.group("class")}"{after_class} '
        f'textLength="{target}" lengthAdjust="spacingAndGlyphs">'
        f'{body}</text>'
    )


changed = 0
for path in sorted(DIAGRAMS.glob("*.svg")):
    original = path.read_text(encoding="utf-8")
    updated = TEXT_RE.sub(normalize, original)
    if updated != original:
        path.write_text(updated, encoding="utf-8")
        changed += 1

print(f"Diagram normalization complete: {changed} legacy SVG file(s) adjusted for text overflow.")
