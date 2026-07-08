using EducationCenter.Crm.Application.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace EducationCenter.Crm.Api.Realtime;

public abstract class GroupedHub : Hub
{
    private readonly INotificationGroupResolver _groupResolver;

    protected GroupedHub(INotificationGroupResolver groupResolver)
    {
        _groupResolver = groupResolver;
    }

    public override async Task OnConnectedAsync()
    {
        var groups = _groupResolver.ResolveGroups(Context.User ?? new());

        foreach (var group in groups)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, group, Context.ConnectionAborted);
        }

        await base.OnConnectedAsync();
    }
}
