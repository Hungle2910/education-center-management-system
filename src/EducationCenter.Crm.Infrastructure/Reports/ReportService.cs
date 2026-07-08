using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EducationCenter.Crm.Application.Reports;
using EducationCenter.Crm.Domain.Classes;
using EducationCenter.Crm.Domain.People;
using EducationCenter.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EducationCenter.Crm.Infrastructure.Reports;

public sealed class ReportService : IReportService
{
  private readonly ApplicationDbContext _db;

  public ReportService(ApplicationDbContext db)
  {
    _db = db;
  }

  public async Task<TuitionReportResponse> GetTuitionReportAsync(CancellationToken cancellationToken)
  {
    var invoices = await _db.TuitionInvoices
        .AsNoTracking()
        .Include(i => i.Class)
        .ToListAsync(cancellationToken);

    var totalCollected = invoices
        .Where(i => i.Status == "Đã thanh toán" || i.Status == "Thanh toán dư")
        .Sum(i => i.PaidAmount ?? i.TotalAmount);

    var totalUnpaid = invoices
        .Where(i => i.Status == "Chưa thanh toán" || i.Status == "Thanh toán thiếu")
        .Sum(i => i.TotalAmount - (i.PaidAmount ?? 0));

    var totalOverdue = invoices
        .Where(i => i.Status == "Quá hạn")
        .Sum(i => i.TotalAmount);

    var revenueByClass = invoices
        .Where(i => i.Status == "Đã thanh toán" || i.Status == "Thanh toán dư" || i.PaidAmount > 0)
        .GroupBy(i => new { i.ClassId, ClassName = i.Class?.Name ?? "Lớp học" })
        .Select(g => new ClassRevenueItem(
            g.Key.ClassId,
            g.Key.ClassName,
            g.Sum(i => i.PaidAmount ?? i.TotalAmount)))
        .ToList();

    var revenueByMonth = invoices
        .Where(i => i.Status == "Đã thanh toán" || i.Status == "Thanh toán dư" || i.PaidAmount > 0)
        .GroupBy(i => i.Month)
        .Select(g => new MonthlyRevenueItem(
            g.Key,
            g.Sum(i => i.PaidAmount ?? i.TotalAmount)))
        .OrderBy(m => m.Month)
        .ToList();

    return new TuitionReportResponse(
        totalCollected,
        totalUnpaid,
        totalOverdue,
        revenueByClass,
        revenueByMonth);
  }

  public async Task<IReadOnlyCollection<ClassReportItem>> GetClassReportAsync(CancellationToken cancellationToken)
  {
    var classes = await _db.Classes
        .AsNoTracking()
        .ToListAsync(cancellationToken);

    var occurrences = await _db.ScheduleOccurrences
        .AsNoTracking()
        .ToListAsync(cancellationToken);

    var attendances = await _db.Attendances
        .AsNoTracking()
        .ToListAsync(cancellationToken);

    var result = new List<ClassReportItem>();

    foreach (var c in classes)
    {
      // Sĩ số thực tế là số lượng học sinh có điểm danh trong các buổi học của lớp này
      var classOccurrenceIds = occurrences
          .Where(o => o.ClassId == c.Id)
          .Select(o => o.Id)
          .ToList();

      var activeStudents = attendances
          .Where(a => classOccurrenceIds.Contains(a.OccurrenceId))
          .Select(a => a.StudentId)
          .Distinct()
          .Count();

      // Mặc định sĩ số mục tiêu là MinStudents + 5
      var targetStudents = c.MinStudents + 5;

      // Nguy cơ lỗ nếu số học sinh thực tế dưới sĩ số tối thiểu
      var isAtRisk = activeStudents < c.MinStudents;

      result.Add(new ClassReportItem(
          c.Id,
          c.Name,
          activeStudents,
          targetStudents,
          c.Status,
          isAtRisk));
    }

    return result;
  }

  public async Task<IReadOnlyCollection<TeacherReportItem>> GetTeacherReportAsync(CancellationToken cancellationToken)
  {
    var teachers = await _db.Teachers
        .AsNoTracking()
        .Where(t => t.IsActive)
        .ToListAsync(cancellationToken);

    var occurrences = await _db.ScheduleOccurrences
        .AsNoTracking()
        .ToListAsync(cancellationToken);

    var result = new List<TeacherReportItem>();

    foreach (var t in teachers)
    {
      var teacherOccurrences = occurrences.Where(o => o.TeacherId == t.Id).ToList();

      var completed = teacherOccurrences.Count(o => o.Status == "Đã học");
      var cancelled = teacherOccurrences.Count(o => o.Status == "Đã hủy");
      var makeup = teacherOccurrences.Count(o => o.Status == "Học bù");

      // Giả định mức lương 300,000 VND / buổi học đã dạy
      decimal projectedSalary = completed * 300000;
      decimal paidSalary = projectedSalary; // Giả định đã thanh toán đủ

      result.Add(new TeacherReportItem(
          t.Id,
          t.FullName,
          completed,
          cancelled,
          makeup,
          projectedSalary,
          paidSalary));
    }

    return result;
  }
}
