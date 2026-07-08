using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using EducationCenter.Crm.Application.Common.PhoneNumbers;
using EducationCenter.Crm.Application.Notifications;

namespace EducationCenter.Crm.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IPhoneNumberNormalizer, VietnamPhoneNumberNormalizer>();
        services.AddSingleton<INotificationGroupResolver, NotificationGroupResolver>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
