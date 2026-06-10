# 12-Factor App Compliance Report

Date: 2026-06-10

Scope: `QuizzArena.Backend` (.NET 10 modular monolith — `Host` + `Quizzing`, `Users`, `DocumentProcessing` modules + `Shared`).

## Summary

QuizzArena is a single-deployable modular monolith with solid dependency management and a clean hexagonal structure. It scores well on Codebase, Dependencies, and Backing Services, but has meaningful gaps in Config (committed secrets, hard-coded endpoints), stateless Processes (in-memory saga state), Build/Release/Run (no app Dockerfile), Logs (ad-hoc `Console.WriteLine`), and Disposability (no graceful shutdown).

### Scorecard

| # | Factor | Status | Notes |
|---|--------|--------|-------|
| 1 | Codebase | ✅ Compliant | Single repo, single deployable |
| 2 | Dependencies | ✅ Compliant | Pinned NuGet refs, centralized props, local tool manifest |
| 3 | Config | ❌ Non-compliant | Secrets committed; RabbitMQ/Whisper endpoints hard-coded |
| 4 | Backing services | 🟡 Partial | Treated as attached resources, but several not config-bound |
| 5 | Build, release, run | ✅ Compliant | CI publishes artifact + Docker image to GHCR |
| 6 | Processes | ❌ Non-compliant | In-memory saga repository holds state |
| 7 | Port binding | ✅ Compliant | Kestrel in container binds 8080 via `ASPNETCORE_HTTP_PORTS` |
| 8 | Concurrency | 🟡 Partial | MassTransit consumers in-process, not independently scalable |
| 9 | Disposability | ❌ Non-compliant | No graceful shutdown; `async void` initializer |
| 10 | Dev/prod parity | 🟡 Partial | Dev and prod settings near-identical; migrations dev-only |
| 11 | Logs | ❌ Non-compliant | `Console.WriteLine`, no structured logging |
| 12 | Admin processes | 🟡 Partial | Migrations/seeding run inline at web startup |

Legend: ✅ Compliant · 🟡 Partial · ❌ Non-compliant

---

## I. Codebase — ✅ Compliant

One codebase tracked in Git, deployed as a single unit.

- `QuizzArena.Backend/QuizzArena.Backend.slnx` defines one solution: a `Host` web project plus three module projects (`Quizzing`, `Users`, `DocumentProcessing`) and `Shared`.
- This is a modular monolith — one deployable artifact, not many apps — which aligns with the codebase factor (one codebase per app).
- CI (`.github/workflows/build.yml`) builds the single solution.

No action required.

## II. Dependencies — ✅ Compliant

Dependencies are explicitly declared and isolated.

- Each project uses SDK-style `.csproj` with pinned `PackageReference` versions, e.g. `Host/Host.csproj` (EF Core 10.0.8, JwtBearer 10.0.8, Swashbuckle) and `Modules/QuizzArena.DocumentProcessing/QuizzArena.DocumentProcessing.csproj` (MassTransit 8.3.7, Azure.Storage.Blobs 12.28.0, Npgsql 10.0.1, Pgvector 0.3.0).
- `Directory.Build.props` centralizes language version, nullable, analyzers, and deterministic builds.
- `.config/dotnet-tools.json` declares the local `husky` tool in an isolated manifest, restored automatically via `Directory.Build.targets`.
- Isolation is achieved through `dotnet restore` / NuGet; no reliance on system-wide packages.

No action required.

## III. Config — ❌ Non-compliant

Config should live in the environment, not in code. This is the project's weakest area.

- `Host/appsettings.json` hard-codes credentials in the connection string: `Host=localhost;...;Username=quiz_user;Password=quiz123`, plus `AzureBlobStorage` and `WhisperSettings:BaseUrl`. The same secrets are duplicated in `Host/appsettings.Development.json`.
- `Modules/QuizzArena.DocumentProcessing/DependencyInjection.cs` hard-codes the RabbitMQ host and credentials: `cfg.Host("localhost", "/", h => { h.Username("guest"); h.Password("guest"); });` — not read from configuration at all.
- `.env.example` only covers Postgres variables (`POSTGRES_USER/PASSWORD/DB`) and those are consumed by `docker-compose.yml`, not by the application. There is no `Environment.GetEnvironmentVariable` usage and no environment-variable binding for the app's own config.
- Config is partially read through `IConfiguration` (`GetConnectionString`, `configuration["WhisperSettings:BaseUrl"]`, `configuration["Keycloak:Authority"]` in `Host/Security/AuthenticationMiddleware.cs`), which is the right mechanism — but the values are committed rather than externalized.

