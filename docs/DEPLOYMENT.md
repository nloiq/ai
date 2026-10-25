# Production deployment

Prepared 2026-10-25. Reference target: one Windows IIS instance, ASP.NET Core 10, MySQL 8 and a real HTTPS binding. No host or domain has been selected and no public deployment has been performed. Examples use placeholders. Do not copy local review credentials, database or demonstration pages to production by default.

## Build a release

On the build machine, from the repository root:

```powershell
dotnet tool restore
npm.cmd ci
npm.cmd run css:build
dotnet restore NLOIQ.sln --locked-mode
dotnet build NLOIQ.sln --no-restore -c Release
dotnet publish src/NLOIQ.Web --no-build -c Release -o artifacts/phase12-publish
dotnet ef migrations script 0 --project src/NLOIQ.Infrastructure --startup-project src/NLOIQ.Web --no-build --configuration Release --output artifacts/phase12-schema.sql
```

Archive the commit ID, publish directory, SQL and checksums together. This framework-dependent artifact requires the ASP.NET Core 10 runtime on the host; Node/npm and the SDK are build tools. The build does not run npm automatically: the Tailwind command is required. `appsettings.Local.json`, database dumps and `App_Data/media` are excluded from publishing. Uploads must be restored/provisioned separately. Do not run stateful review/test scripts against production.

## Configuration contract

Supply configuration using the host's restricted service environment or secret store. `__` represents a nested configuration separator. No real credentials belong in committed JSON, deployment scripts, command-line arguments or shared logs.

| Setting | Value / purpose |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production`. Launch profiles are not used by the published DLL. |
| `ConnectionStrings__DefaultConnection` | Required: `server=DB_HOST;port=3306;database=DB_NAME;user=APP_USER;password=SECRET;CharSet=utf8mb4;SslMode=VerifyFull;`. Configure a trusted CA on the host for remote MySQL. Confirm connector TLS settings with the DBA. |
| `AllowedHosts` | Exact public host names separated by semicolons, without schemes, paths or ports. The shipped Production file restricts this to loopback until configured. |
| `Logging__LogLevel__Default` | `Information` normally. Keep SQL command logging at Warning; do not enable sensitive-data logging. |
| `ASPNETCORE_URLS` | Standalone Kestrel only, e.g. `https://127.0.0.1:5443`; IIS integration manages its own binding. |
| `Kestrel__Certificates__Default__Path` / `Password` | Standalone HTTPS only: separately provision a real certificate, restricted private key and secret. Not required for IIS TLS termination. |

`PublicBaseUrl` is a **published CMS Site settings field**, not an environment variable. Set the canonical HTTPS origin there and publish it. Preview video configuration is Development-only content configuration and does not publish CMS projects; enter approved YouTube IDs in project records.

The app reads `appsettings.Local.json` only in Development. Neither startup nor the publication worker applies migrations or seeds content. There is no public registration, automatic default administrator, password-reset email service, readiness endpoint or user-management UI to assume exists.

## Database and first administrator

1. DBA creates a dedicated UTF-8 (`utf8mb4`) schema and accounts. Use a separate migration identity for schema changes; the running app normally needs SELECT/INSERT/UPDATE/DELETE, not DDL or access to other databases. Do not reuse the development account/password. Verify MySQL server version, connector compatibility, table-name case handling and network/TLS on the target.
2. For an empty database, review `phase12-schema.sql` before applying it with the MySQL client using its interactive password prompt or restricted credentials file. This script is **not idempotent**. It is only for a blank schema. It includes EF migration history but no demonstration content or administrator.
3. For an upgrade, read the destination's `__EFMigrationsHistory` first, then generate `dotnet ef migrations script FROM_MIGRATION TO_MIGRATION --project src/NLOIQ.Infrastructure --startup-project src/NLOIQ.Web --no-build --configuration Release --output artifacts/upgrade.sql`. Use exact committed migration IDs, review and test on a restored staging copy. Back up first, stop all writers/workers, apply once, verify history, then start the matching release. MySQL DDL may commit implicitly; do not assume a failed script fully rolled back. Do not blindly rerun it.
4. In the published directory, with Production and the destination connection string already supplied securely, run `dotnet NLOIQ.Web.dll --create-admin` in an interactive console as the deployment operator. Enter a real administrator email and unique 14–256 character password with upper/lower/digit/symbol. Input is hidden in an interactive console. Do not pipe credentials or retain them in transcript files.
5. The bootstrap creates missing SuperAdmin/Editor/Viewer roles through Identity, then creates the new SuperAdmin. An existing email is rejected without changing its password/roles. A failure assigning the role removes the new user. Recheck the outcome after interruption before retrying. Roles may remain after an invalid password; no demo content is inserted. Keep the password in the owner's password manager.

