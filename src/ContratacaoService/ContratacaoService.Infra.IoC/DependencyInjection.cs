using ContratacaoService.Infra.Data;
using ContratacaoService.Infra.ExternalServices;
using ContratacaoService.Infra.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ContratacaoService.Infra.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDataInfrastructure(configuration);
        services.AddMessagingInfrastructure(configuration);
        services.AddExternalServicesInfrastructure(configuration);

        return services;
    }
}
