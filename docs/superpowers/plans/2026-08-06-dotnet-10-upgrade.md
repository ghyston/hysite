# .NET 10 Upgrade Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move Hysite from .NET 8 to .NET 10 while keeping the web app, EF Core migrations, PostgreSQL integration, Docker deployment, and unit tests working.

**Architecture:** Upgrade the SDK, target frameworks, Microsoft/EF package family, and deployment images as one vertical change. Keep behavioral refactors out of the framework migration except where package major-version changes require source fixes, especially MediatR handler signatures and EF design-time context creation.

**Tech Stack:** ASP.NET Core Razor Pages/controllers, EF Core + Npgsql PostgreSQL provider, MediatR, FluentValidation, LibGit2Sharp, Markdig, xUnit, Docker, GitHub Actions.

## Global Constraints

- Keep changes small and focused on the .NET 10 migration.
- Do not stage changes or create commits.
- Keep request/command behavior in the Application commands layer; web routing files stay thin.
- Use `net10.0` for all local projects after the upgrade.
- Keep the database provider family aligned: EF Core packages at `10.0.10`, `Npgsql.EntityFrameworkCore.PostgreSQL` at `10.0.3`, `EFCore.NamingConventions` at `10.0.1`.
- Before reporting implementation complete, run `dotnet test tests/Web.UnitTests/Web.UnitTests.csproj`, `dotnet build`, and `git diff --check`.
- If plain `dotnet build` hangs locally, use `dotnet build hysite.sln --no-restore --disable-build-servers -m:1 -p:UseSharedCompilation=false -nodeReuse:false` after a successful restore/test run and report the substitution.

---

## Current Findings

Baseline from 2026-08-06:

- `global.json` pins SDK `8.0.100` with `rollForward: latestFeature`, so the repo selects SDK `8.0.411` even though SDK `10.0.203` is installed locally.
- All four source projects and the test project target `net8.0`.
- `dotnet sln hysite.sln list` includes only `src/Domain/Domain.csproj`, `src/Web/hysite.csproj`, and `tests/Web.UnitTests/Web.UnitTests.csproj`; it omits `src/Application/Application.csproj` and `src/Infrastructure/Infrastructure.csproj`.
- `dotnet test tests/Web.UnitTests/Web.UnitTests.csproj` passes on .NET 8: 34 passed.
- Plain `dotnet build` hung locally. The project-prescribed fallback build passed.
- Current compile/test restore emitted one nullable warning in `src/Infrastructure/Services/FileParserService.cs` around metadata parsing.
- `dotnet ef migrations list --project src/Infrastructure/Infrastructure.csproj --startup-project src/Web/hysite.csproj --context AppDbContext` hung at build. The same command with `--no-build` also hung, so EF design-time startup needs isolation before EF 10 migration work.
- `dotnet-ef` global tool is `10.0.7`, while latest EF packages found are `10.0.10`.
- `src/Web/appsettings.Development.json` contains a credential-like GitHub token value. Rotate it and move it to user secrets or environment variables before migration/CI work that may print configuration.

## Package Matrix

NuGet package health checks were run against `https://api.nuget.org/v3/index.json`.

