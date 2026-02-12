using PropostaService.Application.DTOs;

namespace PropostaService.Application.Interfaces;

public interface IListarPropostasUseCase
{
    Task<IEnumerable<PropostaResponse>> ExecuteAsync();
}
