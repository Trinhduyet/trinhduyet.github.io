from __future__ import annotations

import datetime as dt
import re
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DOCS = ROOT / "docs"
LABS = ROOT / "labs"
MKDOCS = ROOT / "mkdocs.yml"

HIGH_LEVEL_STATUS = [
    DOCS / "index.md",
    DOCS / "00-roadmap" / "master-roadmap.md",
    DOCS / "00-roadmap" / "repository-health.md",
]

CURRENT_ENTRY_PAGES = [
    DOCS / "index.md",
    DOCS / "00-roadmap" / "master-roadmap.md",
]

REQUIRED_NAV = {
    "00-roadmap/repository-health.md",
    "00-roadmap/technology-baseline.md",
    "13-devops-iac/terraform-language-lifecycle-and-validation.md",
    "13-devops-iac/terraform-team-workflows-and-state-recovery.md",
    "13-devops-iac/terraform-on-azure-production.md",
    "14-cloud/azure-core-platform-control-plane-and-governance.md",
    "14-cloud/azure-service-selection-and-workload-architecture.md",
    "14-cloud/azure-governance-security-and-operations-playbook.md",
    "19-ai-engineering/ai-engineer-vocabulary-and-system-boundaries.md",
    "19-ai-engineering/llm-runtime-messages-context-and-reasoning.md",
    "19-ai-engineering/microsoft-extensions-ai-dotnet-integration.md",
    "21-ai-coding-agents/ai-coding-agent-vocabulary-context-and-handoffs.md",
    "21-ai-coding-agents/safe-agentic-coding-workflow.md",
}

# Intentional cross-surface entry; duplicates outside this allow-list are likely nav drift.
ALLOWED_DUPLICATE_NAV = {
    "00-roadmap/learning-quality-standard.md",
}


def lab_directories() -> list[str]:
    return sorted(
        str(path.relative_to(ROOT)).replace("\\", "/")
        for path in LABS.iterdir()
        if path.is_dir() and any(item.is_file() for item in path.rglob("*"))
    )


def nav_targets(text: str) -> list[str]:
    return re.findall(r":\s+([0-9A-Za-z_.\/-]+\.md)\s*$", text, flags=re.MULTILINE)


def main() -> int:
    errors: list[str] = []
    warnings: list[str] = []

    mkdocs_text = MKDOCS.read_text(encoding="utf-8")
    targets = nav_targets(mkdocs_text)
    target_set = set(targets)

    missing_nav = sorted(REQUIRED_NAV - target_set)
    for path in missing_nav:
        errors.append(f"required learning-path page missing from mkdocs nav: {path}")

    for target, count in sorted(Counter(targets).items()):
        if count > 1 and target not in ALLOWED_DUPLICATE_NAV:
            errors.append(f"duplicate mkdocs nav target ({count}x): {target}")

    labs = lab_directories()
    for status_file in HIGH_LEVEL_STATUS:
        text = status_file.read_text(encoding="utf-8")
        for lab in labs:
            if lab not in text:
                errors.append(f"{status_file.relative_to(ROOT)} does not mention current lab: {lab}")

    current_entry_text = "\n".join(
        path.read_text(encoding="utf-8") for path in CURRENT_ENTRY_PAGES
    )
    if "repository-quality-review-2026-08-28.md" in current_entry_text:
        errors.append(
            "current high-level status pages still link the historical 2026-08-28 review; "
            "link repository-health.md instead"
        )

    baseline = DOCS / "00-roadmap" / "technology-baseline.md"
    baseline_text = baseline.read_text(encoding="utf-8")
    match = re.search(r"Baseline reviewed (\d{4}-\d{2}-\d{2})", baseline_text)
    if not match:
        errors.append("technology baseline is missing 'Baseline reviewed YYYY-MM-DD'")
    else:
        reviewed = dt.date.fromisoformat(match.group(1))
        age = (dt.date.today() - reviewed).days
        if age > 45:
            warnings.append(
                f"technology baseline is {age} days old; refresh fast-moving rows"
            )

    print("Site consistency audit")
    print("======================")
    print(f"Runnable labs discovered: {len(labs)}")
    for lab in labs:
        print(f"- {lab}")
    print(f"Nav targets discovered: {len(targets)}")

    for warning in warnings:
        print(f"::warning::{warning}")

    if errors:
        for error in errors:
            print(f"::error::{error}")
        return 1

    print("Repository status, navigation and evidence are consistent.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
