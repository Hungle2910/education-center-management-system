using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EducationCenter.Crm.Application.Attendance;
using EducationCenter.Crm.Domain.Classes;
using EducationCenter.Crm.Domain.People;
using EducationCenter.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

using EducationCenter.Crm.Application.Common.Interfaces;

namespace EducationCenter.Crm.Infrastructure.Attendance;

public sealed class AttendanceService : IAttendanceService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IAuditLogService _auditLogService;

    public AttendanceService(ApplicationDbContext dbContext, IAuditLogService auditLogService)
    {
        _dbContext = dbContext;
        _auditLogService = auditLogService;
    }

    public async Task<OccurrenceAttendanceDto?> GetAttendanceByOccurrenceAsync(
        Guid occurrenceId, 
        CancellationToken cancellationToken)
    {
        var occurrence = await _dbContext.ScheduleOccurrences
            .Include(o => o.Class)
            .FirstOrDefaultAsync(o => o.Id == occurrenceId, cancellationToken);

        if (occurrence is null)
        {
            return null;
        }

        // Fetch all active students
        var students = await _dbContext.Students
            .Where(s => s.Status == StudentStatuses.Active)
            .OrderBy(s => s.FullName)
            .ToListAsync(cancellationToken);

        // Fetch existing attendance records
        var existingAttendances = await _dbContext.Attendances
            .Where(a => a.OccurrenceId == occurrenceId)
            .ToDictionaryAsync(a => a.StudentId, cancellationToken);

        var studentAttendanceList = new List<StudentAttendanceDto>();
        foreach (var student in students)
        {
            if (existingAttendances.TryGetValue(student.Id, out var att))
            {
                studentAttendanceList.Add(new StudentAttendanceDto(
                    student.Id,
                    student.FullName,
                    att.Status,
                    att.Notes
                ));
            }
            else
            {
                studentAttendanceList.Add(new StudentAttendanceDto(
                    student.Id,
                    student.FullName,
                    "Có mặt", // Default status
                    null
                ));
            }
        }

        return new OccurrenceAttendanceDto(
            occurrence.Id,
            occurrence.Class?.Name ?? "Lớp học",
            occurrence.Date.ToString("yyyy-MM-dd"),
            occurrence.StartTime.ToString(@"hh\:mm"),
            occurrence.EndTime.ToString(@"hh\:mm"),
            occurrence.Status,
            studentAttendanceList
        );
    }

    public async Task SubmitAttendanceAsync(
        SubmitAttendanceRequest request, 
        string auditor, 
        CancellationToken cancellationToken)
    {
        var occurrence = await _dbContext.ScheduleOccurrences
            .FirstOrDefaultAsync(o => o.Id == request.OccurrenceId, cancellationToken);

        if (occurrence is null)
        {
            throw new KeyNotFoundException("Không tìm thấy buổi học thực tế.");
        }

        var existingAttendances = await _dbContext.Attendances
            .Where(a => a.OccurrenceId == request.OccurrenceId)
            .ToDictionaryAsync(a => a.StudentId, cancellationToken);

        foreach (var studentDto in request.Students)
        {
            if (existingAttendances.TryGetValue(studentDto.StudentId, out var existing))
            {
                existing.Status = studentDto.Status;
                existing.Notes = studentDto.Notes;
                existing.LastModifiedAtUtc = DateTime.UtcNow;
                existing.AuditedBy = auditor;
            }
            else
            {
                var newAttendance = new Domain.Classes.Attendance
                {
                    Id = Guid.NewGuid(),
                    OccurrenceId = request.OccurrenceId,
                    StudentId = studentDto.StudentId,
                    Status = studentDto.Status,
                    Notes = studentDto.Notes,
                    CreatedAtUtc = DateTime.UtcNow,
                    AuditedBy = auditor
                };
                _dbContext.Attendances.Add(newAttendance);
            }
        }

        // Update occurrence status to Đã học
        occurrence.Status = "Đã học";
        occurrence.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            null, null, null,
            "Điểm danh lớp học",
            "ScheduleOccurrence",
            occurrence.Id.ToString(),
            $"Điểm danh buổi học ngày {occurrence.Date}. Người điểm danh: {auditor}.",
            null,
            cancellationToken);
    }
}
