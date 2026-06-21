using Identification.Contracts;
using Identification.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace Identification;

public static class IdentificationDI
{
    public static IServiceCollection AddIdentificationSupport(this IServiceCollection services)
    {
        services.AddScoped<IIdentityInfo, IdentityInfo>();
        services.AddScoped<IInfoSetter, InfoSetter>();

        return services;
    }
}