Recommendations:
- Move all secrets and environment-specific values out of `appsettings*.json` into environment variables (ASP.NET Core binds `ConnectionStrings__DefaultConnection`, `Keycloak__Authority`, etc. automatically) or a secret store (User Secrets in dev, a vault in prod).
- Read RabbitMQ host/credentials from configuration instead of hard-coding `guest/guest`.
- Keep `appsettings.json` limited to non-sensitive defaults; never commit real credentials.

## IV. Backing Services — 🟡 Partial

Backing services should be attached resources, swappable via config.

- `docker-compose.yml` declares PostgreSQL (`pgvector/pgvector:pg17`), RabbitMQ, Ollama, Azurite (Azure Blob emulator), and Whisper ASR (high/low profiles).
- The app treats them as attached resources: EF Core/Npgsql for three DbContexts, `BlobRepository` (Azure.Storage.Blobs) bound via the `AzureBlobStorage` connection string, MassTransit for messaging, and `WhisperTranscription` via `HttpClient`.
- However, several endpoints are not fully config-bound: RabbitMQ is hard-coded to `localhost`/`guest`, and the Whisper base URL falls back to a hard-coded `http://localhost:9000/`. This makes swapping a backing service (e.g. pointing at a managed broker) require code changes rather than config changes.

Recommendations:
- Bind RabbitMQ and Whisper endpoints to configuration so each resource can be swapped purely through environment changes.

## V. Build, Release, Run — ✅ Compliant

These three stages are now strictly separated.

- **Build:** CI restores, compiles, verifies formatting (`dotnet format --verify-no-changes`), and runs tests. A `dotnet publish` step produces a Release artifact uploaded to GitHub Actions.
- **Release:** The `docker` job builds an immutable image from the repo and pushes it to GitHub Container Registry (`ghcr.io`) tagged with the branch name, short SHA, and `latest` on the default branch. The `docker` job only runs on `push` (not PRs), so pull requests validate without publishing.
- **Run:** The image is self-contained (`aspnet:10.0` runtime, non-root user, port 8080). The same image is promoted across environments by changing environment variables — no rebuilds.
- `scripts/init.sql` runs as a Postgres init script for the pgvector extension.

Previously: CI only built and tested; there was no Dockerfile for the app, no publish step, and no Docker image stage.

## VI. Processes — ❌ Non-compliant

Processes should be stateless and share-nothing.

- `Modules/QuizzArena.DocumentProcessing/DependencyInjection.cs` registers the saga with an in-memory repository: `x.AddSagaStateMachine<IngestionSaga, IngestionSagaState>().InMemoryRepository();`. Saga state lives in process memory, is lost on restart, and cannot be shared across instances — this breaks statelessness and prevents horizontal scaling.
- On the positive side, JWT authentication is stateless and there is no HTTP session state.

Recommendations:
- Persist saga state in a durable, shared store (e.g. EF Core/Postgres or Redis saga repository) so any instance can resume in-flight work.

## VII. Port Binding — 🟡 Partial

## VII. Port Binding — ✅ Compliant

The app is self-contained and exports its service via port binding.

- The Docker container sets `ASPNETCORE_HTTP_PORTS=8080` (in `docker-compose.yml`), so Kestrel binds port 8080 through an environment variable rather than a dev-only file.
- `Host/Properties/launchSettings.json` still defines ports `5245`/`64387` for local development (`dotnet run`), which is appropriate — `launchSettings.json` is dev tooling only and is not shipped in the image.

Previously: the app relied on `launchSettings.json` for port config with no production override.

## VIII. Concurrency — 🟡 Partial

Scale out via the process model.

- MassTransit runs `TranscriptionRequestConsumer` and `IngestionSaga` as hosted background consumers in-process inside `Host` (`cfg.ConfigureEndpoints(context)`). Work can be processed concurrently, but message processing is coupled to the web process rather than running as independently scalable worker processes.
- Combined with the in-memory saga (factor VI), the app cannot currently be scaled to multiple instances safely.

