# NLO IQ decisions

Date: 2026-10-23. All architecture choices below are proposed pending Phase 0 approval.

| ID | Decision | Reason / status |
| --- | --- | --- |
| D001 | Strict approval gate after each phase | Explicit user requirement; Phase 0 only authorized now |
| D002 | .NET 10 LTS MVC modular monolith | Installed supported baseline; domain/application/infrastructure/web boundaries |
| D003 | MySQL and EF Core | Required stack; stable provider version must be verified before Phase 2 setup |
| D004 | Razor, Tailwind v4, vanilla JS | Required stack; no SPA or prebuilt admin framework |
| D005 | Route cultures en/ar and revision-scoped translation tables | Real bilingual content, RTL and localized SEO |
| D006 | Complete required Arabic/English translations before publication | Explicit indicators and blocking validation avoid silent fallback |
| D007 | Immutable revisions and separate published pointers | Draft isolation and reliable restoration; foundational schema in Phase 5 |
| D008 | Shared semantic theme tokens | Public/admin consistency with constrained runtime customization |
| D009 | Native MySQL first for development | Running service; Docker unavailable; schema access still unverified |
| D010 | No fabricated company facts | Only approved content can become real public claims |
| D011 | Editor drafts; SuperAdmin publishes | Proposed least-privilege role policy; may be revised |
| D012 | Documentation only in Phase 0 | No application scaffolding until its approved phase; Git setup planned in Phase 2 |

User revision: perform local configuration and Git setup now, before Phase 1. Store credentials only in ignored local configuration. Target private GitHub repository: nloiq/NLOIQ; remote is not created yet.


User revision: use the existing https://github.com/nloiq/ai.git repository as origin instead of creating nloiq/NLOIQ. Keep main as the tracked branch and preserve existing repository visibility.