| Project | Package | Current | Latest / Recommended | Upgrade note |
| --- | --- | ---: | ---: | --- |
| Web | `Microsoft.EntityFrameworkCore.Design` | `8.0.0` | `10.0.10` | Required with EF Core 10 tooling. |
| Web | `Serilog.Extensions.Logging.File` | `3.0.0` | `3.0.0` stable | No stable update. NuGet computes net10 compatibility, but package is old; consider replacing later with `Serilog.AspNetCore` + `Serilog.Sinks.File`. |
| Application | `FluentValidation` | `11.4.0` | `12.1.1` | Major upgrade; FluentValidation 12 supports .NET 8+ and removes older platform support. |
| Application | `FluentValidation.DependencyInjectionExtensions` | `11.4.0` | `12.1.1` | Keep aligned with `FluentValidation`. |
| Application | `MediatR` | `11.1.0` | `12.5.0` or `14.2.0` | Prefer `12.5.0` unless the project accepts MediatR 13+ commercial/RPL licensing. Latest is `14.2.0`. |
| Application | `MediatR.Extensions.Microsoft.DependencyInjection` | `11.0.0` | remove | Functionality moved into `MediatR` 12+. |
| Application | `Microsoft.EntityFrameworkCore` | `8.0.0` | `10.0.10` | Required with `net10.0` EF stack. |
| Application | `Microsoft.Extensions.Configuration` | `8.0.0` | `10.0.10` | Keep Microsoft.Extensions family aligned. |
| Infrastructure | `EFCore.NamingConventions` | `8.0.0-rc.2` | `10.0.1` | Current version is pre-release; update to stable 10.x. |
| Infrastructure | `LibGit2Sharp` | `0.30.0` | `0.32.0` | Validate native assets in Docker/Linux. |
| Infrastructure | `Markdig` | `0.33.0` | `1.3.2` | Re-run markdown rendering tests/snapshots. |
| Infrastructure | `Markdown.ColorCode` | `2.0.0` | `3.0.1` | Re-run syntax highlighting tests/snapshots. |
| Infrastructure | `Microsoft.EntityFrameworkCore` | `8.0.0` | `10.0.10` | Required. |
| Infrastructure | `Microsoft.EntityFrameworkCore.Design` | `8.0.0` | `10.0.10` | Required for migrations. Mark `PrivateAssets=all`. |
| Infrastructure | `Microsoft.EntityFrameworkCore.InMemory` | `8.0.0` | `10.0.10` | Required for repository tests. |
| Infrastructure | `Microsoft.Extensions.FileProviders.Abstractions` | `8.0.0` | `10.0.10` | Keep Microsoft.Extensions family aligned. |
| Infrastructure | `Npgsql.EntityFrameworkCore.PostgreSQL` | `8.0.0` | `10.0.3` | Required for EF Core 10 provider compatibility. |
| Infrastructure | `System.ServiceModel.Syndication` | `8.0.0` | `10.0.10` | Keep runtime package aligned. |
| Web.UnitTests | `coverlet.collector` | `3.1.2` | `10.0.1` | Major upgrade; verify coverage command if used in CI. |
| Web.UnitTests | `FluentAssertions` | `6.9.0` | `7.2.2` or `8.10.0` | Prefer `7.2.2` unless the project accepts FluentAssertions 8 commercial-use licensing. |
| Web.UnitTests | `Microsoft.NET.Test.Sdk` | `17.3.2` | `18.8.1` | Required for modern SDK/test platform compatibility. |
| Web.UnitTests | `Moq` | `4.18.4` | `4.20.72` | Low-risk update. |
| Web.UnitTests | `xunit` | `2.4.2` | `2.9.3` or `xunit.v3` | NuGet reports `xunit` as legacy; xUnit v3 migration is larger and changes package names/project shape. |
| Web.UnitTests | `xunit.runner.visualstudio` | `2.4.5` | `3.1.5` | Use with either latest v2 or v3 strategy after verification. |

No vulnerable packages were reported by `dotnet list package --vulnerable` for the audited projects.

## Risk Register

- **High: EF design-time commands hang.** Add an `IDesignTimeDbContextFactory<AppDbContext>` so migrations can be listed, added, and scripted without executing web startup, hosted services, certificate checks, or runtime migration code.
- **High: Production database migration behavior.** `Program.cs` calls `scope.MigrateDatabase()` on startup. With EF 10 and Npgsql 10, verify generated SQL against a disposable PostgreSQL database before deployment.
- **High: Committed credential.** Rotate the GitHub token in `appsettings.Development.json`, remove it from source, and use user secrets or environment variables.
- **Medium: Solution file is incomplete.** Package audits against `hysite.sln` currently miss Application and Infrastructure direct package references.
- **Medium: MediatR major upgrade.** MediatR 12 changes registration syntax and void request handlers. MediatR 13+ adds commercial/RPL licensing considerations.
- **Medium: Test package licensing.** FluentAssertions 8 introduces commercial-use licensing. Choose `7.2.2` or accept the v8 terms explicitly.
- **Medium: xUnit v3 migration.** xUnit v3 changes package names and test project executable behavior. It is not necessary for the first net10 cut.
- **Medium: Docker base image behavior.** .NET 10 container images use Ubuntu by default. Validate native LibGit2Sharp assets and certificate path behavior in the final container.
- **Low: ASP.NET Core 10 breaking changes.** This app does not appear to use OpenAPI, Blazor, cookie auth, `WebHostBuilder`, or `IActionContextAccessor`, so most documented ASP.NET Core 10 breaks are unlikely to apply.
- **Low: .NET 10 configuration behavior.** .NET 10 preserves null values in configuration. Current code reads many keys as nullable strings; verify missing key behavior in startup tests.

