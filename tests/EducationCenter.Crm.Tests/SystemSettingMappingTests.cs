using EducationCenter.Crm.Domain.Settings;
using EducationCenter.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EducationCenter.Crm.Tests;

public sealed class SystemSettingMappingTests
{
    [Fact]
    public void SystemSetting_ShouldHaveExpectedMapping()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("system-setting-mapping")
            .Options;

        using var dbContext = new ApplicationDbContext(options);
        var entityType = dbContext.Model.FindEntityType(typeof(SystemSetting));

        Assert.NotNull(entityType);
        Assert.Equal("SystemSettings", entityType.GetTableName());

        var primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Equal("Key", Assert.Single(primaryKey.Properties).Name);

        var keyProperty = entityType.FindProperty(nameof(SystemSetting.Key));
        Assert.NotNull(keyProperty);
        Assert.False(keyProperty.IsNullable);
        Assert.Equal(150, keyProperty.GetMaxLength());

        var valueProperty = entityType.FindProperty(nameof(SystemSetting.Value));
        Assert.NotNull(valueProperty);
        Assert.False(valueProperty.IsNullable);
        Assert.Equal(1000, valueProperty.GetMaxLength());

        var isSensitiveProperty = entityType.FindProperty(nameof(SystemSetting.IsSensitive));
        Assert.NotNull(isSensitiveProperty);
        Assert.False(isSensitiveProperty.IsNullable);
        Assert.Equal(false, isSensitiveProperty.GetDefaultValue());
    }
}
