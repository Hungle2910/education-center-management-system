using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using EducationCenter.Crm.Application.Auth;
using EducationCenter.Crm.Application.CoreData;
using EducationCenter.Crm.Infrastructure.Auth;
using EducationCenter.Crm.Infrastructure.CoreData;
using EducationCenter.Crm.Infrastructure.Persistence;

using EducationCenter.Crm.Application.Schedules;
using EducationCenter.Crm.Infrastructure.Schedules;
using EducationCenter.Crm.Application.Attendance;
using EducationCenter.Crm.Infrastructure.Attendance;
using EducationCenter.Crm.Application.Tuition;
using EducationCenter.Crm.Infrastructure.Tuition;
using EducationCenter.Crm.Application.Dashboard;
using EducationCenter.Crm.Infrastructure.Dashboard;
using EducationCenter.Crm.Application.Reports;
using EducationCenter.Crm.Infrastructure.Reports;
using EducationCenter.Crm.Application.Common;
using EducationCenter.Crm.Application.Common.Interfaces;
using EducationCenter.Crm.Infrastructure.Services;
using EducationCenter.Crm.Application.Admissions;
using EducationCenter.Crm.Infrastructure.Admissions;
using EducationCenter.Crm.Infrastructure.Logging;

namespace EducationCenter.Crm.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Thiếu cấu hình kết nối database.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddHttpContextAccessor();

        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var secretKey = jwtSection[nameof(JwtOptions.SecretKey)];
        if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
        {
            throw new InvalidOperationException("Thiếu cấu hình JWT hợp lệ.");
        }

        services.Configure<JwtOptions>(options =>
        {
            options.Issuer = jwtSection[nameof(JwtOptions.Issuer)]
                ?? throw new InvalidOperationException("Thiếu JWT issuer.");
            options.Audience = jwtSection[nameof(JwtOptions.Audience)]
                ?? throw new InvalidOperationException("Thiếu JWT audience.");
            options.SecretKey = secretKey;

            if (int.TryParse(jwtSection[nameof(JwtOptions.ExpiryMinutes)], out var expiryMinutes))
            {
                options.ExpiryMinutes = expiryMinutes;
            }
        });
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IParentService, ParentService>();
        services.AddScoped<ITeacherService, TeacherService>();
        services.AddScoped<IClassService, ClassService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<ITuitionService, TuitionService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IExcelService, ExcelService>();
        services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();
        services.AddScoped<IAdmissionsService, AdmissionsService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IVietQrService, VietQrService>();
        services.AddScoped<IPaymentSettingService, PaymentSettingService>();
        
        return services;
    }
}
