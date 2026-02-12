using ContratacaoService.Application.DTOs;

namespace ContratacaoService.Application.Interfaces;

public interface IListarContratacoesUseCase
{
    Task<IEnumerable<ContratacaoResponse>> ExecuteAsync();
}
