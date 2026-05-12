using System.Linq;
using Tamp.Scaffold;
using Xunit;

namespace Tamp.Templates.AspNet.Tests;

public sealed class AspNetTemplateTests
{
    private static ScaffoldContext FakeContext() => new()
    {
        RepoRoot = AbsolutePath.Create("/tmp/x"),
        TampCoreVersion = "1.4.0",
    };

    [Fact]
    public void Name_And_Metadata_Are_Stable()
    {
        var t = new AspNetTemplate();
        Assert.Equal("aspnet", t.Name);
        Assert.False(string.IsNullOrWhiteSpace(t.Description));
        Assert.Equal("1.4.0", t.MinimumTampCoreVersion);
    }

    [Fact]
    public void Provider_Returns_AspNet_Template()
    {
        var provider = new AspNetTemplateProvider();
        var template = provider.GetTemplate();
        Assert.IsType<AspNetTemplate>(template);
        Assert.Equal("aspnet", template.Name);
    }

    [Fact]
    public void Renders_Expected_File_Set()
    {
        var specs = new AspNetTemplate().Render(FakeContext()).ToList();
        var paths = specs.Select(s => s.Path.Value).ToList();

        Assert.Contains(paths, p => p.EndsWith("WebApi.slnx"));
        Assert.Contains(paths, p => p.EndsWith($"src{System.IO.Path.DirectorySeparatorChar}WebApi{System.IO.Path.DirectorySeparatorChar}WebApi.csproj"));
        Assert.Contains(paths, p => p.EndsWith($"src{System.IO.Path.DirectorySeparatorChar}WebApi{System.IO.Path.DirectorySeparatorChar}Program.cs"));
        Assert.Contains(paths, p => p.EndsWith($"build{System.IO.Path.DirectorySeparatorChar}Build.cs"));
        Assert.Contains(paths, p => p.EndsWith($"build{System.IO.Path.DirectorySeparatorChar}Build.csproj"));
        Assert.Contains(paths, p => p.EndsWith($".config{System.IO.Path.DirectorySeparatorChar}dotnet-tools.json"));
        Assert.Contains(paths, p => p.EndsWith("tamp.sh"));
        Assert.Contains(paths, p => p.EndsWith("tamp.cmd"));
    }

    [Fact]
    public void Build_Cs_Includes_Publish_Target_For_The_Api_Project()
    {
        var rendered = AspNetTemplate.RenderBuildCs("WebApi");
        Assert.Contains("Target Publish => _ => _", rendered);
        Assert.Contains("DotNet.Publish", rendered);
        Assert.Contains("WebApi.csproj", rendered);
        // Inherits the minimal-template surface
        Assert.Contains("Target Clean => _ => _.Executes(() => CleanArtifacts());", rendered);
        Assert.Contains(".Default()", rendered);
    }

    [Fact]
    public void Api_Csproj_Uses_The_Web_Sdk()
    {
        var rendered = AspNetTemplate.RenderApiCsproj();
        Assert.Contains("Microsoft.NET.Sdk.Web", rendered);
        Assert.Contains("net10.0", rendered);
    }

    [Fact]
    public void Program_Cs_Wires_A_Minimal_Api_Endpoint_And_A_Health_Probe()
    {
        var rendered = AspNetTemplate.RenderApiProgram();
        Assert.Contains("WebApplication.CreateBuilder", rendered);
        Assert.Contains("app.MapGet(\"/health/live\"", rendered);
        Assert.Contains("app.Run();", rendered);
    }

    [Fact]
    public void Tamp_Sh_Is_Emitted_Executable()
    {
        var specs = new AspNetTemplate().Render(FakeContext()).ToList();
        var sh = specs.Single(s => s.Path.Value.EndsWith("tamp.sh"));
        Assert.True(sh.Executable);
        Assert.Equal(WriteMode.SkipIfExists, sh.Mode);
    }

    [Fact]
    public void Build_Csproj_Pins_Tamp_To_Context_Version()
    {
        var rendered = AspNetTemplate.RenderBuildCsproj(FakeContext());
        Assert.Contains("<PackageReference Include=\"Tamp.Core\" Version=\"1.4.0\" />", rendered);
        Assert.Contains("<PackageReference Include=\"Tamp.NetCli.V10\" Version=\"1.4.0\" />", rendered);
    }
}
