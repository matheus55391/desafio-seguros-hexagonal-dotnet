using Microsoft.Extensions.DependencyInjection;
using PropostaService.Application.Interfaces;
using PropostaService.Application.UseCases;

namespace PropostaService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICriarPropostaUseCase, CriarPropostaUseCase>();
        services.AddScoped<IListarPropostasUseCase, ListarPropostasUseCase>();
        services.AddScoped<IObterPropostaPorIdUseCase, ObterPropostaPorIdUseCase>();
        services.AddScoped<IAlterarStatusPropostaUseCase, AlterarStatusPropostaUseCase>();

        return services;
    }
}
