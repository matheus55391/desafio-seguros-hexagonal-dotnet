using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PropostaService.Domain.Ports;
using PropostaService.Infra.Data.Persistence;
using PropostaService.Infra.Data.Persistence.Repositories;

namespace PropostaService.Infra.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddDataInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PropostaDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PropostaDb")));

        services.AddScoped<IPropostaRepository, PropostaRepository>();

        return services;
    }
}
