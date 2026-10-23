# NLO IQ project progress

Updated: 2026-10-23 (Asia/Baghdad).

Current phase: **0 â€” Discovery and Planning**.
Status: **Delivered for review; awaiting explicit approval**.
Approved phases: none.

## Completed

- Read the master project brief; confirmed phase-by-phase approval gates.
- Inspected the empty workspace and installed toolchain.
- Confirmed running MySQL80 service and reachable local TCP port 3306; no authenticated database check.
- Documented architecture, initial schema, revision isolation, localization, themes, security, roadmap and missing assets in PHASE_0_DISCOVERY.md.
- Created progress, decision and change records.

## Pending

- User review of Phase 0. Do not start Phase 1 without explicit approval.
- Phase 1: two distinct design compositions and design system.
- Phases 2â€“12: application foundation through deployment handover, each separately approved.

## Known limitations

- No application code, runnable preview, screenshots, builds or application tests yet.
- EF Core 10/MySQL provider compatibility and exact package pins are unresolved.
- MySQL credentials, schema, patch version and authenticated connection are unverified.
- Docker daemon is unavailable; native MySQL is present.
- Company assets/content and hosting information are missing.
- Git initialized on main at user request; local checkpoint prepared. User selected https://github.com/nloiq/ai.git as origin; synchronization is verified through Git remote tracking.

## Review commands

APPROVE PHASE advances one phase. REVISE modifies the current phase. SHOW PREVIEW reports the latest actual preview. EXPLAIN makes no changes. STATUS reports progress. ROLLBACK requires confirming discarded work; no approved checkpoint currently exists.

## User-requested setup revision

- Supplied database connection saved in ignored appsettings.Local.json; tracked example contains no password.
- Credentials supplied; authenticated database check remains pending Phase 2.
- User initially requested nloiq/NLOIQ, then explicitly selected existing repository https://github.com/nloiq/ai.git.
- Origin configured as https://github.com/nloiq/ai.git. Remote had no refs before the initial push. Use git status -sb and git ls-remote origin refs/heads/main to verify synchronization. Repository visibility was not changed.
- Phase 0 approval remains pending; no advancement to Phase 1.


