using PropostaService.Application.DTOs;
using PropostaService.Application.Interfaces;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Ports;

namespace PropostaService.Application.UseCases;

public class CriarPropostaUseCase : ICriarPropostaUseCase
{
    private readonly IPropostaRepository _repository;

    public CriarPropostaUseCase(IPropostaRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<PropostaResponse> ExecuteAsync(CriarPropostaRequest request)
    {
        var proposta = new Proposta(
            request.NomeCliente,
            request.CpfCliente,
            request.TipoSeguro,
            request.ValorCobertura,
            request.ValorPremio
        );

        await _repository.CriarAsync(proposta);

        return MapToResponse(proposta);
    }

    private static PropostaResponse MapToResponse(Proposta proposta)
    {
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
