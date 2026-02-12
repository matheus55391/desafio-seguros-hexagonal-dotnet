using ContratacaoService.Application.DTOs;
using ContratacaoService.Application.Interfaces;
using ContratacaoService.Domain.Ports;

namespace ContratacaoService.Application.UseCases;

public class ObterContratacaoPorIdUseCase : IObterContratacaoPorIdUseCase
{
    private readonly IContratacaoRepository _repository;

    public ObterContratacaoPorIdUseCase(IContratacaoRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<ContratacaoResponse?> ExecuteAsync(Guid id)
    {
        var contratacao = await _repository.ObterPorIdAsync(id);
        if (contratacao == null)
        {
            return null;
        }

        return new ContratacaoResponse(
            contratacao.Id,
            contratacao.PropostaId,
            contratacao.DataContratacao
        );
    }
}