## Files To Modify

- Modify: `global.json` to select .NET 10 SDK.
- Modify: `hysite.sln` to include Application and Infrastructure.
- Modify: `src/Domain/Domain.csproj` target framework.
- Modify: `src/Application/Application.csproj` target framework and package references.
- Modify: `src/Infrastructure/Infrastructure.csproj` target framework and package references.
- Modify: `src/Web/hysite.csproj` target framework and package references.
- Modify: `tests/Web.UnitTests/Web.UnitTests.csproj` target framework and test packages.
- Modify: `src/Application/DependencyInjection.cs` for MediatR 12+ registration.
- Modify: `src/Application/Commands/CloneContentCmd.cs` and `src/Application/Commands/UpdatePostsCommand.cs` if upgrading MediatR past 11.
- Create: `src/Infrastructure/Persistance/AppDbContextFactory.cs` for design-time migrations.
- Modify: `Dockerfile` to use .NET 10 SDK/runtime images.
- Modify: `.github/workflows/deploy_to_linode.yml` to install .NET 10 and refresh old GitHub Action versions.
- Modify: `src/Web/appsettings.Development.json` to remove the token after rotating it outside source control.

---

### Task 1: Normalize Solution Membership

**Files:**
- Modify: `hysite.sln`

**Interfaces:**
- Consumes: Existing project references from `src/Web/hysite.csproj`.
- Produces: A solution where `dotnet list hysite.sln package` audits all direct package references.

- [ ] **Step 1: Add missing projects to the solution**

Run:

```bash
dotnet sln hysite.sln add src/Application/Application.csproj src/Infrastructure/Infrastructure.csproj
```

- [ ] **Step 2: Verify solution membership**

Run:

```bash
dotnet sln hysite.sln list
```

Expected project list:

```text
src/Application/Application.csproj
src/Domain/Domain.csproj
src/Infrastructure/Infrastructure.csproj
src/Web/hysite.csproj
tests/Web.UnitTests/Web.UnitTests.csproj
```

- [ ] **Step 3: Re-run package audit**

Run:

```bash
dotnet list hysite.sln package --outdated
dotnet list hysite.sln package --deprecated
dotnet list hysite.sln package --vulnerable
```

Expected:

```text
Application and Infrastructure appear in the output.
No vulnerable packages are reported before upgrade.
```

### Task 2: Pin .NET 10 SDK And Target Frameworks

**Files:**
- Modify: `global.json`
- Modify: `src/Domain/Domain.csproj`
- Modify: `src/Application/Application.csproj`
- Modify: `src/Infrastructure/Infrastructure.csproj`
- Modify: `src/Web/hysite.csproj`
- Modify: `tests/Web.UnitTests/Web.UnitTests.csproj`

**Interfaces:**
- Consumes: Local SDK `10.0.203` from `dotnet --info`.
- Produces: All projects targeting `net10.0`.

- [ ] **Step 1: Update SDK selection**

Change `global.json` to:

```json
{
    "sdk":
    {
        "version": "10.0.203",
        "rollForward": "latestFeature"
    }
}
```

- [ ] **Step 2: Update all project target frameworks**

In every `.csproj`, replace:

```xml
<TargetFramework>net8.0</TargetFramework>
```

with:

```xml
<TargetFramework>net10.0</TargetFramework>
```

- [ ] **Step 3: Verify SDK and restore**

Run:

```bash
dotnet --version
dotnet restore hysite.sln
```

Expected:

```text
10.0.203
Restore completed successfully.
```

### Task 3: Upgrade Required Microsoft, EF Core, And Npgsql Packages

**Files:**
- Modify: `src/Application/Application.csproj`
- Modify: `src/Infrastructure/Infrastructure.csproj`
- Modify: `src/Web/hysite.csproj`

**Interfaces:**
- Consumes: `net10.0` target frameworks.
- Produces: EF Core 10-compatible app and provider package graph.

- [ ] **Step 1: Update Web EF design package**

