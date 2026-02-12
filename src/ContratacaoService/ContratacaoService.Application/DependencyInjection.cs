using ContratacaoService.Application.Interfaces;
using ContratacaoService.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace ContratacaoService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IContratarPropostaUseCase, ContratarPropostaUseCase>();
        services.AddScoped<IListarContratacoesUseCase, ListarContratacoesUseCase>();
        services.AddScoped<IObterContratacaoPorIdUseCase, ObterContratacaoPorIdUseCase>();

        return services;
    }
}
