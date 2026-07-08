using EducationCenter.Crm.Application.Common.PhoneNumbers;
using EducationCenter.Crm.Application.CoreData;
using EducationCenter.Crm.Domain.Classes;
using EducationCenter.Crm.Domain.People;
using EducationCenter.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace EducationCenter.Crm.Infrastructure.CoreData;

public sealed class TeacherService : ITeacherService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPhoneNumberNormalizer _phoneNumberNormalizer;

    public TeacherService(
        ApplicationDbContext dbContext,
        IPhoneNumberNormalizer phoneNumberNormalizer)
    {
        _dbContext = dbContext;
        _phoneNumberNormalizer = phoneNumberNormalizer;
    }

    public async Task<IReadOnlyCollection<TeacherResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Teachers
            .AsNoTracking()
            .OrderBy(teacher => teacher.FullName)
            .Select(teacher => ToResponse(teacher))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<TeacherResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var teacher = await _dbContext.Teachers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return teacher is null ? null : ToResponse(teacher);
    }

    public async Task<TeacherResponse> CreateAsync(TeacherRequest request, CancellationToken cancellationToken)
    {
        var teacher = new Teacher
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = NormalizeOptionalText(request.Email),
            PhoneNumber = NormalizeOptionalPhoneNumber(request.PhoneNumber),
            Subject = NormalizeOptionalText(request.Subject),
            IsActive = request.IsActive,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Teachers.Add(teacher);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(teacher);
    }

    public async Task<TeacherResponse?> UpdateAsync(
        Guid id,
        TeacherRequest request,
        CancellationToken cancellationToken)
    {
        var teacher = await _dbContext.Teachers
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (teacher is null)
        {
            return null;
        }

        teacher.FullName = request.FullName.Trim();
        teacher.Email = NormalizeOptionalText(request.Email);
        teacher.PhoneNumber = NormalizeOptionalPhoneNumber(request.PhoneNumber);
        teacher.Subject = NormalizeOptionalText(request.Subject);
        teacher.IsActive = request.IsActive;
        teacher.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(teacher);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var teacher = await _dbContext.Teachers.FindAsync(new object[] { id }, cancellationToken);
        if (teacher is null)
        {
            return (false, "Không tìm thấy giáo viên.");
        }

        // Kiểm tra giáo viên có đang được gán lớp học không
        var hasClasses = await _dbContext.Classes
            .AnyAsync(c => c.TeacherId == id, cancellationToken);
        if (hasClasses)
        {
            return (false, "Không thể xóa giáo viên này vì đang được phân công lớp học.");
        }

        _dbContext.Teachers.Remove(teacher);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    private string? NormalizeOptionalPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return null;
        }

        _phoneNumberNormalizer.TryNormalize(phoneNumber, out var normalizedPhoneNumber);
        return normalizedPhoneNumber;
    }

    private static TeacherResponse ToResponse(Teacher teacher)
    {
        return new TeacherResponse(
            teacher.Id,
            teacher.FullName,
            teacher.Email,
            teacher.PhoneNumber,
            teacher.Subject,
            teacher.IsActive);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
