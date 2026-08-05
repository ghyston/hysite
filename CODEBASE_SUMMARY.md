# Hysite Codebase Summary

This repository contains the source for `hyston.blog`, a small .NET blog engine.
The app reads Markdown posts from a Git repository, parses them into blog posts,
stores them in PostgreSQL through Entity Framework Core, renders them through
Razor Pages, and exposes an RSS feed.

## Solution Structure

- `src/Web`: ASP.NET Core Razor Pages application and HTTP controllers.
- `src/Application`: application interfaces, MediatR commands/queries, repository implementation, DTO validation.
- `src/Infrastructure`: EF Core/PostgreSQL persistence, Git access, Markdown parsing, RSS generation, version service.
- `src/Domain`: domain entities, DTOs, and shared result type.
- `tests/Web.UnitTests`: xUnit tests for selected web/parser behavior.

The solution targets .NET 8. The SDK is pinned by `global.json` to `8.0.100`
with `latestFeature` roll-forward.

## Main Runtime Flow

`src/Web/Program.cs` is the entry point. It:

- Configures console and file logging.
- Registers Application, Infrastructure, and Web services.
- Applies EF Core database migrations at startup.
- Ensures local log, post, and image directories exist.
- Configures Razor Pages, controllers, static files, HTTPS redirection, and error handling.

`src/Web/Services/StartupService.cs` runs during host startup. When the
`loadFromGit` configuration value is true, it deletes the configured local posts
folder and sends `CloneContentCmd` through MediatR.

## Content Loading

Blog content is loaded from a Git repository configured by:

- `PostsGitUrl`
- `PostsLocalPath`
- `GithubUser`
- `GithubToken` or `READER_TOKEN`

`src/Infrastructure/Services/GitService.cs` handles Git operations using
LibGit2Sharp. It clones the `publish` branch on startup and pulls updates when
the webhook endpoint is called.

`src/Application/Commands/CloneContentCmd.cs` performs initial content loading:

1. Loads and validates Git settings.
2. Clones the content repository.
3. Parses Markdown files from `PostsLocalPath`.
4. Creates `BlogPost` entities and associated `BlogTag` entities.
5. Converts Markdown to HTML.
6. Replaces all blog posts in the database.
7. Writes the RSS feed file.

`src/Application/Commands/UpdatePostsCommand.cs` handles webhook-triggered
updates. It validates the GitHub HMAC signature, pulls the content repository,
reparses posts, replaces stored posts, and regenerates RSS.

## Markdown Post Format

Posts are Markdown files with a small custom header:

```text
Title
yyyy/MM/dd HH:mm
tag1,tag2
@@@
Markdown body
```

Parsing is implemented in `src/Infrastructure/Services/FileParserService.cs`.
Important parser behavior:

- Files are loaded recursively from the configured posts folder.
- Only `.md` files are parsed.
- Filenames must not contain spaces.
- The filename without `.md`, lowercased, becomes the public post slug.
- Dates are parsed with the exact format `yyyy/MM/dd HH:mm`.
- The first metadata line before `@@@` is treated as comma-separated tags.
- Markdown is converted to HTML with Markdig advanced extensions, ColorCode, and footnotes.

## Domain Model

Main domain types:

- `BlogPost`: id, filename/slug, title, Markdown content, HTML content, created date, tags.
- `BlogTag`: tag name as key, many-to-many relationship with blog posts.
- `ViewStatistic`: present in the model but view-counting behavior is currently not implemented.

DTOs:

- `BlogPostDto`: parser output containing created date, tags, title, filename, and Markdown content.

## Persistence

`src/Infrastructure/Persistance/AppDbContext.cs` is the EF Core context and
implements `IHysiteContext`.

Database sets:

- `BlogPosts`
- `BlogTags`
- `ViewStatistics`

