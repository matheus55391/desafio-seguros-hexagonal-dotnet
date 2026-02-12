using PropostaService.Application.DTOs;

namespace PropostaService.Application.Interfaces;

public interface IObterPropostaPorIdUseCase
{
    Task<PropostaResponse?> ExecuteAsync(Guid propostaId);
}
