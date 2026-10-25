# Maintenance, backup and recovery

Owner/operator must agree backup frequency, retention, restore time/data-loss targets and an alert recipient before launch. These are operating instructions, not a provisioned backup service. Store encrypted backups off-host with restricted access and test restoration regularly.

## Backup set

Capture the MySQL database (including Identity, content/revisions, audit, media metadata and schedules), the complete private `App_Data/media` tree, persistent Data Protection keys with their protection material, host configuration/secrets/certificates through the approved secret backup process, and the matching release artifact/commit/migration history. A Git checkout or `dotnet publish` alone cannot recreate runtime data.

For a consistent set, put the site into maintenance and **stop all application instances**, including publication workers and administrative writers. Record UTC time, commit, runtime version, migration IDs, pending schedules and file inventory. Take the database and media backups during the same stopped interval, then resume only after completion is verified. A database-only transactional dump does not coordinate concurrent filesystem uploads/deletes. Do not disable foreign-key checks as a substitute for consistency.

Example MySQL command, adapted by the DBA for the chosen schema and backup location:

```text
mysqldump --host=DB_HOST --port=3306 --user=BACKUP_USER --password --single-transaction --routines --events --triggers --no-tablespaces --set-gtid-purged=OFF --default-character-set=utf8mb4 --result-file=ABSOLUTE_BACKUP_PATH.sql DB_NAME
```

Use the interactive password prompt or a restricted client configuration, never a password argument. The result-file option avoids shell encoding/redirection damage. This assumes compatible MySQL tools and InnoDB, no concurrent DDL and adequate backup privileges. Encrypt the SQL after capture; it contains account hashes and private content. Copy the full media tree with a host-native backup tool preserving filenames/permissions. Record checksums and verify file counts, readback and successful command exit codes; copying a locked/failed dump is not success.

## Restore rehearsal and disaster recovery

1. Provision an isolated schema and private storage, with the same application release/runtime and compatible MySQL case/collation settings. Do not point a staging instance at production. Keep the web app stopped so the worker cannot execute restored pending schedules.
2. Restore the dump with the MySQL client's `source ABSOLUTE_PATH.sql` command into the intended empty schema, using an interactive/restricted login. Confirm the exact destination before any destructive import. Restore media and key/config backups with correct service-identity ACLs. Windows-protected keys may require the original protection identity/material; a copied XML file alone may not suffice.
3. Compare migration history, row/file inventory and referential checks. An older backup should run its matching code first; apply later migrations only as a separately reviewed upgrade. Review all pending jobs before starting, especially during recovery from an old backup. Do not reconnect a staging restore to production services.
4. Start privately over HTTPS. Verify login, both language pages, images/variants, private previews, draft/public isolation and save/publish. Confirm the actual file bytes are restored, not just metadata. Check audit and worker logs. Record observed restore time and data loss against the agreed targets.
5. Production restoration requires owner authorization, a stopped site, confirmed target and preservation of the failed state for investigation where practical. Restore database and files as a matched set; reconcile pending jobs before enabling traffic. Revalidate TLS/host/origin and monitor after restart.

## Routine operation

Monitor process health/restarts, HTTP 5xx and login failures, certificate expiration, database availability/backup age, storage capacity and failed/overdue schedules. There is no custom readiness/metrics endpoint: use an external HTTPS check against a known published page plus a separate database/storage/worker check. Do not treat a login-page 200 as proof publishing works. Log to the host's protected logging destination with retention; enable IIS stdout logging only briefly for startup diagnosis because it can grow without rotation.

Review dependencies/runtime updates, lockfiles and security audits before a maintenance release. Rebuild/minify CSS, run relevant tests against the dedicated development/staging database and repeat target-host acceptance for meaningful runtime/media/auth changes. Keep the previous release and coordinated backup available. Never run the seeded demo initializer or CMS review automation on production.

Maintain a single instance until shared keys/media, cross-instance rate limiting and worker behavior are reviewed on the selected infrastructure. Scheduling uses database state but is hosted in-process; always-on startup and recovery matter. Take a backup before bulk content retirement. Do not delete media folders based only on current pages: immutable history may still reference them.

| Symptom | First checks |
|---|---|
| Connection refused / IIS startup error | Process/service/app-pool state, installed runtime, protected startup logs, environment connection string and host binding. |
| Production home 404 | Non-demo `home` page published for both languages; published referenced dependencies. Development prototype is intentionally absent. |
| Media missing after release | ContentRoot's `App_Data/media` mount/copy, file ACLs and matching database backup. |
| Login/CSRF fails after restart | HTTPS cookie transport, stable Data Protection identity/key storage, correct host and system clock. |
| 429 or account locked | Rate-limit window/15-minute lockout; check real client IP behind the approved proxy. |
| Schedule overdue/failed | Running worker, clock, pending/failed state, scheduling account eligibility, draft invalidation and route/dependency errors. |

Architecture: `NLOIQ.Web` handles MVC/auth/localization and hosted scheduling; `NLOIQ.Application` defines contracts/validation; `NLOIQ.Domain` owns models; `NLOIQ.Infrastructure` owns EF/MySQL, revision workflows, public projections and image storage. See DATABASE_ARCHITECTURE.md, PHASE_10_PUBLISHING.md and PHASE_11_QUALITY_REVIEW.md for schema/workflow/test evidence. No background email service, automated disaster-recovery system or production monitoring integration is implemented.
