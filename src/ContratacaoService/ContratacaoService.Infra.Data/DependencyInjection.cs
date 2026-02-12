using ContratacaoService.Domain.Ports;
using ContratacaoService.Infra.Data.Persistence;
using ContratacaoService.Infra.Data.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ContratacaoService.Infra.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddDataInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ContratacaoDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("ContratacaoDb")));

        services.AddScoped<IContratacaoRepository, ContratacaoRepository>();

        return services;
    }
}
