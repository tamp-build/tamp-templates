# Changelog

All notable changes to **Tamp.Templates.*** packages are recorded here.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/);
versions follow [SemVer](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.2.0] - 2026-11-20 - .NET 11 in, .NET 8 and .NET 9 out

### Changed

- **Multi-targets `net10.0;net11.0`** instead of `net8.0;net9.0;net10.0` ([ADR 0015](https://github.com/tamp-build/tamp/blob/main/docs/adr/0015-target-framework-strategy.md)). .NET 11 shipped 2026-11-10 and .NET 8 and .NET 9 reached end of support the same day; the rule is to add a TFM the day a release ships and drop one the day Microsoft ends its support. `global.json` pins SDK 11.0.100.

  **If you target `net8.0` or `net9.0`, stay on the previous version** — this one carries no assets your project can resolve.

- **`Tamp.*` pinned to 1.21.0**, which carries a SARIF fix worth knowing about if this project's output is read as evidence: before 1.21.0, a SARIF result whose scanner omitted `level` was read as `none` — the least severe value — instead of the declared `warning`, on every runtime before net11 ([tamp#166](https://github.com/tamp-build/tamp/issues/166)).

- **Publishing moved to nuget.org trusted publishing (OIDC).** The `tamp_nuget_api_key` org secret expired 2026-11-01 along with every other pre-2026-08-17 nuget.org key. The publish job now mints a short-lived key from its own OIDC token, so there is no long-lived credential to rotate or leak.

## [0.1.1] — 2026-09-27

### Added

- Package now ships XML documentation files (`.xml`) alongside the assembly, so consumers get IntelliSense and API docs. (Mirrors [tamp-build/tamp#3](https://github.com/tamp-build/tamp/pull/50).)


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
