using ContratacaoService.Application.DTOs;

namespace ContratacaoService.Application.Interfaces;

public interface IObterContratacaoPorIdUseCase
{
    Task<ContratacaoResponse?> ExecuteAsync(Guid id);
}
