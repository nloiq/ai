# Phase 6 review: authentication and admin shell

Phase 5 was explicitly approved on 2026-10-23. Phase 6 is ready for review; Phase 7 has not started.

## Open the working preview

- English: http://127.0.0.1:5080/en/account/login
- Arabic: http://127.0.0.1:5080/ar/account/login
- Open local `artifacts/PHASE_6_LOGIN.txt` for the generated review administrator credentials. This file is ignored by Git; it is never a deployed configuration source.
- Sign in, inspect the database inventory, switch language/theme, resize to mobile, and sign out.
- The current inventory remains 22 draft content items, 0 published items and 3 media assets. There is one explicitly created local review account. The Phase 5 inventory JSON is the historical checkpoint with zero users.

## Role permissions

| Workspace | SuperAdmin | Editor | Viewer |
| --- | --- | --- | --- |
| Overview (`/{culture}/admin`) | Yes | Yes | Yes |
| Content (`/admin/editor`) | Yes | Yes | No |
| Administration (`/admin/settings`) | Yes | No | No |
| Account count | Yes | No | No |

All routes enforce policies server-side. Restricted requests reach a localized HTTP 403 page. Content/settings workspaces are placeholders for approved later CMS phases; no editing or account management is implied.

## Security behavior

Identity hashes passwords. Bootstrap requires 14+ characters and upper/lower/digit/symbol, with a maximum of 256 matching the login limit. Unique email is required. There is no public registration endpoint.

Login and logout require anti-forgery tokens; logout accepts POST only. Local-only return URLs prevent external redirect injection. Invalid credentials, inactive users and lockout use a generic localized message. Five failed passwords lock an account for 15 minutes; the endpoint also limits 20 requests per source IP per minute. This limiter is process-local; distributed rate limits/proxy configuration belong to deployment.

Application cookies are HttpOnly, SameSite Strict and session-only. Authentication tickets expire after 30 minutes with sliding renewal. Cookies require HTTPS outside Development; localhost HTTP is intentionally supported for this review. Every authenticated request validates the Identity security stamp and current active/locked state and refreshes role claims. Admin/account responses are no-store. Existing CSP and frame protections remain.

No new database migration is required. Existing Identity tables and role definitions are used. Content fixture initialization still creates no user accounts and preserves existing accounts.

## Create your own administrator

From the repository root, use an interactive PowerShell terminal:

```powershell
dotnet run --project src/NLOIQ.Web --no-build -- --create-admin
```

Enter an email and password when prompted. Password input is hidden in an interactive console. This command requires direct server/database access, creates a SuperAdmin explicitly, and refuses to reset or modify an existing email. It does not run during web startup. Production must provide its database connection through environment/hosting secrets; the local review account must not be copied into production.

## Verification

```powershell
dotnet restore NLOIQ.sln --locked-mode
dotnet build NLOIQ.sln --no-restore
dotnet run --project tests/NLOIQ.AuthChecks --no-build
dotnet run --project tests/NLOIQ.DatabaseChecks --no-build
npm.cmd run test:browser
node tests/prototype-smoke.mjs
dotnet run --project tests/NLOIQ.Checks --no-build
npm.cmd run test:interactions
```

Authentication checks require the local preview running and the dedicated development database. They create six random temporary accounts, test real HTTP/Identity behavior with headless Edge, then remove those accounts in a finally block. Tests cover all three policies, English/Arabic login, responsive RTL dashboard at four widths, persistent dark theme, CSRF, POST-only logout, session cookies, cache headers, external redirect rejection, unknown/inactive users, persisted lockout, rate limits and disabling an already signed-in account. The authentication test intentionally exhausts the login limiter; restart the local preview before manual review or wait one minute before rerunning it.

Screenshots: docs/previews/phase6/admin-en-desktop.png and admin-ar-mobile-dark.png. Passwords and authenticated browser state remain in ignored local artifacts only.

No production readiness claim: deployed HTTPS/proxy settings, Data Protection key persistence, account recovery/MFA and full release QA still need their planned review.
