using ContratacaoService.Application.DTOs;
using ContratacaoService.Application.Interfaces;
using ContratacaoService.Domain.Ports;

namespace ContratacaoService.Application.UseCases;

public class ListarContratacoesUseCase : IListarContratacoesUseCase
{
    private readonly IContratacaoRepository _repository;

    public ListarContratacoesUseCase(IContratacaoRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<IEnumerable<ContratacaoResponse>> ExecuteAsync()
    {
        var contratacoes = await _repository.ObterTodasAsync();

        return contratacoes.Select(c => new ContratacaoResponse(
            c.Id,
            c.PropostaId,
            c.DataContratacao
        ));
    }
}
