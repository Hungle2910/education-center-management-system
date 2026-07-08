using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EducationCenter.Crm.Application.Admissions;
using EducationCenter.Crm.Application.Common.Interfaces;
using EducationCenter.Crm.Application.Common.PhoneNumbers;
using EducationCenter.Crm.Domain.Classes;
using EducationCenter.Crm.Domain.People;
using EducationCenter.Crm.Infrastructure.Admissions;
using EducationCenter.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EducationCenter.Crm.Tests;

public sealed class AdmissionsServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbOptions;
    private readonly VietnamPhoneNumberNormalizer _normalizer = new();

    public AdmissionsServiceTests()
    {
        _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task ConvertLeadToStudentAsync_ShouldCreateStudentParentAndLinkCorrectly()
    {
        // Arrange
        await using var dbContext = new ApplicationDbContext(_dbOptions);
        var service = new AdmissionsService(dbContext, _normalizer, new FakeAuditLogService());

        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            StudentName = "Học sinh Tiềm Năng A",
            ParentName = "Phụ huynh A",
            ParentPhone = "0909123456",
            Email = "lead@test.local",
            Status = LeadStatuses.New,
            CreatedAtUtc = DateTime.UtcNow
        };
        dbContext.Leads.Add(lead);
        await dbContext.SaveChangesAsync();

        // Act
        await service.ConvertLeadToStudentAsync(lead.Id, CancellationToken.None);

        // Assert
        var updatedLead = await dbContext.Leads.FindAsync(lead.Id);
        Assert.NotNull(updatedLead);
        Assert.Equal(LeadStatuses.Registered, updatedLead.Status);

        // Check Parent creation & Phone normalization
        var parent = await dbContext.Parents.FirstOrDefaultAsync(p => p.PhoneNumber == "84909123456");
        Assert.NotNull(parent);
        Assert.Equal("Phụ huynh A", parent.FullName);

        // Check Student creation
        var student = await dbContext.Students.FirstOrDefaultAsync(s => s.FullName == "Học sinh Tiềm Năng A");
        Assert.NotNull(student);
        Assert.Equal(StudentStatuses.Active, student.Status);

        // Check StudentParent relationship
        var link = await dbContext.StudentParents.FirstOrDefaultAsync(
            sp => sp.StudentId == student.Id && sp.ParentId == parent.Id);
        Assert.NotNull(link);
        Assert.Equal("Phụ huynh", link.Relationship);
    }

    [Fact]
    public async Task ScheduleTrialSessionAsync_ShouldCreateSessionAndUpdateLeadStatus()
    {
        // Arrange
        await using var dbContext = new ApplicationDbContext(_dbOptions);
        var service = new AdmissionsService(dbContext, _normalizer, new FakeAuditLogService());

        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            StudentName = "Học sinh B",
            ParentPhone = "0909123456",
            Status = LeadStatuses.New,
            CreatedAtUtc = DateTime.UtcNow
        };
        dbContext.Leads.Add(lead);

        var classRoom = new Class
        {
            Id = Guid.NewGuid(),
            Name = "Lớp Toán 9A",
            MonthlyFee = 1000000,
            MinStudents = 5,
            MaxStudents = 15,
            Status = "Đang học",
            CreatedAtUtc = DateTime.UtcNow
        };
        dbContext.Classes.Add(classRoom);
        await dbContext.SaveChangesAsync();

        var request = new ScheduleTrialRequest(lead.Id, classRoom.Id, DateOnly.FromDateTime(DateTime.Today), null, "Ghi chu hoc thu");

        // Act
        var trialDto = await service.ScheduleTrialSessionAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(trialDto);
        Assert.Equal(lead.Id, trialDto.LeadId);
        Assert.Equal(classRoom.Id, trialDto.ClassId);

        var updatedLead = await dbContext.Leads.FindAsync(lead.Id);
        Assert.NotNull(updatedLead);
        Assert.Equal(LeadStatuses.TrialScheduled, updatedLead.Status);
    }

    private sealed class FakeAuditLogService : IAuditLogService
    {
        public Task LogAsync(Guid? userId, string? userEmail, string? userFullName, string action, string? entityType, string? entityId, string? details, string? ipAddress, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(string? searchTerm, string? action, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PagedResult<AuditLogDto>(Array.Empty<AuditLogDto>(), 0, pageNumber, pageSize));
        }
    }
}
