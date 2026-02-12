using ContratacaoService.Application.DTOs;

namespace ContratacaoService.Application.Interfaces;

public interface IContratarPropostaUseCase
{
    Task<ContratacaoResponse> ExecuteAsync(ContratarPropostaRequest request);
}
