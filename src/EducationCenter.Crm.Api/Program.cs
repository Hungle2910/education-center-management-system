using EducationCenter.Crm.Api.Contracts;
using EducationCenter.Crm.Api.ExceptionHandling;
using EducationCenter.Crm.Api.Realtime;
using EducationCenter.Crm.Application;
using EducationCenter.Crm.Application.Notifications;
using EducationCenter.Crm.Domain.Identity;
using EducationCenter.Crm.Infrastructure;
using EducationCenter.Crm.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    builder.Services.AddControllers();
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
        {
            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>()
                ?? ["http://localhost:3000", "http://localhost:3001"];

            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });
    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(item => item.Value?.Errors.Count > 0)
                .SelectMany(item => item.Value!.Errors.Select(error => error.ErrorMessage))
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .ToArray();

            return new BadRequestObjectResult(
                ApiResponse<object>.Fail("Dữ liệu không hợp lệ.", errors));
        };
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Nhập JWT theo định dạng: Bearer {token}"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });
    builder.Services.AddHealthChecks();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();
    builder.Services.AddSignalR();

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddScoped<INotificationService, SignalRNotificationService>();
    AddJwtAuthentication(builder.Services, builder.Configuration);
    AddRolePolicies(builder.Services);

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();

    app.UseCors("Frontend");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<NotificationHub>("/hubs/notifications").RequireAuthorization();
    app.MapHub<CalendarHub>("/hubs/calendar").RequireAuthorization();
    app.MapHub<PaymentHub>("/hubs/payments").RequireAuthorization();

    app.Run();
}
catch (Exception exception)
{
    if (exception.GetType().Name != "HostAbortedException")
    {
        Log.Fatal(exception, "API stopped unexpectedly.");
    }
}
finally
{
    Log.CloseAndFlush();
}

static void AddJwtAuthentication(IServiceCollection services, IConfiguration configuration)
{
    var jwtSection = configuration.GetSection(JwtOptions.SectionName);
    var issuer = jwtSection[nameof(JwtOptions.Issuer)]
        ?? throw new InvalidOperationException("Thiếu JWT issuer.");
    var audience = jwtSection[nameof(JwtOptions.Audience)]
        ?? throw new InvalidOperationException("Thiếu JWT audience.");
    var secretKey = jwtSection[nameof(JwtOptions.SecretKey)]
        ?? throw new InvalidOperationException("Thiếu JWT secret.");

    services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrWhiteSpace(accessToken) &&
                        path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                },
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsJsonAsync(
                        ApiResponse<object>.Fail("Bạn cần đăng nhập."));
                },
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsJsonAsync(
                        ApiResponse<object>.Fail("Bạn không có quyền thực hiện thao tác này."));
                }
            };
        });
}

static void AddRolePolicies(IServiceCollection services)
{
    services.AddAuthorization(options =>
    {
        options.AddPolicy(AppRoles.Admin, policy => policy.RequireRole(AppRoles.Admin));
        options.AddPolicy(AppRoles.Staff, policy => policy.RequireRole(AppRoles.Admin, AppRoles.Staff));
        options.AddPolicy(AppRoles.Teacher, policy => policy.RequireRole(AppRoles.Admin, AppRoles.Teacher));
        options.AddPolicy(AppRoles.Parent, policy => policy.RequireRole(AppRoles.Admin, AppRoles.Parent));
        options.AddPolicy(AppRoles.Student, policy => policy.RequireRole(AppRoles.Admin, AppRoles.Student));
    });
}
