# Phase 12 — Deployment and handover

Phase 11 approved by the user on **2026-10-25 (Asia/Baghdad)** at `4b5a5f7`. Phase 12 prepares the release and documentation; it does not authorize public deployment or live infrastructure changes.

## Deliverables

- [Deployment/configuration guide](DEPLOYMENT.md): release/Tailwind build, environment variables, reviewed migration scripts, secure interactive admin bootstrap and IIS reference procedure.
- [CMS user guide](CMS_USER_GUIDE.md): permissions, bilingual composition, media/video, SEO, draft/public history, restoration and scheduling.
- [Maintenance and backup guide](OPERATIONS.md): coordinated database/media/key backups, isolated restore rehearsal, upgrades, monitoring and troubleshooting.
- Production JSON contains no credentials and defaults to loopback host names until an operator supplies the real AllowedHosts.
- Fresh-production administrator setup creates missing Identity roles without running the Development demo initializer. Existing-email refusal and role-assignment failure cleanup remain in place.

The delivered application is an ASP.NET Core MVC/MySQL bilingual engineering website/CMS covering low-current engineering, software solutions, SCADA integration and system automation. It includes revision-based content composition, private media, role-protected editing, localized SEO/branding, previews/history and scheduled publication. Phase 11 records local quality verification and its limits.

## Final release gate

The following require owner/host input and target-environment verification before launch: hosting account/OS and operator, domain/DNS/TLS, approved production database/secrets and connectivity, stable private media/key storage, backup/restore targets and rehearsal, always-on worker/monitoring, approved company copy/logo/contact/project assets and a published non-demo bilingual homepage. The existing local design preview and development workflow examples are not a final production content set.

No real host, DNS, certificate, production database or external backup/monitoring service has been provisioned. Windows IIS is the documented reference; Linux/native image processing and arbitrary proxy setups need their own verification. Local automated accessibility checks are not a full accessibility certification.

**Phase 12 handover approved by the user on 2026-10-25 (Asia/Baghdad), checkpoint `72f80c2`. Public deployment requires explicit authorization identifying the target.**

## Preparation verification

Release build: all eight projects, zero warnings/errors. Tailwind minified CSS and framework-dependent publish complete. Blank-schema SQL generated for operator review without applying it. The published artifact excludes local credentials and runtime uploads and includes IIS web.config. A rollback-only bootstrap check verifies missing-role creation, password verification, SuperAdmin assignment and refusal to replace an existing account, preserving original account/role inventory. The actual Phase 12 artifact passes the isolated loopback Production HTTPS smoke (secure antiforgery cookies, published page, private preview denial, localized 404, hidden error details and disabled prototype). See PHASE_12_VALIDATION.json.

Repeat the focused bootstrap check on the dedicated development database using `dotnet run --project tests/NLOIQ.AuthChecks --no-build -c Release -- --bootstrap-check` with redirected stdin. For the published artifact, set `NLO_RELEASE_DIRECTORY=artifacts/phase12-publish` in the process environment and run `node tests/release-smoke.cjs`; clear the variable afterward. These local checks do not verify IIS installation, a real certificate or disaster recovery on the future host.
