using EducationCenter.Crm.Application.Common.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EducationCenter.Crm.Infrastructure.Services;

/// <summary>
/// Authentication priority:
///   1. GoogleCalendar:CredentialsJson (Service Account JSON) – set via user-secrets or env var
///   2. Application Default Credentials (ADC) – run: gcloud auth application-default login
///   3. DRY-RUN (mock) – logs only, no real events created
/// </summary>
public sealed class GoogleCalendarService : IGoogleCalendarService
{
    private readonly ILogger<GoogleCalendarService> _logger;
    private readonly string? _credentialsJson;
    private readonly string _calendarId;
    private readonly bool _useAdc;
    private readonly bool _isDryRun;

    public GoogleCalendarService(IConfiguration configuration, ILogger<GoogleCalendarService> logger)
    {
        _logger = logger;
        _credentialsJson = configuration["GoogleCalendar:CredentialsJson"];
        _calendarId = configuration["GoogleCalendar:CalendarId"] ?? "primary";

        var hasJsonKey = !string.IsNullOrWhiteSpace(_credentialsJson);
        var adcMode = configuration["GoogleCalendar:UseApplicationDefaultCredentials"];
        _useAdc = !hasJsonKey && !string.Equals(adcMode, "false", StringComparison.OrdinalIgnoreCase);

        if (hasJsonKey)
        {
            _logger.LogInformation("Google Calendar: using Service Account JSON credentials. CalendarId={CalendarId}", _calendarId);
        }
        else if (_useAdc)
        {
            _logger.LogInformation(
                "Google Calendar: no JSON key configured — will attempt Application Default Credentials (ADC). " +
                "Run 'gcloud auth application-default login' if not done. CalendarId={CalendarId}", _calendarId);
        }
        else
        {
            _logger.LogWarning(
                "Google Calendar: no credentials configured and ADC is disabled. Running in DRY-RUN mode (events logged only).");
            _isDryRun = true;
        }
    }

    private async Task<CalendarService?> GetCalendarServiceAsync()
    {
        if (_isDryRun)
        {
            return null;
        }

        try
        {
            GoogleCredential credential;

            if (!string.IsNullOrWhiteSpace(_credentialsJson))
            {
                // Mode 1: Service Account JSON key
                credential = GoogleCredential.FromJson(_credentialsJson)
                    .CreateScoped(CalendarService.Scope.Calendar);
            }
            else
            {
                // Mode 2: Application Default Credentials (gcloud auth application-default login)
                credential = await GoogleCredential.GetApplicationDefaultAsync();
                credential = credential.CreateScoped(CalendarService.Scope.Calendar);
            }

            return new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Education Center CRM"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to initialize Google Calendar Service (ADC or JSON). " +
                "Ensure 'gcloud auth application-default login' has been run or credentials are configured correctly.");
            return null;
        }
    }

    public async Task<string?> CreateEventAsync(
        string summary,
        string location,
        string description,
        DateTime startDateTime,
        DateTime endDateTime,
        CancellationToken cancellationToken)
    {
        var service = await GetCalendarServiceAsync();
        if (service == null)
        {
            var mockId = $"mock-gcal-{Guid.NewGuid()}";
            _logger.LogInformation(
                "[DRY-RUN GCal] CreateEvent: Summary={Summary}, Start={Start}, End={End} -> MockId={MockId}",
                summary, startDateTime, endDateTime, mockId);
            return mockId;
        }

        try
        {
            var ev = new Event
            {
                Summary = summary,
                Location = location,
                Description = description,
                Start = new EventDateTime { DateTimeDateTimeOffset = startDateTime },
                End = new EventDateTime { DateTimeDateTimeOffset = endDateTime }
            };

            var request = service.Events.Insert(ev, _calendarId);
            var created = await request.ExecuteAsync(cancellationToken);
            _logger.LogInformation("Google Calendar: event created. Id={Id}", created.Id);
            return created.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google Calendar: failed to create event.");
            return null;
        }
    }

    public async Task UpdateEventAsync(
        string eventId,
        string summary,
        string location,
        string description,
        DateTime startDateTime,
        DateTime endDateTime,
        CancellationToken cancellationToken)
    {
        var service = await GetCalendarServiceAsync();
        if (service == null)
        {
            _logger.LogInformation(
                "[DRY-RUN GCal] UpdateEvent: Id={Id}, Summary={Summary}, Start={Start}, End={End}",
                eventId, summary, startDateTime, endDateTime);
            return;
        }

        try
        {
            var ev = new Event
            {
                Summary = summary,
                Location = location,
                Description = description,
                Start = new EventDateTime { DateTimeDateTimeOffset = startDateTime },
                End = new EventDateTime { DateTimeDateTimeOffset = endDateTime }
            };

            var request = service.Events.Update(ev, _calendarId, eventId);
            await request.ExecuteAsync(cancellationToken);
            _logger.LogInformation("Google Calendar: event updated. Id={Id}", eventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google Calendar: failed to update event. Id={Id}", eventId);
        }
    }

    public async Task DeleteEventAsync(
        string eventId,
        CancellationToken cancellationToken)
    {
        var service = await GetCalendarServiceAsync();
        if (service == null)
        {
            _logger.LogInformation("[DRY-RUN GCal] DeleteEvent: Id={Id}", eventId);
            return;
        }

        try
        {
            var request = service.Events.Delete(_calendarId, eventId);
            await request.ExecuteAsync(cancellationToken);
            _logger.LogInformation("Google Calendar: event deleted. Id={Id}", eventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google Calendar: failed to delete event. Id={Id}", eventId);
        }
    }
}
