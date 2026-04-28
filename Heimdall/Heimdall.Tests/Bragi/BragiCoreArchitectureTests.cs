using Heimdall.BragiCore.Extraction;

namespace Heimdall.Tests.Bragi;

public sealed class BragiCoreArchitectureTests
{
    [Fact]
    public void BragiCore_DoesNotReferenceWinUI()
    {
        var referencedAssemblyNames = typeof(SubjectExtractionService)
            .Assembly
            .GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .ToArray();

        Assert.DoesNotContain("Microsoft.UI.Xaml", referencedAssemblyNames);
        Assert.DoesNotContain("Microsoft.WinUI", referencedAssemblyNames);
        Assert.DoesNotContain("Microsoft.WindowsAppSDK", referencedAssemblyNames);
    }
}
