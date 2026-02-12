using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PropostaService.Infra.Data;
using PropostaService.Infra.Messaging;

namespace PropostaService.Infra.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDataInfrastructure(configuration);
        services.AddMessagingInfrastructure(configuration);

        return services;
    }
}
