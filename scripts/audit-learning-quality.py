from __future__ import annotations

import os
import re
from dataclasses import dataclass
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "docs"
LABS = ROOT / "labs"
STRICT = os.getenv("DOCS_LEARNING_QUALITY_STRICT", "0") == "1"

MODULE_DIR = re.compile(r"^(?P<number>\d{2})-(?P<slug>.+)$")


@dataclass(frozen=True)
class ModuleAudit:
    number: str
    slug: str
    path: Path
    has_readme: bool
    has_references: bool
    has_lab: bool
    has_exit_signal: bool
    has_evidence_signal: bool
    has_verification_signal: bool
    has_failure_signal: bool


def contains_any(text: str, patterns: tuple[str, ...]) -> bool:
    lowered = text.lower()
    return any(pattern in lowered for pattern in patterns)


def audit_module(path: Path, number: str, slug: str) -> ModuleAudit:
    readme = path / "README.md"
    references = path / "references.md"
    text = readme.read_text(encoding="utf-8") if readme.exists() else ""

    lab_path = LABS / f"{number}-{slug}"

    return ModuleAudit(
        number=number,
        slug=slug,
        path=path,
        has_readme=readme.exists(),
        has_references=references.exists(),
        has_lab=lab_path.exists() and any(lab_path.rglob("*")),
        has_exit_signal=contains_any(
            text,
            (
                "exit criteria",
                "exit criterion",
                "definition of done",
                "hoàn thành module",
                "đạt module",
            ),
        ),
        has_evidence_signal=contains_any(
            text,
            (
                "evidence",
                "bằng chứng",
                "failure drill",
                "failure experiment",
                "portfolio",
            ),
        ),
        has_verification_signal=contains_any(
            text,
            ("verification metadata", "verified:", "updated:"),
        ),
        has_failure_signal=contains_any(
            text,
            (
                "failure",
                "timeout",
                "rollback",
                "debug",
                "incident",
                "outage",
            ),
        ),
    )


def main() -> int:
    modules: list[ModuleAudit] = []

    for path in sorted(DOCS.iterdir()):
        if not path.is_dir():
            continue
        match = MODULE_DIR.match(path.name)
        if not match:
            continue
        modules.append(
            audit_module(path, match.group("number"), match.group("slug"))
        )

    warnings: list[str] = []

    print("Learning quality audit")
    print("======================")
    print(
        "Module | README | References | Runnable lab | Exit | Evidence | Failure | Verified"
    )
    print("--- | --- | --- | --- | --- | --- | --- | ---")

    for module in modules:
        def mark(value: bool) -> str:
            return "yes" if value else "no"

        print(
            f"{module.number}-{module.slug} | "
            f"{mark(module.has_readme)} | "
            f"{mark(module.has_references)} | "
            f"{mark(module.has_lab)} | "
            f"{mark(module.has_exit_signal)} | "
            f"{mark(module.has_evidence_signal)} | "
            f"{mark(module.has_failure_signal)} | "
            f"{mark(module.has_verification_signal)}"
        )

        if not module.has_readme:
            warnings.append(f"{module.path}: missing README.md")
            continue

        # references.md is preferred but some focused tracks intentionally keep
        # a small canonical source section in README. Keep this informational.
        if not module.has_references:
            warnings.append(
                f"{module.path}: no references.md; ensure README has canonical sources"
            )

        if not module.has_exit_signal:
            warnings.append(f"{module.path}/README.md: no clear exit/DoD signal")
        if not module.has_evidence_signal:
            warnings.append(f"{module.path}/README.md: no evidence/failure-drill signal")
        if not module.has_failure_signal:
            warnings.append(f"{module.path}/README.md: no visible failure/debugging signal")
        if not module.has_verification_signal:
            warnings.append(f"{module.path}/README.md: no verification/update metadata")

    runnable = [m for m in modules if m.has_lab]
    guided_only = [m for m in modules if m.has_readme and not m.has_lab]

    print()
    print(f"Modules discovered: {len(modules)}")
    print(f"Modules with runnable labs under labs/: {len(runnable)}")
    print(f"Modules without dedicated runnable labs: {len(guided_only)}")

    if runnable:
        print(
            "Runnable lab coverage: "
            + ", ".join(f"{m.number}-{m.slug}" for m in runnable)
        )

    if warnings:
        print()
        print("Learning quality debt:")
        for warning in warnings:
            print(f"::warning::{warning}")

        print()
        print(
            "Warnings are non-blocking by default. "
            "Set DOCS_LEARNING_QUALITY_STRICT=1 when legacy debt is intentionally "
            "being enforced. See docs/00-roadmap/learning-quality-standard.md."
        )

        if STRICT:
            return 1
    else:
        print("Learning quality audit found no structural warnings.")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
