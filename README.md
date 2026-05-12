# Tamp.Templates

NuGet-distributed templates for [Tamp](https://github.com/tamp-build/tamp)'s `tamp init` scaffolder.

The Tamp CLI ships one **embedded** template (`minimal`) so the on-ramp works offline. Additional templates live in this repo as separate NuGet packages and are pulled on demand when an adopter runs `tamp init --template <name>` (CLI 0.2.0+).

| Package | What it scaffolds | Status |
|---|---|---|
| `Tamp.Templates.AspNet` | ASP.NET minimal-API web service + Tamp build script with Compile / Test / Publish targets | preview |

## How template distribution works

```
adopter:   tamp init --template aspnet
   │
   ▼
CLI 0.2.0+:
  1. Embedded source: does it carry "aspnet"? No.
  2. NuGet source:    restore Tamp.Templates.AspNet from configured feeds (uses your existing NuGet config — corporate mirrors, offline cache, auth all honored).
  3. Load:            reflection-discover AspNetTemplateProvider, invoke GetTemplate().
  4. Drift check:     template.MinimumTampCoreVersion vs the CLI's own version. Refuse on mismatch with an upgrade hint.
  5. Render:          AspNetTemplate.Render(ctx) → list of FileSpecs.
  6. Write:           ScaffoldRunner applies each spec to disk.
```

The CLI registers sources **embedded first → NuGet second**, so:
- `tamp init` (no flags) always works offline.
- `tamp init --template foo` only goes to NuGet if no embedded template matches.

## Authoring a template package

Template packages depend on exactly one thing: **`Tamp.Core`** — the scaffold contracts (`IScaffoldTemplate`, `IScaffoldTemplateProvider`, `FileSpec`, `WriteMode`, `ScaffoldContext`) live there.

Minimum viable template package:

```csharp
// MyTemplate.cs
using Tamp.Scaffold;

public sealed class MyTemplate : IScaffoldTemplate
{
    public string Name => "my-shape";
    public string Description => "Scaffold a my-shape project.";
    public string MinimumTampCoreVersion => "1.4.0";

    public IEnumerable<FileSpec> Render(ScaffoldContext ctx)
    {
        yield return new FileSpec(
            ctx.RepoRoot / "src" / "MyApp" / "Program.cs",
            "// my-shape scaffolded by tamp init\n",
            WriteMode.Create);
        // ... more files ...
    }
}

// MyTemplateProvider.cs — the entry point the CLI's NuGet source discovers.
public sealed class MyTemplateProvider : IScaffoldTemplateProvider
{
    public IScaffoldTemplate GetTemplate() => new MyTemplate();
}
```

Csproj surface — note the four `<Tamp*>` metadata properties the CLI's NuGet source reads:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net8.0;net9.0;net10.0</TargetFrameworks>
    <PackageId>Foo.Tamp.Templates.MyShape</PackageId>
    <IsPackable>true</IsPackable>

    <TampTemplateName>my-shape</TampTemplateName>
    <TampTemplateDescription>Scaffold a my-shape project.</TampTemplateDescription>
    <MinimumTampCoreVersion>1.4.0</MinimumTampCoreVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Tamp.Core" Version="1.4.0" />
  </ItemGroup>
</Project>
```

Publish to nuget.org and adopters can `tamp init --template-source Foo.Tamp.Templates.MyShape` immediately.

## Repository layout

```
tamp-templates/
├── src/
│   └── Tamp.Templates.AspNet/        # the ASP.NET template package
├── tests/
│   └── Tamp.Templates.AspNet.Tests/  # unit tests pinning the rendered file set
├── build/
│   └── Build.cs                      # Tamp build script (Restore/Compile/Test/Pack/Push)
└── .github/workflows/
    ├── ci.yml                        # build + test on every push/PR
    └── release.yml                   # tag-triggered pack + push to nuget.org
```

## Building this repo

```bash
git clone https://github.com/tamp-build/tamp-templates
cd tamp-templates
dotnet test                            # runs every template package's tests
dotnet tool install -g dotnet-tamp     # if not already installed
dotnet tamp Ci                         # full pipeline through the dogfooded build script
```

## Releasing a template package

Tags drive releases. Bump `<Version>` in `Directory.Build.props`, commit, tag, push:

```bash
# Edit Directory.Build.props → <Version>0.1.1</Version>
git commit -am "release: Tamp.Templates.AspNet 0.1.1"
git tag -a v0.1.1 -m "Tamp.Templates.AspNet 0.1.1"
git push origin main v0.1.1
```

The Release workflow waits for CI to pass on the tagged commit, then `tamp Ci` + `tamp Push` from the dogfooded Build.cs.

## License

MIT. See [LICENSE](LICENSE).
