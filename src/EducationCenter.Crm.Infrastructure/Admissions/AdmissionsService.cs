using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EducationCenter.Crm.Application.Admissions;
using EducationCenter.Crm.Application.Common.PhoneNumbers;
using EducationCenter.Crm.Domain.People;
using EducationCenter.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

using EducationCenter.Crm.Application.Common.Interfaces;

namespace EducationCenter.Crm.Infrastructure.Admissions;

public sealed class AdmissionsService : IAdmissionsService
{
    private readonly ApplicationDbContext _db;
    private readonly IPhoneNumberNormalizer _phoneNumberNormalizer;
    private readonly IAuditLogService _auditLogService;

    public AdmissionsService(
        ApplicationDbContext db, 
        IPhoneNumberNormalizer phoneNumberNormalizer,
        IAuditLogService auditLogService)
    {
        _db = db;
        _phoneNumberNormalizer = phoneNumberNormalizer;
        _auditLogService = auditLogService;
    }

    public async Task<IReadOnlyCollection<LeadDto>> GetLeadsAsync(CancellationToken cancellationToken)
    {
        var leads = await _db.Leads
            .AsNoTracking()
            .OrderByDescending(l => l.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return leads.Select(l => new LeadDto(
            l.Id,
            l.StudentName,
            l.ParentName,
            l.ParentPhone,
            l.Email,
            l.Source,
            l.Status,
            l.Notes,
            l.CreatedAtUtc
        )).ToList();
    }

    public async Task<LeadDto> CreateLeadAsync(CreateLeadRequest request, CancellationToken cancellationToken)
    {
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            StudentName = request.StudentName.Trim(),
            ParentName = request.ParentName?.Trim(),
            ParentPhone = request.ParentPhone.Trim(),
            Email = request.Email?.Trim(),
            Source = request.Source?.Trim(),
            Status = LeadStatuses.New,
            Notes = request.Notes?.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Leads.Add(lead);
        await _db.SaveChangesAsync(cancellationToken);

        return new LeadDto(
            lead.Id,
            lead.StudentName,
            lead.ParentName,
            lead.ParentPhone,
            lead.Email,
            lead.Source,
            lead.Status,
            lead.Notes,
            lead.CreatedAtUtc
        );
    }

    public async Task<LeadDto> UpdateLeadAsync(Guid id, UpdateLeadRequest request, CancellationToken cancellationToken)
    {
        var lead = await _db.Leads.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy học sinh tiềm năng.");

        lead.StudentName = request.StudentName.Trim();
        lead.ParentName = request.ParentName?.Trim();
        lead.ParentPhone = request.ParentPhone.Trim();
        lead.Email = request.Email?.Trim();
        lead.Source = request.Source?.Trim();
        lead.Status = request.Status;
        lead.Notes = request.Notes?.Trim();

        await _db.SaveChangesAsync(cancellationToken);

        return new LeadDto(
            lead.Id,
            lead.StudentName,
            lead.ParentName,
            lead.ParentPhone,
            lead.Email,
            lead.Source,
            lead.Status,
            lead.Notes,
            lead.CreatedAtUtc
        );
    }

    public async Task ConvertLeadToStudentAsync(Guid id, CancellationToken cancellationToken)
    {
        var lead = await _db.Leads.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy học sinh tiềm năng.");

        // Normalize parent phone
        _phoneNumberNormalizer.TryNormalize(lead.ParentPhone, out var normalizedPhone);
        if (string.IsNullOrWhiteSpace(normalizedPhone))
        {
            normalizedPhone = lead.ParentPhone;
        }

        // Find or create Parent
        var parent = await _db.Parents
            .FirstOrDefaultAsync(p => p.PhoneNumber == normalizedPhone, cancellationToken);

        if (parent == null)
        {
            parent = new Parent
            {
                Id = Guid.NewGuid(),
                FullName = string.IsNullOrWhiteSpace(lead.ParentName) ? $"Phụ huynh {lead.StudentName}" : lead.ParentName.Trim(),
                PhoneNumber = normalizedPhone,
                Email = lead.Email?.Trim(),
                ZaloLink = $"https://zalo.me/{normalizedPhone}",
                CreatedAtUtc = DateTime.UtcNow
            };
            _db.Parents.Add(parent);
        }

        // Create Student
        var student = new Student
        {
            Id = Guid.NewGuid(),
            FullName = lead.StudentName.Trim(),
            Email = lead.Email?.Trim(),
            Status = StudentStatuses.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Students.Add(student);

        // Link Student & Parent
        var studentParent = new StudentParent
        {
            StudentId = student.Id,
            ParentId = parent.Id,
            Relationship = "Phụ huynh",
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.StudentParents.Add(studentParent);

        // Update Lead Status
        lead.Status = LeadStatuses.Registered;

        await _db.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            null, null, null,
            "Chuyển tiềm năng thành chính thức",
            "Lead",
            lead.Id.ToString(),
            $"Chuyển đổi học sinh tiềm năng '{lead.StudentName}' thành học sinh chính thức và liên kết với phụ huynh '{parent.FullName}' (SĐT: {parent.PhoneNumber}).",
            null,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<TrialSessionDto>> GetTrialSessionsAsync(CancellationToken cancellationToken)
    {
        var trials = await _db.TrialSessions
            .Include(t => t.Lead)
            .Include(t => t.Class)
            .Include(t => t.Teacher)
            .OrderByDescending(t => t.TrialDate)
            .ToListAsync(cancellationToken);

        return trials.Select(t => new TrialSessionDto(
            t.Id,
            t.LeadId,
            t.Lead?.StudentName ?? "Chưa rõ",
            t.ClassId,
            t.Class?.Name ?? "Chưa rõ",
            t.TrialDate,
            t.TeacherId,
            t.Teacher?.FullName,
            t.Feedback,
            t.Result,
            t.Notes,
            t.CreatedAtUtc
        )).ToList();
    }

    public async Task<TrialSessionDto> ScheduleTrialSessionAsync(ScheduleTrialRequest request, CancellationToken cancellationToken)
    {
        var lead = await _db.Leads.FindAsync(new object[] { request.LeadId }, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy học sinh tiềm năng.");

        var trial = new TrialSession
        {
            Id = Guid.NewGuid(),
            LeadId = request.LeadId,
            ClassId = request.ClassId,
            TrialDate = request.TrialDate,
            TeacherId = request.TeacherId,
            Notes = request.Notes?.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        lead.Status = LeadStatuses.TrialScheduled;

        _db.TrialSessions.Add(trial);
        await _db.SaveChangesAsync(cancellationToken);

        var loadedTrial = await _db.TrialSessions
            .Include(t => t.Lead)
            .Include(t => t.Class)
            .Include(t => t.Teacher)
            .FirstAsync(t => t.Id == trial.Id, cancellationToken);

        await _auditLogService.LogAsync(
            null, null, null,
            "Đăng ký học thử",
            "TrialSession",
            trial.Id.ToString(),
            $"Đặt lịch học thử cho học sinh '{loadedTrial.Lead?.StudentName}' tại lớp '{loadedTrial.Class?.Name}' vào ngày {loadedTrial.TrialDate}.",
            null,
            cancellationToken);

        return new TrialSessionDto(
            loadedTrial.Id,
            loadedTrial.LeadId,
            loadedTrial.Lead?.StudentName ?? "Chưa rõ",
            loadedTrial.ClassId,
            loadedTrial.Class?.Name ?? "Chưa rõ",
            loadedTrial.TrialDate,
            loadedTrial.TeacherId,
            loadedTrial.Teacher?.FullName,
            loadedTrial.Feedback,
            loadedTrial.Result,
            loadedTrial.Notes,
            loadedTrial.CreatedAtUtc
        );
    }

    public async Task EvaluateTrialSessionAsync(Guid id, EvaluateTrialRequest request, CancellationToken cancellationToken)
    {
        var trial = await _db.TrialSessions
            .Include(t => t.Lead)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy lịch hẹn học thử.");

        trial.Feedback = request.Feedback?.Trim();
        trial.Result = request.Result;
        trial.Notes = request.Notes?.Trim();

        if (trial.Lead != null)
        {
            trial.Lead.Status = LeadStatuses.TrialCompleted;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAsync(
            null, null, null,
            "Đánh giá học thử",
            "TrialSession",
            trial.Id.ToString(),
            $"Đánh giá kết quả học thử cho học sinh '{trial.Lead?.StudentName}': {request.Result}. Nhận xét: {request.Feedback}.",
            null,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<ParentCareLogDto>> GetParentCareLogsAsync(Guid? parentId, Guid? leadId, CancellationToken cancellationToken)
    {
        var query = _db.ParentCareLogs
            .Include(c => c.Parent)
            .Include(c => c.Lead)
            .AsQueryable();

        if (parentId.HasValue)
        {
            query = query.Where(c => c.ParentId == parentId.Value);
        }
        if (leadId.HasValue)
        {
            query = query.Where(c => c.LeadId == leadId.Value);
        }

        var logs = await query
            .OrderByDescending(c => c.LoggedAtUtc)
            .ToListAsync(cancellationToken);

        return logs.Select(c => new ParentCareLogDto(
            c.Id,
            c.ParentId,
            c.Parent?.FullName,
            c.LeadId,
            c.Lead?.StudentName,
            c.StaffId,
            c.ContactType,
            c.Notes,
            c.LoggedAtUtc
        )).ToList();
    }

    public async Task<ParentCareLogDto> CreateCareLogAsync(CreateCareLogRequest request, CancellationToken cancellationToken)
    {
        var log = new ParentCareLog
        {
            Id = Guid.NewGuid(),
            ParentId = request.ParentId,
            LeadId = request.LeadId,
            StaffId = request.StaffId,
            ContactType = request.ContactType,
            Notes = request.Notes.Trim(),
            LoggedAtUtc = DateTime.UtcNow
        };

        _db.ParentCareLogs.Add(log);
        await _db.SaveChangesAsync(cancellationToken);

        var loadedLog = await _db.ParentCareLogs
            .Include(c => c.Parent)
            .Include(c => c.Lead)
            .FirstAsync(c => c.Id == log.Id, cancellationToken);

        return new ParentCareLogDto(
            loadedLog.Id,
            loadedLog.ParentId,
            loadedLog.Parent?.FullName,
            loadedLog.LeadId,
            loadedLog.Lead?.StudentName,
            loadedLog.StaffId,
            loadedLog.ContactType,
            loadedLog.Notes,
            loadedLog.LoggedAtUtc
        );
    }
}
