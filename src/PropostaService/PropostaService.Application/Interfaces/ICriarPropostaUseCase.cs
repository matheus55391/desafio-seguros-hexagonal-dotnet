using PropostaService.Application.DTOs;

namespace PropostaService.Application.Interfaces;

public interface ICriarPropostaUseCase
{
    Task<PropostaResponse> ExecuteAsync(CriarPropostaRequest request);
}
