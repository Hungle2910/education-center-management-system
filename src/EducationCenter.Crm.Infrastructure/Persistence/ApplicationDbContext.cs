using EducationCenter.Crm.Domain.Identity;
using EducationCenter.Crm.Domain.Classes;
using EducationCenter.Crm.Domain.People;
using EducationCenter.Crm.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace EducationCenter.Crm.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    public DbSet<Student> Students => Set<Student>();

    public DbSet<Parent> Parents => Set<Parent>();

    public DbSet<StudentParent> StudentParents => Set<StudentParent>();

    public DbSet<Teacher> Teachers => Set<Teacher>();

    public DbSet<Class> Classes => Set<Class>();

    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<ClassSchedule> ClassSchedules => Set<ClassSchedule>();
    public DbSet<ScheduleOccurrence> ScheduleOccurrences => Set<ScheduleOccurrence>();
    public DbSet<EducationCenter.Crm.Domain.Classes.Attendance> Attendances => Set<EducationCenter.Crm.Domain.Classes.Attendance>();
    public DbSet<EducationCenter.Crm.Domain.Classes.IndividualMakeup> IndividualMakeups => Set<EducationCenter.Crm.Domain.Classes.IndividualMakeup>();
    public DbSet<EducationCenter.Crm.Domain.Classes.TuitionInvoice> TuitionInvoices => Set<EducationCenter.Crm.Domain.Classes.TuitionInvoice>();
    public DbSet<EducationCenter.Crm.Domain.Settings.DiscountCode> DiscountCodes => Set<EducationCenter.Crm.Domain.Settings.DiscountCode>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<TrialSession> TrialSessions => Set<TrialSession>();
    public DbSet<ParentCareLog> ParentCareLogs => Set<ParentCareLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<PaymentSetting> PaymentSettings => Set<PaymentSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
