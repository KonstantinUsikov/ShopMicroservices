using System.Reflection;

namespace Basket.API;

public class DependencyContextAssemblyCatalogCustom : DependencyContextAssemblyCatalog
{
    public override IReadOnlyCollection<Assembly> GetAssemblies()
    {
        return new List<Assembly> { typeof(Program).Assembly };
    }
}