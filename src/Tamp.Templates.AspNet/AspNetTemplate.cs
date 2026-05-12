using System.Collections.Generic;
using Tamp.Scaffold;

namespace Tamp.Templates.AspNet;

/// <summary>
/// Scaffolds a minimal ASP.NET Web API project plus a Tamp build script that
/// knows how to build, test, pack, and publish it. The step up from the CLI's
/// embedded "minimal" template: an actual API csproj + Program.cs in place.
/// </summary>
public sealed class AspNetTemplate : IScaffoldTemplate
{
    public string Name => "aspnet";
    public string Description => "ASP.NET minimal-API web service with a Tamp build script (Compile / Test / Pack / Publish).";
    public string MinimumTampCoreVersion => "1.4.0";

    public IEnumerable<FileSpec> Render(ScaffoldContext ctx)
    {
        const string ProjectName = "WebApi";

        yield return new FileSpec(
            ctx.RepoRoot / $"{ProjectName}.slnx",
            RenderSolution(ProjectName),
            WriteMode.Create);

        yield return new FileSpec(
            ctx.RepoRoot / "src" / ProjectName / $"{ProjectName}.csproj",
            RenderApiCsproj(),
            WriteMode.Create);

        yield return new FileSpec(
            ctx.RepoRoot / "src" / ProjectName / "Program.cs",
            RenderApiProgram(),
            WriteMode.Create);

        yield return new FileSpec(
            ctx.RepoRoot / "build" / "Build.cs",
            RenderBuildCs(ProjectName),
            WriteMode.Create);

        yield return new FileSpec(
            ctx.RepoRoot / "build" / "Build.csproj",
            RenderBuildCsproj(ctx),
            WriteMode.Create);

        yield return new FileSpec(
            ctx.RepoRoot / ".config" / "dotnet-tools.json",
            RenderToolsJson(ctx),
            WriteMode.SkipIfExists);

        yield return new FileSpec(
            ctx.RepoRoot / "tamp.sh",
            RenderTampSh(),
            WriteMode.SkipIfExists) { Executable = true };

        yield return new FileSpec(
            ctx.RepoRoot / "tamp.cmd",
            RenderTampCmd(),
            WriteMode.SkipIfExists);
    }

    internal static string RenderSolution(string projectName)
        => $$"""
        <Solution>
          <Folder Name="/src/">
            <Project Path="src/{{projectName}}/{{projectName}}.csproj" />
          </Folder>
        </Solution>

        """;

    internal static string RenderApiCsproj()
        => """
        <Project Sdk="Microsoft.NET.Sdk.Web">
          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
          </PropertyGroup>
        </Project>

        """;

    internal static string RenderApiProgram()
        => """
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/", () => "WebApi up.");
        app.MapGet("/health/live", () => Results.Ok(new { status = "ok" }));

        app.Run();

        """;

    internal static string RenderBuildCs(string projectName)
        => $$"""
        using Tamp;
        using Tamp.NetCli.V10;

        class Build : TampBuild
        {
            public static int Main(string[] args) => Execute<Build>(args);

            [Parameter("Build configuration")]
            Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

            [Solution] readonly Solution Solution = null!;

            AbsolutePath Artifacts => RootDirectory / "artifacts";
            AbsolutePath ApiProject => RootDirectory / "src" / "{{projectName}}" / "{{projectName}}.csproj";

            Target Clean => _ => _.Executes(() => CleanArtifacts());

            Target Restore => _ => _
                .Internal()
                .Executes(() => DotNet.Restore(s => s.SetProject(Solution.Path)));

            Target Compile => _ => _
                .DependsOn(Restore)
                .Executes(() => DotNet.Build(s => s
                    .SetProject(Solution.Path)
                    .SetConfiguration(Configuration)
                    .SetNoRestore(true)));

            Target Test => _ => _
                .DependsOn(Compile)
                .Executes(() => DotNet.Test(s => s
                    .SetProject(Solution.Path)
                    .SetConfiguration(Configuration)
                    .SetNoBuild(true)));

            Target Publish => _ => _
                .DependsOn(Compile)
                .Executes(() => DotNet.Publish(s => s
                    .SetProject(ApiProject)
                    .SetConfiguration(Configuration)
                    .SetNoBuild(true)
                    .SetOutput(Artifacts / "publish")));

            Target Default => _ => _
                .Default()
                .DependsOn(Compile);
        }

        """;

    internal static string RenderBuildCsproj(ScaffoldContext ctx)
        => $$"""
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <OutputType>Exe</OutputType>
            <TargetFramework>net10.0</TargetFramework>
            <IsPackable>false</IsPackable>
            <RootNamespace>Build</RootNamespace>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include="Tamp.Core" Version="{{ctx.TampCoreVersion}}" />
            <PackageReference Include="Tamp.NetCli.V10" Version="{{ctx.TampCoreVersion}}" />
          </ItemGroup>
        </Project>

        """;

    internal static string RenderToolsJson(ScaffoldContext ctx)
        => $$"""
        {
          "version": 1,
          "isRoot": true,
          "tools": {
            "dotnet-tamp": {
              "version": "{{ctx.TampCoreVersion}}",
              "commands": ["dotnet-tamp"]
            }
          }
        }

        """;

    internal static string RenderTampSh()
        => """
        #!/usr/bin/env bash
        # Tamp build-script entry point. Scaffolded by `tamp init --template aspnet`.
        set -euo pipefail

        SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
        STAMP="$SCRIPT_DIR/.config/.tamp-tools-restored"

        if [ -f "$SCRIPT_DIR/.config/dotnet-tools.json" ] && [ ! -f "$STAMP" ]; then
            (cd "$SCRIPT_DIR" && dotnet tool restore) && mkdir -p "$SCRIPT_DIR/.config" && touch "$STAMP"
        fi

        cd "$SCRIPT_DIR" && exec dotnet tamp "$@"
        """;

    internal static string RenderTampCmd()
        => """
        @echo off
        rem Tamp build-script entry point. Scaffolded by `tamp init --template aspnet`.
        setlocal

        set "SCRIPT_DIR=%~dp0"
        if "%SCRIPT_DIR:~-1%"=="\" set "SCRIPT_DIR=%SCRIPT_DIR:~0,-1%"
        set "STAMP=%SCRIPT_DIR%\.config\.tamp-tools-restored"

        if exist "%SCRIPT_DIR%\.config\dotnet-tools.json" if not exist "%STAMP%" (
            pushd "%SCRIPT_DIR%" >nul
            dotnet tool restore || exit /b 1
            popd >nul
            if not exist "%SCRIPT_DIR%\.config" mkdir "%SCRIPT_DIR%\.config"
            type nul > "%STAMP%"
        )

        pushd "%SCRIPT_DIR%" >nul
        dotnet tamp %*
        set "EXIT=%ERRORLEVEL%"
        popd >nul
        exit /b %EXIT%
        """;
}
