using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EducationCenter.Crm.Api.Contracts;
using EducationCenter.Crm.Application.Common;
using EducationCenter.Crm.Application.Common.PhoneNumbers;
using EducationCenter.Crm.Application.Reports;
using EducationCenter.Crm.Domain.Classes;
using EducationCenter.Crm.Domain.Identity;
using EducationCenter.Crm.Domain.People;
using EducationCenter.Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EducationCenter.Crm.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Staff")]
[Route("api/excel")]
public sealed class ExcelController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IExcelService _excelService;
    private readonly IPhoneNumberNormalizer _phoneNumberNormalizer;
    private readonly IReportService _reportService;

    public ExcelController(
        ApplicationDbContext dbContext,
        IExcelService excelService,
        IPhoneNumberNormalizer phoneNumberNormalizer,
        IReportService reportService)
    {
        _dbContext = dbContext;
        _excelService = excelService;
        _phoneNumberNormalizer = phoneNumberNormalizer;
        _reportService = reportService;
    }

    [HttpGet("templates/students")]
    public IActionResult GetStudentTemplate()
    {
        var bytes = _excelService.GetStudentImportTemplate();
        return File(
            bytes, 
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            "Mau_Nhap_Hoc_Sinh.xlsx");
    }

    [HttpPost("import/students")]
    public async Task<IActionResult> ImportStudents(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<object>.Fail("Vui lòng chọn file Excel hợp lệ."));
        }

        using var stream = file.OpenReadStream();
        var rows = _excelService.ParseStudentImport(stream);

        if (rows == null || rows.Count == 0)
        {
            return BadRequest(ApiResponse<object>.Fail("File Excel trống hoặc sai định dạng cột."));
        }

        var importedCount = 0;
        var errorMessages = new List<string>();

        foreach (var row in rows)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(row.StudentName))
                {
                    continue;
                }

                // Parse Date of Birth
                DateOnly? dob = null;
                if (!string.IsNullOrWhiteSpace(row.DateOfBirthString))
                {
                    if (DateTime.TryParseExact(row.DateOfBirthString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                    {
                        dob = DateOnly.FromDateTime(parsedDate);
                    }
                    else if (DateTime.TryParse(row.DateOfBirthString, out var parsedDate2))
                    {
                        dob = DateOnly.FromDateTime(parsedDate2);
                    }
                }

                // Normalize parent phone
                var rawPhone = row.ParentPhone;
                string? normalizedPhone = null;
                if (!string.IsNullOrWhiteSpace(rawPhone))
                {
                    _phoneNumberNormalizer.TryNormalize(rawPhone, out var norm);
                    normalizedPhone = norm;
                }

                if (string.IsNullOrWhiteSpace(normalizedPhone))
                {
                    errorMessages.Add($"Học sinh '{row.StudentName}': Số điện thoại phụ huynh không hợp lệ.");
                    continue;
                }

                // Find or create parent
                var parent = await _dbContext.Parents
                    .FirstOrDefaultAsync(p => p.PhoneNumber == normalizedPhone, cancellationToken);

                if (parent == null)
                {
                    var parentName = string.IsNullOrWhiteSpace(row.ParentName) ? $"Phụ huynh {row.StudentName}" : row.ParentName;
                    parent = new Parent
                    {
                        Id = Guid.NewGuid(),
                        FullName = parentName.Trim(),
                        PhoneNumber = normalizedPhone,
                        ZaloLink = $"https://zalo.me/{normalizedPhone}",
                        CreatedAtUtc = DateTime.UtcNow
                    };
                    _dbContext.Parents.Add(parent);
                }

                // Create student
                var student = new Student
                {
                    Id = Guid.NewGuid(),
                    FullName = row.StudentName.Trim(),
                    DateOfBirth = dob,
                    Status = StudentStatuses.Active,
                    CreatedAtUtc = DateTime.UtcNow
                };
                _dbContext.Students.Add(student);

                // Link student and parent
                var link = new StudentParent
                {
                    StudentId = student.Id,
                    ParentId = parent.Id,
                    Relationship = "Phụ huynh",
                    CreatedAtUtc = DateTime.UtcNow
                };
                _dbContext.StudentParents.Add(link);

                // Xếp lớp (nếu có)
                if (!string.IsNullOrWhiteSpace(row.ClassName))
                {
                    var classRoom = await _dbContext.Classes
                        .FirstOrDefaultAsync(c => c.Name.ToLower() == row.ClassName.ToLower(), cancellationToken);

                    if (classRoom != null)
                    {
                        // Tạo hóa đơn học phí tháng hiện tại
                        var currentMonth = DateTime.UtcNow.ToString("yyyy-MM");
                        var hasInvoice = await _dbContext.TuitionInvoices
                            .AnyAsync(i => i.StudentId == student.Id && i.ClassId == classRoom.Id && i.Month == currentMonth, cancellationToken);

                        if (!hasInvoice)
                        {
                            var invoice = new TuitionInvoice
                            {
                                Id = Guid.NewGuid(),
                                StudentId = student.Id,
                                ClassId = classRoom.Id,
                                Month = currentMonth,
                                BaseAmount = classRoom.MonthlyFee,
                                TotalAmount = classRoom.MonthlyFee,
                                Status = "Chưa thanh toán",
                                CreatedAtUtc = DateTime.UtcNow
                            };
                            _dbContext.TuitionInvoices.Add(invoice);
                        }
                    }
                    else
                    {
                        errorMessages.Add($"Học sinh '{row.StudentName}': Không tìm thấy lớp học '{row.ClassName}'. Học sinh vẫn được tạo nhưng chưa xếp lớp.");
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                importedCount++;
            }
            catch (Exception ex)
            {
                errorMessages.Add($"Lỗi khi xử lý dòng '{row.StudentName}': {ex.Message}");
            }
        }

        var message = $"Đã nhập thành công {importedCount} học sinh.";
        if (errorMessages.Count > 0)
        {
            return Ok(ApiResponse<object>.Ok(new { importedCount, errors = errorMessages }, $"{message} Có một số cảnh báo."));
        }

        return Ok(ApiResponse<object>.Ok(new { importedCount }, message));
    }

    [HttpGet("export/students")]
    public async Task<IActionResult> ExportStudents(CancellationToken cancellationToken)
    {
        var students = await _dbContext.Students
            .AsNoTracking()
            .Include(s => s.StudentParents)
                .ThenInclude(sp => sp.Parent)
            .OrderBy(s => s.FullName)
            .ToListAsync(cancellationToken);

        var exportData = students.Select(s => new
        {
            Ma_Hoc_Sinh = s.StudentCode ?? string.Empty,
            Ho_Ten = s.FullName,
            Email = s.Email ?? string.Empty,
            So_Dien_Thoai = s.PhoneNumber ?? string.Empty,
            Ngay_Sinh = s.DateOfBirth?.ToString("dd/MM/yyyy") ?? string.Empty,
            Trang_Thai = s.Status,
            Phu_Huynh = string.Join(", ", s.StudentParents.Select(sp => $"{sp.Parent.FullName} ({sp.Parent.PhoneNumber})"))
        }).ToList();

        var bytes = _excelService.ExportToExcel(exportData, "Danh sach hoc sinh");
        return File(
            bytes, 
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            $"Danh_Sach_Hoc_Sinh_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet("export/classes")]
    public async Task<IActionResult> ExportClasses(CancellationToken cancellationToken)
    {
        var classes = await _dbContext.Classes
            .AsNoTracking()
            .Include(c => c.Teacher)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        var exportData = classes.Select(c => new
        {
            Ten_Lop = c.Name,
            Mon_Hoc = c.Subject ?? string.Empty,
            Hoc_Phi_Thang = c.MonthlyFee,
            Trang_Thai = c.Status,
            Giao_Vien = c.Teacher?.FullName ?? "Chưa phân công",
            Si_So_Toi_Thieu = c.MinStudents,
            Si_So_Toi_Da = c.MaxStudents
        }).ToList();

        var bytes = _excelService.ExportToExcel(exportData, "Danh sach lop hoc");
        return File(
            bytes, 
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            $"Danh_Sach_Lop_Hoc_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet("export/invoices")]
    public async Task<IActionResult> ExportInvoices([FromQuery] string? month, CancellationToken cancellationToken)
    {
        var targetMonth = string.IsNullOrWhiteSpace(month) ? DateTime.UtcNow.ToString("yyyy-MM") : month;

        var invoices = await _dbContext.TuitionInvoices
            .AsNoTracking()
            .Include(i => i.Student)
            .Include(i => i.Class)
            .Where(i => i.Month == targetMonth)
            .OrderBy(i => i.Class != null ? i.Class.Name : string.Empty)
                .ThenBy(i => i.Student != null ? i.Student.FullName : string.Empty)
            .ToListAsync(cancellationToken);

        var exportData = invoices.Select(i => new
        {
            Ma_Hoa_Don = i.Id,
            Hoc_Sinh = i.Student?.FullName ?? "Chưa rõ",
            Lop_Hoc = i.Class?.Name ?? "Chưa rõ",
            Thang = i.Month,
            Hoc_Phi_Goc = i.BaseAmount,
            Giam_Gia = i.DiscountAmount,
            Khau_Tru = i.DeductAmount,
            Dieu_Chinh = i.AdjustAmount,
            Tong_Tien = i.TotalAmount,
            Da_Thu = i.PaidAmount ?? 0,
            Trang_Thai = i.Status
        }).ToList();

        var bytes = _excelService.ExportToExcel(exportData, $"Hoc phi {targetMonth}");
        return File(
            bytes, 
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            $"Hoc_Phi_{targetMonth}_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet("export/tuition-report")]
    public async Task<IActionResult> ExportTuitionReport(CancellationToken cancellationToken)
    {
        var report = await _reportService.GetTuitionReportAsync(cancellationToken);

        var exportSummary = new List<object>
        {
            new { Chi_So = "Doanh thu đã thu", So_Tien = report.TotalCollected },
            new { Chi_So = "Còn nợ (Chưa thu)", So_Tien = report.TotalUnpaid },
            new { Chi_So = "Hóa đơn quá hạn", So_Tien = report.TotalOverdue }
        };

        var bytes = _excelService.ExportToExcel(exportSummary, "Tong quan doanh thu");
        return File(
            bytes, 
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            $"Bao_Cao_Doanh_Thu_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet("export/class-report")]
    public async Task<IActionResult> ExportClassReport(CancellationToken cancellationToken)
    {
        var classes = await _reportService.GetClassReportAsync(cancellationToken);

        var exportData = classes.Select(c => new
        {
            Ten_Lop = c.ClassName,
            Si_So_Thuc_Te = c.ActiveStudentCount,
            Si_So_Muc_Tieu = c.TargetStudentCount,
            Trang_Thai = c.Status,
            Rui_Ro_Tai_Chinh = c.IsAtRiskOfLoss ? "Cảnh báo lỗ" : "Ổn định"
        }).ToList();

        var bytes = _excelService.ExportToExcel(exportData, "Hieu suat lop hoc");
        return File(
            bytes, 
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            $"Bao_Cao_Lop_Hoc_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet("export/teacher-report")]
    public async Task<IActionResult> ExportTeacherReport(CancellationToken cancellationToken)
    {
        var teachers = await _reportService.GetTeacherReportAsync(cancellationToken);

        var exportData = teachers.Select(t => new
        {
            Ten_Giao_Vien = t.TeacherName,
            Buoi_Da_Day = t.CompletedLessonsCount,
            Buoi_Nghi = t.CancelledLessonsCount,
            Buoi_Day_Bu = t.MakeupLessonsCount,
            Luong_Du_Kien = t.ProjectedSalary,
            Luong_Da_Thanh_Toan = t.PaidSalary
        }).ToList();

        var bytes = _excelService.ExportToExcel(exportData, "Bang luong giao vien");
        return File(
            bytes, 
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            $"Bao_Cao_Luong_Giao_Vien_{DateTime.Now:yyyyMMdd}.xlsx");
    }
}
