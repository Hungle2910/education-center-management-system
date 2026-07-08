using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EducationCenter.Crm.Application.Common.Interfaces;
using EducationCenter.Crm.Application.Schedules;
using EducationCenter.Crm.Domain.Classes;
using EducationCenter.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EducationCenter.Crm.Infrastructure.Schedules;

public sealed class ScheduleService : IScheduleService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IGoogleCalendarService _googleCalendarService;

    public ScheduleService(
        ApplicationDbContext dbContext,
        IGoogleCalendarService? googleCalendarService = null)
    {
        _dbContext = dbContext;
        _googleCalendarService = googleCalendarService ?? new NoOpGoogleCalendarService();
    }

    public async Task<ScheduleResponse> CreateScheduleAsync(CreateScheduleRequest request, CancellationToken cancellationToken)
    {
        if (request.StartTime >= request.EndTime)
        {
            throw new ArgumentException("Thời gian bắt đầu phải trước thời gian kết thúc.");
        }

        // 1. Check for conflicts in ClassSchedules (Weekly repeating schedule)
        var hasWeeklyConflict = await _dbContext.ClassSchedules
            .AnyAsync(cs => cs.DayOfWeek == request.DayOfWeek &&
                            cs.StartTime < request.EndTime &&
                            request.StartTime < cs.EndTime &&
                            (cs.RoomId == request.RoomId || (request.TeacherId != null && cs.Class != null && cs.Class.TeacherId == request.TeacherId)),
                            cancellationToken);

        if (hasWeeklyConflict)
        {
            throw new InvalidOperationException("Không thể lưu lịch. Lớp này đang trùng phòng hoặc trùng giáo viên. Vui lòng chọn thời gian khác.");
        }

        // 2. Fetch dependencies
        var cls = await _dbContext.Classes
            .Include(c => c.Teacher)
            .FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);
            
        if (cls == null)
        {
            throw new KeyNotFoundException("Không tìm thấy lớp học.");
        }

        var room = await _dbContext.Rooms.FindAsync(new object[] { request.RoomId }, cancellationToken);
        if (room == null)
        {
            throw new KeyNotFoundException("Không tìm thấy phòng học.");
        }

        // 3. Save weekly schedule
        var classSchedule = new ClassSchedule
        {
            Id = Guid.NewGuid(),
            ClassId = request.ClassId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            RoomId = request.RoomId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.ClassSchedules.Add(classSchedule);

        // 4. Generate actual occurrences for the next 4 weeks
        var occurrences = new List<ScheduleOccurrence>();
        var today = DateOnly.FromDateTime(DateTime.Today);
        
        for (int i = 0; i < 4; i++)
        {
            // Find next date matching the DayOfWeek
            var daysToAdd = ((int)request.DayOfWeek - (int)today.DayOfWeek + 7) % 7;
            if (daysToAdd == 0 && i == 0) // if today is the day, schedule today
            {
                daysToAdd = 0;
            }
            else if (daysToAdd == 0)
            {
                daysToAdd = 7;
            }
            var occurrenceDate = today.AddDays(daysToAdd + (i * 7));

            occurrences.Add(new ScheduleOccurrence
            {
                Id = Guid.NewGuid(),
                ClassScheduleId = classSchedule.Id,
                ClassId = request.ClassId,
                Date = occurrenceDate,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                RoomId = request.RoomId,
                TeacherId = request.TeacherId ?? cls.TeacherId,
                Status = "Đã lên lịch",
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        foreach (var occurrence in occurrences)
        {
            var startDateTime = occurrence.Date.ToDateTime(occurrence.StartTime);
            var endDateTime = occurrence.Date.ToDateTime(occurrence.EndTime);
            var summary = $"Lớp {cls.Name} - {room.Name}";
            var teacherName = cls.Teacher?.FullName ?? "Chưa phân công";
            var description = $"Giáo viên: {teacherName}\nMôn học: {cls.Subject ?? "Chưa rõ"}\nPhòng: {room.Name}\nCRM ID: {occurrence.Id}";

            var gEventId = await _googleCalendarService.CreateEventAsync(
                summary,
                room.Name,
                description,
                startDateTime,
                endDateTime,
                cancellationToken);

            occurrence.GoogleEventId = gEventId;
        }

        _dbContext.ScheduleOccurrences.AddRange(occurrences);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ScheduleResponse(
            classSchedule.Id,
            classSchedule.ClassId,
            cls.Name,
            classSchedule.DayOfWeek,
            classSchedule.StartTime,
            classSchedule.EndTime,
            classSchedule.RoomId,
            room.Name,
            cls.TeacherId,
            cls.Teacher?.FullName);
    }

    public async Task<IReadOnlyCollection<ScheduleOccurrenceResponse>> GetCalendarAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
    {
        var occurrences = await _dbContext.ScheduleOccurrences
            .Include(so => so.Class)
            .Include(so => so.Room)
            .Include(so => so.Teacher)
            .Where(so => so.Date >= startDate && so.Date <= endDate)
            .OrderBy(so => so.Date)
            .ThenBy(so => so.StartTime)
            .ToListAsync(cancellationToken);

        return occurrences.Select(so => new ScheduleOccurrenceResponse(
            so.Id,
            so.ClassId,
            so.Class?.Name ?? "Lớp học",
            so.Date,
            so.StartTime,
            so.EndTime,
            so.RoomId,
            so.Room?.Name ?? "Phòng học",
            so.TeacherId,
            so.Teacher?.FullName,
            so.Status,
            so.Reason)).ToList();
    }

    public async Task<ConflictCheckResponse> CheckConflictsAsync(ConflictCheckRequest request, CancellationToken cancellationToken)
    {
        var query = _dbContext.ScheduleOccurrences
            .Where(so => so.Date == request.Date &&
                         so.StartTime < request.EndTime &&
                         request.StartTime < so.EndTime);

        if (request.ExcludeOccurrenceId.HasValue)
        {
            query = query.Where(so => so.Id != request.ExcludeOccurrenceId.Value);
        }

        var conflictingOccurrence = await query
            .Include(so => so.Class)
            .Include(so => so.Room)
            .Include(so => so.Teacher)
            .FirstOrDefaultAsync(so => so.RoomId == request.RoomId || (request.TeacherId != null && so.TeacherId == request.TeacherId), cancellationToken);

        if (conflictingOccurrence != null)
        {
            var isRoomConflict = conflictingOccurrence.RoomId == request.RoomId;
            var resourceName = isRoomConflict ? $"Phòng {conflictingOccurrence.Room?.Name}" : $"Giáo viên {conflictingOccurrence.Teacher?.FullName}";
            return new ConflictCheckResponse(
                true,
                $"Không thể lưu lịch. {resourceName} đã có lịch dạy lớp {conflictingOccurrence.Class?.Name} vào thời gian này.");
        }

        return new ConflictCheckResponse(false, null);
    }

    public async Task CancelOccurrenceAsync(Guid occurrenceId, CancelSessionRequest request, CancellationToken cancellationToken)
    {
        var occurrence = await _dbContext.ScheduleOccurrences
            .FirstOrDefaultAsync(o => o.Id == occurrenceId, cancellationToken);

        if (occurrence == null)
        {
            throw new KeyNotFoundException("Không tìm thấy buổi học thực tế.");
        }

        occurrence.Status = "Đã hủy";
        occurrence.UpdatedAtUtc = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(occurrence.GoogleEventId))
        {
            await _googleCalendarService.DeleteEventAsync(occurrence.GoogleEventId, cancellationToken);
            occurrence.GoogleEventId = null;
        }

        // If they choose "Trừ học phí", we can log this or handle it during invoice generation later.
        // For this ticket, marking the session status as "Đã hủy" is the core state transition.

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RegisterIndividualMakeupAsync(ScheduleIndividualMakeupRequest request, CancellationToken cancellationToken)
    {
        var absentOccurrence = await _dbContext.ScheduleOccurrences
            .AnyAsync(o => o.Id == request.AbsentOccurrenceId, cancellationToken);

        var makeupOccurrence = await _dbContext.ScheduleOccurrences
            .AnyAsync(o => o.Id == request.MakeupOccurrenceId, cancellationToken);

        if (!absentOccurrence || !makeupOccurrence)
        {
            throw new KeyNotFoundException("Không tìm thấy buổi học thực tế (buổi nghỉ hoặc buổi bù).");
        }

        // Verify student is absent in the absent occurrence
        var attendance = await _dbContext.Attendances
            .FirstOrDefaultAsync(a => a.OccurrenceId == request.AbsentOccurrenceId && a.StudentId == request.StudentId, cancellationToken);

        if (attendance == null || (attendance.Status != "Vắng có phép" && attendance.Status != "Chờ học bù"))
        {
            throw new InvalidOperationException("Học sinh không ở trạng thái vắng có phép trong buổi học này.");
        }

        // Create IndividualMakeup entry
        var makeup = new IndividualMakeup
        {
            Id = Guid.NewGuid(),
            StudentId = request.StudentId,
            AbsentOccurrenceId = request.AbsentOccurrenceId,
            MakeupOccurrenceId = request.MakeupOccurrenceId,
            CreatedAtUtc = DateTime.UtcNow
        };
        _dbContext.IndividualMakeups.Add(makeup);

        // Update attendance status to Đã học bù
        attendance.Status = "Đã học bù";
        attendance.LastModifiedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<EligibleAbsentStudentDto>> GetEligibleAbsentStudentsAsync(Guid occurrenceId, CancellationToken cancellationToken)
    {
        // Get all students who were absent (Vắng có phép) in this occurrence and haven't registered a makeup session yet
        var alreadyMadeUp = await _dbContext.IndividualMakeups
            .Where(im => im.AbsentOccurrenceId == occurrenceId)
            .Select(im => im.StudentId)
            .ToListAsync(cancellationToken);

        var eligibleAttendances = await _dbContext.Attendances
            .Include(a => a.Student)
            .Where(a => a.OccurrenceId == occurrenceId && 
                        (a.Status == "Vắng có phép" || a.Status == "Chờ học bù") &&
                        !alreadyMadeUp.Contains(a.StudentId))
            .ToListAsync(cancellationToken);

        return eligibleAttendances
            .Select(a => new EligibleAbsentStudentDto(
                a.StudentId,
                a.Student?.FullName ?? "Học sinh",
                a.OccurrenceId,
                a.Notes
            ))
            .ToArray();
    }

    private sealed class NoOpGoogleCalendarService : IGoogleCalendarService
    {
        public Task<string?> CreateEventAsync(
            string summary,
            string location,
            string description,
            DateTime startDateTime,
            DateTime endDateTime,
            CancellationToken cancellationToken) => Task.FromResult<string?>(null);

        public Task UpdateEventAsync(
            string eventId,
            string summary,
            string location,
            string description,
            DateTime startDateTime,
            DateTime endDateTime,
            CancellationToken cancellationToken) => Task.CompletedTask;

        public Task DeleteEventAsync(
            string eventId,
            CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
