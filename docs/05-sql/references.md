# References — Module 05 SQL

> [← Module overview](README.md)

## Official sources

- [SQL Server constraints](https://learn.microsoft.com/en-us/sql/relational-databases/tables/primary-and-foreign-key-constraints?view=sql-server-ver17)
- [SELECT Transact-SQL](https://learn.microsoft.com/en-us/sql/t-sql/queries/select-transact-sql?view=sql-server-ver17)
- [Locking and row versioning](https://learn.microsoft.com/en-us/sql/relational-databases/sql-server-transaction-locking-and-row-versioning-guide?view=sql-server-ver17)
- [Query Store](https://learn.microsoft.com/en-us/sql/relational-databases/performance/monitoring-performance-by-using-the-query-store?view=sql-server-ver17)
- [SQL Server indexes](https://learn.microsoft.com/en-us/sql/relational-databases/indexes/indexes?view=sql-server-ver17)

## Roadmap and repository

- [Master roadmap](../00-roadmap/master-roadmap.md)
- [Knowledge dependency graph](../00-roadmap/prerequisites.md)
- [Technology baseline](../00-roadmap/technology-baseline.md)
- [Source policy](../00-roadmap/source-policy.md)

## Source decisions

| Decision | Source class | Rule |
| --- | --- | --- |
| Core behavior/protocol | Official documentation/specification | Prefer normative/current source |
| Production trade-off | Official architecture guidance + measured evidence | Do not promote a blog benchmark to a guarantee |
| Version-sensitive API | Current versioned docs | Record version and refresh before deployment |
| Security/operations | Official security/ops guidance + threat model | Validate configuration and failure path |

## Vietnamese Resources

Community Vietnamese material may aid reading, but English official documentation remains canonical for behavior, version and security claims.

## Verification metadata

- Verified: 2026-08-11.
- Technology target: SQL content v1.
- Context7 queries used: none; callable tool unavailable in this run.
- Notes: links are source-of-truth candidates; learner evidence must be produced locally.