In `src/Web/hysite.csproj`, set:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.10">
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
```

Keep:

```xml
<PackageReference Include="Serilog.Extensions.Logging.File" Version="3.0.0" />
```

- [ ] **Step 2: Update Application packages**

Use this package group in `src/Application/Application.csproj`:

```xml
<PackageReference Include="FluentValidation" Version="12.1.1" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="12.1.1" />
<PackageReference Include="MediatR" Version="12.5.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.10" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="10.0.10" />
```

Remove:

```xml
<PackageReference Include="MediatR.Extensions.Microsoft.DependencyInjection" Version="11.0.0" />
```

If the project accepts MediatR 13+ licensing, use `MediatR` `14.2.0` instead and add `MEDIATR_LICENSE_KEY` to production deployment secrets.

- [ ] **Step 3: Update Infrastructure packages**

Use this package group in `src/Infrastructure/Infrastructure.csproj`:

```xml
<PackageReference Include="EFCore.NamingConventions" Version="10.0.1" />
<PackageReference Include="LibGit2Sharp" Version="0.32.0" />
<PackageReference Include="Markdig" Version="1.3.2" />
<PackageReference Include="Markdown.ColorCode" Version="3.0.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.10" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.10" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.10">
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
<PackageReference Include="Microsoft.Extensions.FileProviders.Abstractions" Version="10.0.10" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.3" />
<PackageReference Include="System.ServiceModel.Syndication" Version="10.0.10" />
```

- [ ] **Step 4: Restore**

Run:

```bash
dotnet restore hysite.sln
```

Expected:

```text
Restore completed successfully.
```

### Task 4: Update MediatR Registration And Void Handlers

**Files:**
- Modify: `src/Application/DependencyInjection.cs`
- Modify: `src/Application/Commands/CloneContentCmd.cs`
- Modify: `src/Application/Commands/UpdatePostsCommand.cs`

**Interfaces:**
- Consumes: `MediatR` 12.5.0 or newer.
- Produces: Application command handlers compatible with MediatR 12+.

- [ ] **Step 1: Replace registration overloads**

Change `AddApplication` to:

```csharp
public static void AddApplication(this IServiceCollection services)
{
    services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(CloneContentCmd).Assembly));
    services.AddValidatorsFromAssemblyContaining<GitSettingsDtoValidator>();
    services.AddScoped<IBlogPostRepository, BlogPostRepository>();
}
```

Remove the now-unused `System.Reflection` using if it remains unused.

- [ ] **Step 2: Update `CloneContentHandler` return type**

Change:

```csharp
public async Task<Unit> Handle(CloneContentCmd request, CancellationToken cancellationToken)
```

to:

```csharp
public async Task Handle(CloneContentCmd request, CancellationToken cancellationToken)
```

Replace each `return Unit.Value;` in that handler with:

```csharp
return;
```

- [ ] **Step 3: Update `UpdatePostsCommandHandler` return type**

Change:

```csharp
public async Task<Unit> Handle(UpdatePostsCommand request, CancellationToken cancellationToken)
```

to:

```csharp
public async Task Handle(UpdatePostsCommand request, CancellationToken cancellationToken)
```

Replace each `return Unit.Value;` in that handler with:

```csharp
return;
```

- [ ] **Step 4: Compile just Application**

Run:

```bash
dotnet build src/Application/Application.csproj --no-restore --disable-build-servers -m:1 -p:UseSharedCompilation=false -nodeReuse:false
```

Expected:

```text
Build succeeded.
```

### Task 5: Add EF Design-Time DbContext Factory

**Files:**
- Create: `src/Infrastructure/Persistance/AppDbContextFactory.cs`

**Interfaces:**
- Consumes: `AppDbContext`, `UseNpgsql`, `UseSnakeCaseNamingConvention`.
- Produces: EF tooling entry point that does not execute `Program.cs` startup side effects.

- [ ] **Step 1: Create the factory**

Create `src/Infrastructure/Persistance/AppDbContextFactory.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HySite.Infrastructure.Persistance;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string FallbackConnectionString =
        "Host=localhost;Username=postgres;Password=postgres;Database=hysite_migrations";

    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("HYSITE_MIGRATIONS_CONNECTION")
            ?? FallbackConnectionString;

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new AppDbContext(options);
    }
}
```

- [ ] **Step 2: Update dotnet-ef to match EF package patch**

Run:

```bash
dotnet tool update --global dotnet-ef --version 10.0.10
```

Expected:

```text
Tool 'dotnet-ef' was successfully updated
```

- [ ] **Step 3: Verify EF can list migrations without build**

Run after a successful explicit build:

```bash
dotnet ef migrations list --project src/Infrastructure/Infrastructure.csproj --startup-project src/Web/hysite.csproj --context AppDbContext --no-build
```

Expected:

```text
20230130211411_Init
20250725174102_AddBlogTags
```

### Task 6: Upgrade Test Packages Without Expanding Test Framework Scope

**Files:**
- Modify: `tests/Web.UnitTests/Web.UnitTests.csproj`

**Interfaces:**
- Consumes: Existing xUnit v2 test source.
- Produces: Tests that run on `net10.0` without xUnit v3 source/project migration.

- [ ] **Step 1: Use v2-compatible test package updates**

Use this package group:

```xml
<PackageReference Include="FluentAssertions" Version="7.2.2" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.8.1" />
<PackageReference Include="Moq" Version="4.20.72" />
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="3.1.5">
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
<PackageReference Include="coverlet.collector" Version="10.0.1">
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
```

If the project accepts FluentAssertions 8 commercial-use terms, use `FluentAssertions` `8.10.0` and add the documented license-warning acknowledgement only after legal approval.

- [ ] **Step 2: Run unit tests**

Run:

```bash
dotnet test tests/Web.UnitTests/Web.UnitTests.csproj
```

Expected:

```text
Passed! - Failed: 0, Passed: 34
```

- [ ] **Step 3: Capture xUnit v3 as separate follow-up**

Create a separate plan for xUnit v3 only if the project wants to remove the legacy `xunit` package. That migration should change package names to `xunit.v3`, set the test project output type as required by xUnit v3, and verify `dotnet test` behavior separately.

### Task 7: Update Docker And GitHub Actions

**Files:**
- Modify: `Dockerfile`
- Modify: `.github/workflows/deploy_to_linode.yml`

**Interfaces:**
- Consumes: App targeting `net10.0`.
- Produces: CI and Docker runtime using .NET 10.

- [ ] **Step 1: Update Docker base images**

Change:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
```

to:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
```

Change:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
```

to:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0
```

- [ ] **Step 2: Update GitHub Actions .NET setup**

Change:

```yaml
dotnet-version: '8.0'
```

to:

```yaml
dotnet-version: '10.0.x'
```

- [ ] **Step 3: Refresh old action versions**

Use:

```yaml
uses: actions/checkout@v4
uses: docker/login-action@v3
uses: docker/build-push-action@v6
```

Keep the existing deployment behavior unless the deployment host requires a compose syntax update.

- [ ] **Step 4: Build the Docker image locally**

Run:

```bash
docker build --build-arg HYSITE_VERSION=net10-local --build-arg READER_TOKEN=unused-local -t hysite:net10-local .
```

Expected:

```text
Successfully tagged hysite:net10-local
```

### Task 8: Verify EF Model And Database Behavior

**Files:**
- Modify only if generated migration output reveals a real model difference:
  - `src/Infrastructure/Migrations/*`

**Interfaces:**
- Consumes: EF Core 10 packages, Npgsql 10 provider, design-time factory.
- Produces: Confidence that existing migrations and runtime model still match.

- [ ] **Step 1: Build explicitly before EF commands**

Run:

```bash
dotnet build hysite.sln --no-restore --disable-build-servers -m:1 -p:UseSharedCompilation=false -nodeReuse:false
```

Expected:

```text
Build succeeded.
```

- [ ] **Step 2: Generate a migration probe**

Run:

```bash
dotnet ef migrations add Net10ModelProbe --project src/Infrastructure/Infrastructure.csproj --startup-project src/Web/hysite.csproj --context AppDbContext --no-build
```

Expected:

```text
The migration was added.
```

- [ ] **Step 3: Inspect generated migration**

Open the generated `Net10ModelProbe` migration. Expected acceptable output:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
}

protected override void Down(MigrationBuilder migrationBuilder)
{
}
```

If EF generates column, key, index, timestamp, or join-table changes, stop and review the SQL before keeping the migration.

- [ ] **Step 4: Remove an empty probe migration**

If `Up` and `Down` are empty, run:

```bash
dotnet ef migrations remove --project src/Infrastructure/Infrastructure.csproj --startup-project src/Web/hysite.csproj --context AppDbContext --no-build
```

Expected:

```text
Removing migration 'Net10ModelProbe'.
Reverting the model snapshot.
Done.
```

- [ ] **Step 5: Script existing migrations**

Run:

```bash
dotnet ef migrations script --project src/Infrastructure/Infrastructure.csproj --startup-project src/Web/hysite.csproj --context AppDbContext --idempotent --no-build --output /tmp/hysite-net10-migrations.sql
```

Expected:

```text
/tmp/hysite-net10-migrations.sql is created and contains only the existing Init and AddBlogTags schema operations.
```

### Task 9: Remove Secret From Development Settings

**Files:**
- Modify: `src/Web/appsettings.Development.json`

**Interfaces:**
- Consumes: Existing configuration keys `GithubUser` and `GithubToken`.
- Produces: Source-controlled development settings without a live token.

- [ ] **Step 1: Rotate the exposed token outside the repository**

Revoke the current token in the GitHub account settings before merging any migration work.

- [ ] **Step 2: Replace the token value in source**

Change:

```json
"GithubToken": "<current literal token>"
```

to:

```json
"GithubToken": ""
```

- [ ] **Step 3: Store local token outside source**

Use one of these:

```bash
dotnet user-secrets init --project src/Web/hysite.csproj
dotnet user-secrets set "GithubToken" "<rotated-token>" --project src/Web/hysite.csproj
```

or:

```bash
export GithubToken="<rotated-token>"
```

### Task 10: Final Verification

**Files:**
- Read-only verification over the whole repo.

**Interfaces:**
- Consumes: Completed migration tasks.
- Produces: Evidence that the .NET 10 branch is buildable and testable.

- [ ] **Step 1: Restore**

Run:

```bash
dotnet restore hysite.sln
```

Expected:

```text
Restore completed successfully.
```

- [ ] **Step 2: Run required unit tests**

Run:

```bash
dotnet test tests/Web.UnitTests/Web.UnitTests.csproj
```

Expected:

```text
Passed! - Failed: 0, Passed: 34
```

- [ ] **Step 3: Run required build**

Run:

```bash
dotnet build
```

Expected:

```text
Build succeeded.
```

If it hangs locally, stop it and run:

```bash
dotnet build hysite.sln --no-restore --disable-build-servers -m:1 -p:UseSharedCompilation=false -nodeReuse:false
```

Expected:

```text
Build succeeded.
```

- [ ] **Step 4: Run whitespace check**

Run:

```bash
git diff --check
```

Expected:

```text
No output.
```

- [ ] **Step 5: Run package health checks**

Run:

```bash
dotnet list hysite.sln package --deprecated
dotnet list hysite.sln package --vulnerable
```

Expected:

```text
No vulnerable packages are reported.
Any remaining deprecated package is intentionally accepted in the plan notes.
```

## References Checked

- Microsoft: .NET 10 breaking changes, especially SDK/MSBuild, containers, configuration, and core libraries: https://learn.microsoft.com/en-us/dotnet/core/compatibility/10
- Microsoft: ASP.NET Core 10 breaking changes: https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/10/overview?view=aspnetcore-10.0
- Microsoft: EF Core 10 breaking changes: https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/breaking-changes
- Microsoft: EF Core 10 overview and support statement: https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew
- Npgsql: EF Core provider 10 release notes: https://www.npgsql.org/efcore/release-notes/10.0.html
- FluentValidation: 12.0 upgrade guide: https://docs.fluentvalidation.net/en/latest/upgrading-to-12.html
- MediatR: 11.x to 12.0 migration guide: https://github-wiki-see.page/m/LuckyPennySoftware/MediatR/wiki/Migration-Guide-11.x-to-12.0
- MediatR: 12.0 to 12.1 registration behavior: https://github-wiki-see.page/m/LuckyPennySoftware/MediatR/wiki/Migration-Guide-12.0-to-12.1
- MediatR licensing FAQ for 13+: https://luckypennysoftware.com/faq
- xUnit.net v3 migration guide: https://xunit.net/docs/getting-started/v3/migration
- FluentAssertions v8 upgrade/licensing notes: https://fluentassertions.com/upgradingtov8 and https://fluentassertions.com/introduction
