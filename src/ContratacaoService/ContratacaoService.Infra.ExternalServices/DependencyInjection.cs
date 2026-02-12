using ContratacaoService.Domain.Ports;
using ContratacaoService.Infra.ExternalServices.Http;
using ContratacaoService.Infra.ExternalServices.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ContratacaoService.Infra.ExternalServices;

public static class DependencyInjection
{
    public static IServiceCollection AddExternalServicesInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PropostaServiceOptions>(configuration.GetSection("PropostaService"));
        services.AddHttpClient<IPropostaServiceClient, PropostaServiceClient>();

        return services;
    }
}
