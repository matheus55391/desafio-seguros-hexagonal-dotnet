using PropostaService.Application.DTOs;

namespace PropostaService.Application.Interfaces;

public interface IAlterarStatusPropostaUseCase
{
    Task<PropostaResponse> ExecuteAsync(Guid propostaId, AlterarStatusRequest request);
}
