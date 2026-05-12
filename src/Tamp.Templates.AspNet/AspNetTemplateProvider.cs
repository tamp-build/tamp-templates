using Tamp.Scaffold;

namespace Tamp.Templates.AspNet;

/// <summary>
/// Entry point the Tamp CLI's NuGet template source (v0.2.0+) discovers
/// via reflection when the user runs <c>tamp init --template aspnet</c>.
/// </summary>
public sealed class AspNetTemplateProvider : IScaffoldTemplateProvider
{
    public IScaffoldTemplate GetTemplate() => new AspNetTemplate();
}