Infrastructure registers PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL`
and applies snake_case naming conventions. Migrations live in
`src/Infrastructure/Migrations`.

`BlogTagConfiguration` configures `BlogTag.Name` as the primary key with a max
length of 100.

## Repository Layer

`src/Application/Interfaces/IBlogPostRepository.cs` defines blog post access.
`src/Application/Repositories/BlogPostRepository.cs` implements it.

Common repository operations:

- Create `BlogPost` entities from parsed DTOs.
- Reuse or create `BlogTag` entities.
- Find a post by filename/slug.
- Retrieve paginated posts ordered newest first.
- Retrieve posts by year.
- Retrieve all available years.
- Retrieve all tag names.
- Retrieve all posts for RSS generation.
- Count posts.
- Find previous and next posts based on created date.

## Web UI

The app uses Razor Pages and minimal styling from:

- `src/Web/wwwroot/css/retro.css`
- `src/Web/wwwroot/css/hyston.blog.css`

Shared layout is in `src/Web/Pages/_Layout.cshtml`.

Main pages:

- `/` and `/{pageNumber:int}`: paginated index via `Index.cshtml` and `Index.cshtml.cs`.
- `/{postName}`: post detail via `Post.cshtml` and `Post.cshtml.cs`.
- `/Archive`: archive overview via `Archive.cshtml` and `Archive.cshtml.cs`.
- `/Year/{year}`: posts for a year via `Year.cshtml` and `Year.cshtml.cs`.
- `/About`: version/framework page via `About.cshtml` and `About.cshtml.cs`.

`src/Web/Pages/BlogEntry.cshtml` is the shared partial that renders a blog post
title, date, HTML content, tags, and separator.

Routing is configured in `src/Web/DependencyInjection.cs`:

- `/Index` is also routed to the empty path.
- `/Post` is routed to `{postname}`.
- Default controller route is also mapped for controllers.

## HTTP Controllers

`src/Web/Controllers/GitController.cs`:

- Route: `/update`
- Reads `X-Hub-Signature` and request body.
- Sends `UpdatePostsCommand`.

`src/Web/Controllers/RssController.cs`:

- Route: `/rss`
- Sends `RssFeedQuery`.
- Returns the RSS XML file as `application/rss+xml; charset=utf-8`.

`src/Web/Controllers/PingController.cs` exists for ping/health-style behavior.

## RSS

RSS generation is implemented by `src/Infrastructure/Services/RssService.cs`
using `System.ServiceModel.Syndication`.

`src/Application/Queries/RssFeedQuery.cs` returns the configured RSS path and
creates the feed if the file is missing.

The RSS file name comes from `RssFeedFile`; the path is built under
`PostsLocalPath`.

## Configuration

Base configuration is in `src/Web/appsettings.json`.

Notable keys:

- `PostsPerPage`
- `PostsGitUrl`
- `PostsLocalPath`
- `LogsLocalPath`
- `RssFeedFile`
- `CertPath`

Production configuration is in `src/Web/appsettings.Production.json`.

Notable production keys:

- `loadFromGit`
- `GithubUser`
- `GithubToken`
- `GithubHookSecret`
- `ConnectionStrings:Database`

The Docker image also accepts:

- `HYSITE_VERSION`: displayed as a short version string.
- `READER_TOKEN`: preferred Git token source for reading the content repository.

## Deployment

The app is containerized with `Dockerfile`.

`docker-compose.yml` runs:

- `hysite`: the web app container.
- `db`: PostgreSQL 15.4.

The compose file mounts certificates and logs, exposes ports 80 and 443, and
uses a named Docker volume for PostgreSQL data.

GitHub Actions workflow:

- `.github/workflows/deploy_to_linode.yml`
- Runs restore, build, and tests.
- Builds and pushes `hyston/hysite:latest`.
- Restarts the Linode deployment over SSH with Docker Compose.

## Tests

The test project is `tests/Web.UnitTests`.

Current test coverage includes:

- `FileParserTests`: verifies parser success for title, date, and tags.
- `AboutModelTests`: verifies version/framework values are read from `IVersionService`.

Useful verification commands:

```bash
dotnet restore
dotnet build
dotnet test
```

## Common Change Areas

- Blog post parsing behavior: `FileParserService`.
- Blog listing, archive, tags, pagination, and navigation: `IBlogPostRepository`, `BlogPostRepository`, and Razor page models.
- Page HTML: files under `src/Web/Pages`.
- Styling: `src/Web/wwwroot/css/hyston.blog.css`.
- RSS behavior: `RssFeedService`, `RssFeedQuery`, `RssController`.
- GitHub webhook update behavior: `GitController`, `UpdatePostsCommand`, `GitService`.
- EF schema changes: domain model, `AppDbContext`, configuration classes, and migrations.
