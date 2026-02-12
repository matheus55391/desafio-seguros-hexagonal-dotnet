using PropostaService.Application.DTOs;
using PropostaService.Application.Interfaces;
using PropostaService.Domain.Ports;

namespace PropostaService.Application.UseCases;

public class ListarPropostasUseCase : IListarPropostasUseCase
{
    private readonly IPropostaRepository _repository;

    public ListarPropostasUseCase(IPropostaRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<IEnumerable<PropostaResponse>> ExecuteAsync()
    {
        var propostas = await _repository.ObterTodasAsync();

        return propostas.Select(p => new PropostaResponse(
            p.Id,
            p.NomeCliente,
            p.CpfCliente,
            p.TipoSeguro,
            p.ValorCobertura,
            p.ValorPremio,
            p.Status.ToString(),
            p.DataCriacao,
            p.DataAtualizacao
        ));
    }
}
