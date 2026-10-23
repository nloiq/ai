# Local setup

The supplied database connection is stored in the root appsettings.Local.json, excluded from Git. Do not commit it or copy its password into documentation.

The tracked appsettings.example.json documents the configuration shape with a placeholder password. Database target: 127.0.0.1:3306, WebSiteNLOIQDb, user nloiq.

No application exists yet. During Phase 2, move the local configuration into the Web project's content root and explicitly load appsettings.Local.json only for Development. ASP.NET Core does not automatically load a file with that name. Use environment variables or secret storage for production.

The supplied connection has not yet been authenticated or used to create a schema. The successful Phase 0 TCP check only confirmed a listening endpoint.

GitHub synchronization requires the intended repository URL. No unrelated existing repository should be selected automatically. Database credentials must remain excluded from every commit and push.