Do **not** run `--initialize-database` in production: it is explicitly Development-only and seeds demonstration records. For existing-account recovery, use a reviewed Identity-based operator procedure; there is no reset UI and raw SQL password hashes are inappropriate.

## IIS reference procedure

1. Install IIS and the ASP.NET Core 10 Hosting Bundle according to Microsoft instructions. Configure a dedicated application pool with No Managed Code and a dedicated identity. Give it read/execute on the release and modify access only to private runtime storage. Use the published `web.config`; do not hand-edit generated module settings without host-specific validation.
2. Create an HTTPS site binding with the approved domain and valid certificate. Redirect HTTP to HTTPS at IIS and configure HSTS at the edge once the HTTPS domain is confirmed. The app currently does not provide its own HTTPS redirect/HSTS middleware. Do not expose an unencrypted public binding.
3. Place the published files in a versioned directory. Bind its `App_Data/media` directory to persistent private storage via a properly ACL-protected junction or restore/copy the media directory into each release while stopped. This path is fixed relative to ContentRoot. It must never be exposed as an IIS virtual directory or served outside application authorization. Keep configuration, dumps and Data Protection keys outside wwwroot.
4. Persist the IIS identity's Data Protection key storage and profile across restarts/releases. Provision it using the IIS guidance below; verify that protected cookies remain usable after recycle. Do not switch identities or delete the key ring during deployment. Back up protected keys and the associated Windows protection identity/material. This app uses the platform's default key persistence; it does not implement a custom shared ring. Multiple hosts require additional reviewed key/storage and limiter configuration.
5. Supply the environment settings above using restricted host configuration. For IIS, enable reliable process startup/AlwaysRunning and appropriate preload/idle settings: the scheduled publisher runs **inside this process**. Configure recycling, crash recovery and monitoring. Shutdown stops publishing; overdue eligible jobs are processed when running again. Do not promise exact-time execution during downtime.
6. Start the site privately, finish CMS configuration/content, and run the checks below before any public DNS/binding cutover. A new Production home correctly returns 404 until a non-demo page with key `home` and its dependencies are published.

IIS integration is the reference proxy arrangement. Arbitrary Nginx/load-balancer deployments need explicit trusted forwarded-header configuration and validation of scheme/client IP. This application has no custom trusted-proxy configuration. Do not enable blanket forwarded-header trust: login throttling depends on client IP. A Linux host also needs actual native SkiaSharp upload/resize tests, persistent storage/key provisioning and a supervised process; it has not been validated here.

## Target-host acceptance and cutover

On private staging with the same runtime and deployment identity: verify real TLS chain/redirects, AllowedHosts, restart recovery, database connectivity, application logs and writable private storage. Sign in over HTTPS; verify Secure/HttpOnly/Strict cookies and CSRF rejection. Upload/resize an approved image, view published responsive variants, and confirm private draft images remain denied anonymously. Check bilingual home/page/project routes, independent Arabic slugs, RTL/mobile/themes, lightbox/video, metadata/canonical/hreflang and missing-page status codes. Publish and restore a real test revision, schedule/cancel a test job, confirm worker success, and remove/cancel test jobs before cutover. Verify a cookie survives a recycle and that restart does not lose media. Test a complete backup restore into an isolated instance.

Use actual approved logo, contact details, services and projects. Review every published root; unpublish retained development workflow examples and never present demo facts as company work. Publish dependencies before their parent composition. Confirm both home languages return 200, private paths remain protected and SEO uses the public HTTPS origin.

Cutover requires explicit owner authorization and an agreed rollback point. Stop writers, take a coordinated backup, apply reviewed migrations, switch the release, restore/mount storage and restart. Perform the acceptance checks; monitor errors, disk and schedules. If only application code changed compatibly, switch back to the previous release. If schema/data changed, keep the site stopped and use the tested coordinated restore plan; do not assume an old binary can read a newer schema.

## Sources

- [ASP.NET Core hosting](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/?view=aspnetcore-10.0)
- [IIS hosting and Data Protection provisioning](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-10.0)
- [EF migration deployment](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying)
- [Data Protection configuration](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-10.0)
