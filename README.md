# Usher Management API (UMS)

Backend API for managing **ushers** (field staff): guest applications, profiles, approval flows, and related identity data. The codebase follows a layered layout (`Domain`, `Application`, `Infrastructure`, `Contracts`, `UMS.api`) with Entity Framework Core and PostgreSQL.

## Stack

| Area | Choice |
|------|--------|
| Runtime | **.NET 10** (`net10.0`) |
| Web host | ASP.NET Core (`UMS.api`) |
| Data | **EF Core 10** + **Npgsql** |
| API docs | **NSwag** (package referenced on the API project) |
| Tests | **xUnit** (`Tests/UMS.tests`) |

Central package versions live in [`Directory.Packages.props`](Directory.Packages.props).

## Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download) (matches [CI](.github/workflows/ci.yml); local machines must use a 10.x SDK to build).
- **PostgreSQL** reachable via `ConnectionStrings:DefaultConnection`. The model expects PostgreSQL extensions **`pgcrypto`** and **`citext`** (see [`AppDbContext`](src/UMS.Infrastructure/Persistence/Context/AppDbContext.cs)).

## Configuration

- Default connection string is in [`src/UMS.api/appsettings.json`](src/UMS.api/appsettings.json) under `ConnectionStrings:DefaultConnection`. Replace host, database, user, and password for your environment.
- For local development, prefer [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) or environment variables instead of committing real credentials.

```bash
cd src/UMS.api
dotnet user-secrets init   # once per machine/repo clone
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=Usher_mgmt;Username=…;Password=…"
```

## Build and run

From the repository root:

```bash
dotnet restore
dotnet build src/UMS.api/UMS.api.csproj
dotnet run --project src/UMS.api/UMS.api.csproj
```

HTTP URLs are defined in [`src/UMS.api/Properties/launchSettings.json`](src/UMS.api/Properties/launchSettings.json) (for example `http://localhost:5225`).

### Solution file

The repo includes [`UMS.slnx`](UMS.slnx) (XML solution format). If your tooling does not open `.slnx` yet, build individual projects as shown above.

### Database migrations

Migrations live under `src/UMS.Infrastructure/Persistence/Migrations/`. Apply them after PostgreSQL is running and the connection string is set:

```bash
dotnet ef database update \
  --project src/UMS.Infrastructure/UMS.Infrastructure.csproj \
  --startup-project src/UMS.api/UMS.api.csproj
```

Install the EF CLI if needed: `dotnet tool install --global dotnet-ef`.

## Tests

```bash
dotnet test Tests/UMS.tests/UMS.tests.csproj
```

## CI (`ci.yml`)

On every **pull request**, GitHub Actions:

1. Checks out the repo  
2. Installs **.NET SDK 10.0.x**  
3. Runs `dotnet restore`  
4. Runs `dotnet format --verify-no-changes` (formatting must match the repo)  
5. Runs `dotnet build --no-restore`  
6. Runs `dotnet test --no-build`

Workflow file: [`.github/workflows/ci.yml`](.github/workflows/ci.yml).

## Sync PRs with `main` (`synch-prs-main.yml`)

When **`main`** receives a push, a workflow merges the latest `main` into each **open PR** whose base is `main`. If the merge conflicts, it aborts and posts a comment on that PR asking for a manual resolution.

Requirements: `contents: write` and `pull-requests: write` for `GITHUB_TOKEN` (already set in the workflow). Workflow file: [`.github/workflows/synch-prs-main.yml`](.github/workflows/synch-prs-main.yml).

## Domain overview

Core entities include **`User`** (roles such as guest vs usher, email verification, refresh tokens) and **`Usher`** (profile, approval status, documents). Domain logic lives in [`src/UMS.Domain`](src/UMS.Domain). The HTTP API currently exposes the default ASP.NET Core template controller alongside registered infrastructure services; feature endpoints can be added under `UMS.api`.

---

**Security:** Do not deploy with the sample password from `appsettings.json`. Use secrets management appropriate to your environment.
