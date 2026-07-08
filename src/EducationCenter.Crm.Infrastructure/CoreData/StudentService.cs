using EducationCenter.Crm.Application.Common.PhoneNumbers;
using EducationCenter.Crm.Application.CoreData;
using EducationCenter.Crm.Domain.Classes;
using EducationCenter.Crm.Domain.People;
using EducationCenter.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace EducationCenter.Crm.Infrastructure.CoreData;

public sealed class StudentService : IStudentService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPhoneNumberNormalizer _phoneNumberNormalizer;

    public StudentService(
        ApplicationDbContext dbContext,
        IPhoneNumberNormalizer phoneNumberNormalizer)
    {
        _dbContext = dbContext;
        _phoneNumberNormalizer = phoneNumberNormalizer;
    }

    public async Task<IReadOnlyCollection<StudentResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Students
            .Include(student => student.StudentParents)
                .ThenInclude(studentParent => studentParent.Parent)
            .AsNoTracking()
            .OrderBy(student => student.FullName)
            .Select(student => ToResponse(student))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<StudentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await FindResponseAsync(id, cancellationToken);
    }

    public async Task<StudentResponse> CreateAsync(StudentRequest request, CancellationToken cancellationToken)
    {
        var student = new Student
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            StudentCode = NormalizeOptionalText(request.StudentCode),
            Email = NormalizeOptionalText(request.Email),
            PhoneNumber = NormalizeOptionalPhoneNumber(request.PhoneNumber),
            DateOfBirth = request.DateOfBirth,
            Status = NormalizeOptionalText(request.Status) ?? StudentStatuses.Active,
            CreatedAtUtc = DateTime.UtcNow
        };

        AddParentLinks(student, request.Parents);

        _dbContext.Students.Add(student);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return (await FindResponseAsync(student.Id, cancellationToken))!;
    }

    public async Task<StudentResponse?> UpdateAsync(
        Guid id,
        StudentRequest request,
        CancellationToken cancellationToken)
    {
        var student = await _dbContext.Students
            .Include(item => item.StudentParents)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (student is null)
        {
            return null;
        }

        student.FullName = request.FullName.Trim();
        student.StudentCode = NormalizeOptionalText(request.StudentCode);
        student.Email = NormalizeOptionalText(request.Email);
        student.PhoneNumber = NormalizeOptionalPhoneNumber(request.PhoneNumber);
        student.DateOfBirth = request.DateOfBirth;
        student.Status = NormalizeOptionalText(request.Status) ?? StudentStatuses.Active;
        student.UpdatedAtUtc = DateTime.UtcNow;

        student.StudentParents.Clear();
        AddParentLinks(student, request.Parents);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return (await FindResponseAsync(id, cancellationToken))!;
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var student = await _dbContext.Students
            .Include(s => s.StudentParents)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (student is null)
        {
            return (false, "Không tìm thấy học sinh.");
        }

        // Kiểm tra điểm danh
        var hasAttendance = await _dbContext.Attendances
            .AnyAsync(a => a.StudentId == id, cancellationToken);
        if (hasAttendance)
        {
            return (false, "Không thể xóa học sinh này vì đã có dữ liệu điểm danh.");
        }

        // Kiểm tra học phí
        var hasInvoices = await _dbContext.TuitionInvoices
            .AnyAsync(ti => ti.StudentId == id, cancellationToken);
        if (hasInvoices)
        {
            return (false, "Không thể xóa học sinh này vì đã có dữ liệu hóa đơn học phí.");
        }

        // Kiểm tra học bù
        var hasMakeups = await _dbContext.IndividualMakeups
            .AnyAsync(im => im.StudentId == id, cancellationToken);
        if (hasMakeups)
        {
            return (false, "Không thể xóa học sinh này vì đã có lịch học bù.");
        }

        // Xóa các liên kết phụ huynh trước
        _dbContext.StudentParents.RemoveRange(student.StudentParents);
        _dbContext.Students.Remove(student);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    private async Task<StudentResponse?> FindResponseAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Students
            .Include(student => student.StudentParents)
                .ThenInclude(studentParent => studentParent.Parent)
            .AsNoTracking()
            .Where(student => student.Id == id)
            .Select(student => ToResponse(student))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private void AddParentLinks(Student student, IReadOnlyCollection<ParentLinkRequest>? parents)
    {
        if (parents is null)
        {
            return;
        }

        foreach (var parent in parents.DistinctBy(item => item.ParentId))
        {
            student.StudentParents.Add(new StudentParent
            {
                StudentId = student.Id,
                ParentId = parent.ParentId,
                Relationship = NormalizeOptionalText(parent.Relationship),
                CreatedAtUtc = DateTime.UtcNow
            });
        }
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

    private static StudentResponse ToResponse(Student student)
    {
        return new StudentResponse(
            student.Id,
            student.FullName,
            student.StudentCode,
            student.Email,
            student.PhoneNumber,
            student.DateOfBirth,
            student.Status,
            student.StudentParents
                .OrderBy(studentParent => studentParent.Parent.FullName)
                .Select(studentParent => new ParentSummaryResponse(
                    studentParent.Parent.Id,
                    studentParent.Parent.FullName,
                    studentParent.Parent.Email,
                    studentParent.Parent.PhoneNumber,
                    studentParent.Parent.ZaloLink,
                    studentParent.Relationship))
                .ToArray());
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