Recommendations:
- Once saga state is durable, consider separating message-consumer workers so web and worker concurrency can scale independently.

## IX. Disposability — ❌ Non-compliant

Maximize robustness with fast startup and graceful shutdown.

- No graceful shutdown handling: no `IHostApplicationLifetime` hooks, and `TranscriptionRequestConsumer.Consume` does not honor a cancellation token, so in-flight work isn't drained on shutdown.
- `UserModuleInitializer.Initialize` is `async void`, running `context.Database.Migrate()` and seeding as fire-and-forget at startup. It cannot be awaited and risks startup races and unobserved exceptions.
- The Whisper `HttpClient` timeout is set to 60 minutes, which combined with in-memory saga state means a restart can lose long-running work.

Recommendations:
- Make initializers `async Task` and await them during startup.
- Honor cancellation tokens in consumers and add graceful shutdown; pair with a durable saga store so in-flight work survives restarts.

## X. Dev/Prod Parity — 🟡 Partial

Keep development, staging, and production as similar as possible.

- `appsettings.json` and `appsettings.Development.json` are nearly identical (same localhost DB and credentials); production values are not externalized.
- Migrations only auto-apply in Development: `if (app.Environment.IsDevelopment()) { app.ApplyMigrations(); }` in `Host/Program.cs`, so the production startup path differs from development.
- docker-compose provides dev backing services, but the app is not dockerized and Azurite (dev emulator) vs. real Azure Blob is not parameterized.

Recommendations:
- Externalize per-environment config so the only differences are values, not code paths.
- Adopt a consistent migration strategy across environments (see factor XII).

## XI. Logs — ❌ Non-compliant

Treat logs as event streams written to stdout; don't manage log files in-app.

- Only the default `Logging.LogLevel` block exists in appsettings; no structured logging (e.g. Serilog) is configured.
- Code writes directly with `Console.WriteLine(...)` in `IngestionSaga.cs` and `UserValidationMiddleware.cs` instead of `ILogger`. While stdout is the right destination, the output is unstructured and ad-hoc.

Recommendations:
- Replace `Console.WriteLine` with `ILogger<T>`.
- Add structured logging (e.g. Serilog with a console JSON sink) so logs are machine-parseable in the event stream.

## XII. Admin Processes — 🟡 Partial

Run admin/management tasks as one-off processes.

- `Host/Extensions/MigrationExtension.cs` iterates `IModuleInitializer` implementations; each module's `*ModuleInitializer` runs `context.Database.Migrate()`, and `UserModuleInitializer` also runs `SeedRunner.SeedAsync(context)`.
- These are not separate one-off processes — they run inline during web app startup, and only in Development. The `scripts/` folder contains only `init.sql`.

Recommendations:
- Run migrations and seeding as dedicated one-off commands/jobs (e.g. a CLI entry point or a pre-deploy job) decoupled from web startup, applied consistently across environments.

---

## Priority Recommendations

Resolved since the initial assessment:
- ✅ Added a Dockerfile (multi-stage, non-root) and wired the app into `docker-compose.yml` with all backing services config-driven. (Factors V, VII)
- ✅ CI now publishes a `dotnet publish` artifact and builds/pushes a Docker image to GHCR on every push to `main`/`develop`. (Factor V)
- ✅ Port binding via `ASPNETCORE_HTTP_PORTS=8080` set in the container environment. (Factor VII)
- ✅ RabbitMQ host/credentials read from `IConfiguration` (`RabbitMq:Host`, `RabbitMq:Username`, `RabbitMq:Password`). (Factors III, IV)

Remaining:
1. Remove committed secrets from `appsettings*.json`; bind all config (DB, Whisper, Keycloak) from environment variables. (Factor III)
2. Replace the in-memory saga repository with a durable store (EF Core/Postgres or Redis). (Factors VI, VIII)
3. Add structured logging (Serilog) and replace `Console.WriteLine` with `ILogger<T>`. (Factor XI)
4. Make initializers awaitable (`async Task`); honor cancellation tokens in consumers; add graceful shutdown hooks. (Factor IX)
5. Decouple migrations/seeding from web startup; run them as a pre-deploy job applied consistently across environments. (Factors X, XII)
