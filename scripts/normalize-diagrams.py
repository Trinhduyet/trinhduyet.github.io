from __future__ import annotations

import html
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DIAGRAMS = ROOT / "docs" / "assets" / "diagrams"

# Existing editorial SVGs are intentionally hand-authored and mostly use
# 120px boxes. A few legacy generated files contain a single-line title or
# subtitle that is wider than the box. This build-time normalization keeps
# the source design intact while preventing visible overflow on Pages.
TEXT_RE = re.compile(
    r'<text(?P<attrs>[^>]*)class="(?P<class>[^"]+)"(?P<attrs2>[^>]*)>'
    r'(?P<body>[^<]+)</text>'
)


def normalize(match: re.Match[str]) -> str:
    attrs = match.group("attrs") + match.group("attrs2")
    classes = set(match.group("class").split())
    body = match.group("body")
    plain = html.unescape(body).strip()

    if "textLength=" in attrs or not plain:
        return match.group(0)

    # Ignore edge labels/callouts. Only constrain box content.
    if not ({"node-title", "title", "sub"} & classes):
        return match.group(0)

    # Approximate width budget for legacy 120px boxes. New diagrams with
    # wider boxes remain unchanged because their labels are already wrapped.
    if "sub" in classes:
        threshold = 26
        target = 106
    else:
        threshold = 20
        target = 106

    if len(plain) <= threshold:
        return match.group(0)

    # Keep explicit x/y/text-anchor and inject SVG text fitting attributes.
    # lengthAdjust only applies when the label would otherwise overflow.
    class_attr = f'class="{match.group("class")}"'
    before_class = match.group("attrs")
    after_class = match.group("attrs2")
    return (
        f'<text{before_class}{class_attr}{after_class} '
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

print(f"Diagram normalization complete: {changed} SVG file(s) adjusted for text overflow.")
