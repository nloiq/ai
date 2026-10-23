# NLO IQ project progress

Updated: 2026-10-23 (Asia/Baghdad).
Current phase: **6 ? Authentication and Admin Shell**.
Status: **Delivered for review; awaiting explicit Phase 6 approval**.
Approved phases: **0, 1, 2, 3, 4 and 5**.
Selected design: revised Concept A, engineering folio.
Last approved checkpoint: 5c69a82 (Phase 5 database architecture).

## Completed

- Previous public prototype, SQL content architecture, immutable revisions and bilingual draft fixtures retained.
- ASP.NET Core Identity sign-in and POST-only sign-out with anti-forgery tokens.
- SuperAdmin, Editor and Viewer policies enforced on server routes and reflected in navigation.
- Password hashing, 14-character password minimum, five-failure/15-minute account lockout, 20 login requests/IP/minute.
- HttpOnly/Strict session cookies; 30-minute sliding ticket; Secure required outside Development.
- Security stamps/roles and active/locked state checked on each authenticated request.
- English/Arabic admin login, responsive sidebar/dashboard, RTL and shared system/light/dark themes.
- Dashboard reports real database counts. CMS editing remains Phase 7.
- Explicit administrator CLI; no registration endpoint, hardcoded password or automatic startup user seed.
- One random local review administrator created; credentials only in ignored artifacts/PHASE_6_LOGIN.txt.
- Real Identity/MySQL/browser checks passed for policies, lockout, CSRF, safe redirects, disabled-session revocation, rate limits and logout.
- Existing 16 database scenarios, five public browser tests and validation/HTTP regressions passed.

## Review

English login: http://127.0.0.1:5080/en/account/login
Arabic login: http://127.0.0.1:5080/ar/account/login
Credentials: artifacts/PHASE_6_LOGIN.txt (local only; never commit).
Review instructions and permission matrix: PHASE_6_ADMIN.md.
Restart: dotnet run --project src/NLOIQ.Web --no-build.

## Limitations and remaining phases

- CMS/public revision wiring and editing start only after Phase 6 approval, in Phase 7.
- Editor/settings destinations currently show an honest upcoming-phase page, protected by their final role policies.
- No account management UI, password reset email or MFA setup yet; administrator creation is CLI only.
- Fixtures remain draft/demo; real logo, approved project copy and contact details are outstanding.
- Production HTTPS/domain/proxy, shared persistent Data Protection keys and deployment checks remain Phase 12.
- Full publishing/scheduler/revisions, media/SEO/appearance, complete localization and final QA remain their planned phases.
- Live external video playback still requires user review.

7 Core CMS; 8 Media/SEO/appearance; 9 Complete localization; 10 Publishing/preview/revisions; 11 QA; 12 Deployment/handover. Each requires explicit approval. **Do not start Phase 7 yet.**

## Commands

APPROVE PHASE advances one phase. REVISE modifies the current phase. SHOW PREVIEW reports actual preview. EXPLAIN makes no changes. STATUS reports progress. ROLLBACK requires confirming discarded changes and database implications before restoring a checkpoint.
