using System.Reflection;
using EducationCenter.Crm.Api.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace EducationCenter.Crm.Tests;

public sealed class DashboardControllerAuthorizationTests
{
    [Fact]
    public void DashboardController_AllowsOnlyAdminAndStaffRoles()
    {
        var authorize = typeof(DashboardController)
            .GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorize);
        Assert.Equal("Admin,Staff", authorize.Roles);
    }
}
