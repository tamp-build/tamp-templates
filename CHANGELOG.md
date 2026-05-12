# Changelog

All notable changes to **Tamp.Templates.*** packages are recorded here.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/);
versions follow [SemVer](https://semver.org/spec/v2.0.0.html).

## [0.1.0] — 2026-05-12

### Added

- **`Tamp.Templates.AspNet`** — first template package. `tamp init --template aspnet` (CLI 0.2.0+) scaffolds:
  - `WebApi.slnx` solution
  - `src/WebApi/WebApi.csproj` — minimal-API web project (Microsoft.NET.Sdk.Web)
  - `src/WebApi/Program.cs` — minimal-API endpoints (`/`, `/health/live`)
  - `build/Build.cs` — Tamp build script extending the minimal template with a `Publish` target
  - `build/Build.csproj`, `.config/dotnet-tools.json`, `tamp.sh`, `tamp.cmd`

- `MinimumTampCoreVersion = 1.4.0` declared on the package — drift-protection gate the CLI enforces at load time.

- CI (`ci.yml`) and Release (`release.yml`) workflows mirror the satellite-fanout pattern: build/test on every push, tag-triggered pack + push via dogfooded `tamp Ci`/`tamp Push`.
