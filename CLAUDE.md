# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

**Database migrations** (run once, or after pulling schema changes):
```bash
cd src/PopBlog.Database
dotnet run "server=localhost;Database=popblog;Trusted_Connection=True;TrustServerCertificate=True;"
```

**Web application:**
```bash
cd src/PopBlog.Web
dotnet run
# https://localhost:5001 / http://localhost:5000
```

**Frontend assets** (run from `src/PopBlog.Web` after `npm install`):
```bash
npm install
npx gulp        # copies Bootstrap/TinyMCE, transpiles/minifies JS and CSS
```

**Run tests:**
```bash
dotnet test src/PopBlog.Tests/
```

**Build solution:**
```bash
dotnet build POPBlog.sln
```

## Architecture

The solution has four projects:

- **`PopBlog.Mvc`** — Razor Class Library; the reusable blog engine. Contains all controllers, models, services, repositories, and views. Distributed as a NuGet package.
- **`PopBlog.Web`** — The deployable ASP.NET Core 8 host. References `PopBlog.Mvc` as a project dependency and can override views/layouts. The entry point is `Program.cs`.
- **`PopBlog.Database`** — Console app using DbUp; runs numbered SQL migration scripts in `/Scripts/` to create or upgrade the database schema.
- **`PopBlog.Tests`** — xUnit test project.

### Data flow in `PopBlog.Mvc`

Controllers → Services → Repositories → SQL Server (via Dapper)

- **Repositories** are thin Dapper wrappers over raw SQL. Each maps to one or two tables.
- **Services** own business logic: image resizing (ImageSharp), audio metadata (TagLibSharp), RSS/podcast feed generation, reCAPTCHA validation, IP-ban checks, timezone conversion.
- **`StorageRepository`** wraps Azure Blob Storage for podcast audio files.
- **`UserMiddleware`** runs on every request to populate user context for authorization.
- **`Config`** (bound in `ServiceCollections.cs`) reads all application settings from `appsettings.json`.

### Key configuration (`appsettings.json`)

| Key | Purpose |
|-----|---------|
| `PopBlogConnectionString` | SQL Server connection |
| `ReCaptchaSiteKey` / `ReCaptchaSecretKey` | Google reCAPTCHA for comment spam |
| `StorageConnectionString` / `StorageContainerName` | Azure Blob Storage for podcasts |
| `TimeZone` | UTC offset for display |
| `IsUsingDirectDownload` | Skip download-counter redirect for large files |

### Frontend

Gulp (`gulpfile.js` in `PopBlog.Web`) handles:
- Copying Bootstrap 5 and TinyMCE 6 into `wwwroot/lib/`
- Babel-transpiling and uglifying custom JS → `wwwroot/lib/PopBlog/dist/`
- Minifying custom CSS with source maps

TinyMCE is the rich-text editor for post creation/editing in the admin UI.
