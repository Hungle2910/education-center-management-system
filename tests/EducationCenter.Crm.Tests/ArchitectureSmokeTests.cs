using EducationCenter.Crm.Domain;

namespace EducationCenter.Crm.Tests;

public sealed class ArchitectureSmokeTests
{
    [Fact]
    public void CleanArchitectureAssemblies_ShouldLoad()
    {
        var assemblyNames = new[]
        {
            typeof(AssemblyReference).Assembly.GetName().Name,
            typeof(Application.DependencyInjection).Assembly.GetName().Name,
            typeof(Infrastructure.DependencyInjection).Assembly.GetName().Name
        };

        Assert.Contains("EducationCenter.Crm.Domain", assemblyNames);
        Assert.Contains("EducationCenter.Crm.Application", assemblyNames);
        Assert.Contains("EducationCenter.Crm.Infrastructure", assemblyNames);
    }
}
