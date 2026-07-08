using EducationCenter.Crm.Api.Realtime;
using EducationCenter.Crm.Application.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace EducationCenter.Crm.Tests;

public sealed class SignalRNotificationServiceTests
{
    [Fact]
    public async Task SendToGroupAsync_SendsNotificationToExpectedGroup()
    {
        var proxy = new RecordingClientProxy();
        var hubContext = new FakeHubContext(proxy);
        var service = new SignalRNotificationService(hubContext);
        var notification = new NotificationDto(
            "Lịch học thay đổi",
            "Lớp đã đổi lịch.",
            NotificationLevels.Warning,
            DateTime.UtcNow);

        await service.SendToGroupAsync("class:demo", notification);

        Assert.Equal("class:demo", hubContext.RecordingClients.LastGroupName);
        Assert.Equal(NotificationHubEvents.ReceiveNotification, proxy.LastMethod);
        Assert.Same(notification, Assert.Single(proxy.LastArguments));
    }

    private sealed class FakeHubContext : IHubContext<NotificationHub>
    {
        public FakeHubContext(RecordingClientProxy proxy)
        {
            RecordingClients = new RecordingHubClients(proxy);
            Clients = RecordingClients;
            Groups = new NoOpGroupManager();
        }

        public RecordingHubClients RecordingClients { get; }

        public IHubClients Clients { get; }
        
        public IGroupManager Groups { get; }
    }

    private sealed class RecordingHubClients : IHubClients
    {
        private readonly RecordingClientProxy _proxy;

        public RecordingHubClients(RecordingClientProxy proxy)
        {
            _proxy = proxy;
        }

        public string? LastGroupName { get; private set; }

        public IClientProxy All => _proxy;

        public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => _proxy;

        public IClientProxy Client(string connectionId) => _proxy;

        public IClientProxy Clients(IReadOnlyList<string> connectionIds) => _proxy;

        public IClientProxy Group(string groupName)
        {
            LastGroupName = groupName;
            return _proxy;
        }

        public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds)
        {
            LastGroupName = groupName;
            return _proxy;
        }

        public IClientProxy Groups(IReadOnlyList<string> groupNames)
        {
            LastGroupName = groupNames.FirstOrDefault();
            return _proxy;
        }

        public IClientProxy User(string userId) => _proxy;

        public IClientProxy Users(IReadOnlyList<string> userIds) => _proxy;
    }

    private sealed class RecordingClientProxy : IClientProxy
    {
        public string? LastMethod { get; private set; }

        public IReadOnlyList<object?> LastArguments { get; private set; } = Array.Empty<object?>();

        public Task SendCoreAsync(
            string method,
            object?[] args,
            CancellationToken cancellationToken = default)
        {
            LastMethod = method;
            LastArguments = args;
            return Task.CompletedTask;
        }
    }

    private sealed class NoOpGroupManager : IGroupManager
    {
        public Task AddToGroupAsync(
            string connectionId,
            string groupName,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task RemoveFromGroupAsync(
            string connectionId,
            string groupName,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
