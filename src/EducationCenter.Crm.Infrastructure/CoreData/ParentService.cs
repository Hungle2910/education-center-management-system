using EducationCenter.Crm.Application.Common.PhoneNumbers;
using EducationCenter.Crm.Application.CoreData;
using EducationCenter.Crm.Domain.People;
using EducationCenter.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EducationCenter.Crm.Infrastructure.CoreData;

public sealed class ParentService : IParentService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPhoneNumberNormalizer _phoneNumberNormalizer;

    public ParentService(
        ApplicationDbContext dbContext,
        IPhoneNumberNormalizer phoneNumberNormalizer)
    {
        _dbContext = dbContext;
        _phoneNumberNormalizer = phoneNumberNormalizer;
    }

    public async Task<IReadOnlyCollection<ParentResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Parents
            .Include(parent => parent.StudentParents)
                .ThenInclude(studentParent => studentParent.Student)
            .AsNoTracking()
            .OrderBy(parent => parent.FullName)
            .Select(parent => ToResponse(parent))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<ParentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await FindResponseAsync(id, cancellationToken);
    }

    public async Task<ParentResponse> CreateAsync(ParentRequest request, CancellationToken cancellationToken)
    {
        var phoneNumber = NormalizePhoneNumber(request.PhoneNumber);
        var parent = new Parent
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = NormalizeOptionalText(request.Email),
            PhoneNumber = phoneNumber,
            ZaloLink = CreateZaloLink(phoneNumber),
            CreatedAtUtc = DateTime.UtcNow
        };

        AddStudentLinks(parent, request.Students);

        _dbContext.Parents.Add(parent);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return (await FindResponseAsync(parent.Id, cancellationToken))!;
    }

    public async Task<ParentResponse?> UpdateAsync(
        Guid id,
        ParentRequest request,
        CancellationToken cancellationToken)
    {
        var parent = await _dbContext.Parents
            .Include(item => item.StudentParents)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (parent is null)
        {
            return null;
        }

        var phoneNumber = NormalizePhoneNumber(request.PhoneNumber);
        parent.FullName = request.FullName.Trim();
        parent.Email = NormalizeOptionalText(request.Email);
        parent.PhoneNumber = phoneNumber;
        parent.ZaloLink = CreateZaloLink(phoneNumber);
        parent.UpdatedAtUtc = DateTime.UtcNow;

        parent.StudentParents.Clear();
        AddStudentLinks(parent, request.Students);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return (await FindResponseAsync(id, cancellationToken))!;
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var parent = await _dbContext.Parents
            .Include(p => p.StudentParents)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (parent is null)
        {
            return (false, "Không tìm thấy phụ huynh.");
        }

        // Kiểm tra xem phụ huynh có nhật ký chăm sóc không
        var hasCareLogs = await _dbContext.ParentCareLogs
            .AnyAsync(l => l.ParentId == id, cancellationToken);
        if (hasCareLogs)
        {
            return (false, "Không thể xóa phụ huynh này vì có lịch sử chăm sóc khách hàng.");
        }

        // Xóa liên kết học sinh - phụ huynh trước
        _dbContext.StudentParents.RemoveRange(parent.StudentParents);
        _dbContext.Parents.Remove(parent);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    private async Task<ParentResponse?> FindResponseAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Parents
            .Include(parent => parent.StudentParents)
                .ThenInclude(studentParent => studentParent.Student)
            .AsNoTracking()
            .Where(parent => parent.Id == id)
            .Select(parent => ToResponse(parent))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private void AddStudentLinks(Parent parent, IReadOnlyCollection<StudentLinkRequest>? students)
    {
        if (students is null)
        {
            return;
        }

        foreach (var student in students.DistinctBy(item => item.StudentId))
        {
            parent.StudentParents.Add(new StudentParent
            {
                ParentId = parent.Id,
                StudentId = student.StudentId,
                Relationship = NormalizeOptionalText(student.Relationship),
                CreatedAtUtc = DateTime.UtcNow
            });
        }
    }

    private string NormalizePhoneNumber(string phoneNumber)
    {
        _phoneNumberNormalizer.TryNormalize(phoneNumber, out var normalizedPhoneNumber);
        return normalizedPhoneNumber;
    }

    private static ParentResponse ToResponse(Parent parent)
    {
        return new ParentResponse(
            parent.Id,
            parent.FullName,
            parent.Email,
            parent.PhoneNumber,
            parent.ZaloLink,
            parent.StudentParents
                .OrderBy(studentParent => studentParent.Student.FullName)
                .Select(studentParent => new StudentSummaryResponse(
                    studentParent.Student.Id,
                    studentParent.Student.FullName,
                    studentParent.Student.StudentCode,
                    studentParent.Student.PhoneNumber,
                    studentParent.Relationship))
                .ToArray());
    }

    private static string CreateZaloLink(string phoneNumber)
    {
        return $"https://zalo.me/{phoneNumber}";
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
