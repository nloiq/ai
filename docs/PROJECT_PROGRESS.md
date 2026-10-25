# NLO IQ project progress

Updated: 2026-10-25 (Asia/Baghdad).
Current phase: **12 — Deployment and handover**.
Status: **Preparation complete; awaiting final Phase 12 approval**.
Approved phases: **0 through 11**.
Last approved checkpoint: `4b5a5f7` (Phase 11 quality).
Selected design: Concept A, engineering folio.

Phase 11 was explicitly approved by the user on 2026-10-25. Its quality evidence and limitations remain in PHASE_11_QUALITY_REVIEW.md.

## Phase 12 delivered

- Production configuration and environment-variable documentation.
- Release/Tailwind build, reviewed-script migration procedure and interactive secure administrator bootstrap.
- Fresh production role initialization without Development demo seeding; rollback-only verification passes.
- IIS reference hosting and target-host acceptance/cutover instructions.
- Coordinated database/media/key backup, restore rehearsal and maintenance guidance.
- CMS user guide and final application summary.
- Release build (zero warnings/errors), publish exclusions and actual artifact's isolated Production HTTPS smoke pass. Blank-schema SQL generated but not applied.

Guide: PHASE_12_HANDOVER.md. Evidence: PHASE_12_VALIDATION.json.
Local preview: http://127.0.0.1:5080/en/ and /ar/.

## Final gate

**Stop for final Phase 12 approval. No public deployment or live infrastructure changes performed.** Hosting/domain/TLS, production secrets/storage, backup restore rehearsal, operations/monitoring and approved company copy/logo/contact/projects remain owner/host inputs and target-host acceptance work. A non-demo bilingual homepage must be published before launch. The local Development design preview is not a finished production content set.

Final handover approval does not select or authorize a production target. Public deployment requires explicit target authorization. REVISE requests changes; SHOW PREVIEW reports the app; STATUS reports progress.
