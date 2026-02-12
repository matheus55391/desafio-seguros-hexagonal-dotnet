using PropostaService.Application.DTOs;
using PropostaService.Application.Interfaces;
using PropostaService.Domain.Ports;

namespace PropostaService.Application.UseCases;

public class ObterPropostaPorIdUseCase : IObterPropostaPorIdUseCase
{
    private readonly IPropostaRepository _repository;

    public ObterPropostaPorIdUseCase(IPropostaRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<PropostaResponse?> ExecuteAsync(Guid propostaId)
    {
        var proposta = await _repository.ObterPorIdAsync(propostaId);
        
        if (proposta == null)
        {
            return null;
        }

        return new PropostaResponse(
            proposta.Id,
            proposta.NomeCliente,
            proposta.CpfCliente,
            proposta.TipoSeguro,
            proposta.ValorCobertura,
            proposta.ValorPremio,
            proposta.Status.ToString(),
            proposta.DataCriacao,
            proposta.DataAtualizacao
        );
    }
}
