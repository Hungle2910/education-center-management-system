using EducationCenter.Crm.Application.CoreData;
using EducationCenter.Crm.Domain.Classes;
using EducationCenter.Crm.Domain.People;
using EducationCenter.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace EducationCenter.Crm.Infrastructure.CoreData;

public sealed class ClassService : IClassService
{
    private readonly ApplicationDbContext _dbContext;

    public ClassService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<ClassResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Classes
            .Include(classRoom => classRoom.Teacher)
            .AsNoTracking()
            .OrderBy(classRoom => classRoom.Name)
            .Select(classRoom => ToResponse(classRoom))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<ClassResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await FindResponseAsync(id, cancellationToken);
    }

    public async Task<ClassResponse> CreateAsync(ClassRequest request, CancellationToken cancellationToken)
    {
        var classRoom = new Class
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Subject = NormalizeOptionalText(request.Subject),
            TeacherId = request.TeacherId,
            Status = NormalizeOptionalText(request.Status) ?? ClassStatuses.Upcoming,
            MinStudents = request.MinStudents,
            MaxStudents = request.MaxStudents,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Classes.Add(classRoom);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return (await FindResponseAsync(classRoom.Id, cancellationToken))!;
    }

    public async Task<ClassResponse?> UpdateAsync(
        Guid id,
        ClassRequest request,
        CancellationToken cancellationToken)
    {
        var classRoom = await _dbContext.Classes
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (classRoom is null)
        {
            return null;
        }

        classRoom.Name = request.Name.Trim();
        classRoom.Subject = NormalizeOptionalText(request.Subject);
        classRoom.TeacherId = request.TeacherId;
        classRoom.Status = NormalizeOptionalText(request.Status) ?? ClassStatuses.Upcoming;
        classRoom.MinStudents = request.MinStudents;
        classRoom.MaxStudents = request.MaxStudents;
        classRoom.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return (await FindResponseAsync(id, cancellationToken))!;
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var classRoom = await _dbContext.Classes.FindAsync(new object[] { id }, cancellationToken);
        if (classRoom is null)
        {
            return (false, "Không tìm thấy lớp học.");
        }

        // Kiểm tra lịch học
        var hasSchedules = await _dbContext.ClassSchedules
            .AnyAsync(cs => cs.ClassId == id, cancellationToken);
        if (hasSchedules)
        {
            return (false, "Không thể xóa lớp học này vì đã được xếp lịch học.");
        }

        // Kiểm tra hóa đơn học phí
        var hasInvoices = await _dbContext.TuitionInvoices
            .AnyAsync(ti => ti.ClassId == id, cancellationToken);
        if (hasInvoices)
        {
            return (false, "Không thể xóa lớp học này vì đã có hóa đơn học phí phát sinh.");
        }

        // Kiểm tra buổi học thử
        var hasTrials = await _dbContext.TrialSessions
            .AnyAsync(ts => ts.ClassId == id, cancellationToken);
        if (hasTrials)
        {
            return (false, "Không thể xóa lớp học này vì đã có học sinh đăng ký học thử.");
        }

        _dbContext.Classes.Remove(classRoom);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    private async Task<ClassResponse?> FindResponseAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Classes
            .Include(classRoom => classRoom.Teacher)
            .AsNoTracking()
            .Where(classRoom => classRoom.Id == id)
            .Select(classRoom => ToResponse(classRoom))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static ClassResponse ToResponse(Class classRoom)
    {
        return new ClassResponse(
            classRoom.Id,
            classRoom.Name,
            classRoom.Subject,
            classRoom.TeacherId,
            classRoom.Teacher?.FullName,
            classRoom.Status,
            classRoom.MinStudents,
            classRoom.MaxStudents);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
