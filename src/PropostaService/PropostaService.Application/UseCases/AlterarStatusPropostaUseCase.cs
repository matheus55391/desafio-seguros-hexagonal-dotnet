using PropostaService.Application.DTOs;
using PropostaService.Application.Interfaces;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Ports;

namespace PropostaService.Application.UseCases;

public class AlterarStatusPropostaUseCase : IAlterarStatusPropostaUseCase
{
    private readonly IPropostaRepository _repository;
    private readonly IMessagePublisher _messagePublisher;

    public AlterarStatusPropostaUseCase(
        IPropostaRepository repository,
        IMessagePublisher messagePublisher)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
    }

    public async Task<PropostaResponse> ExecuteAsync(Guid propostaId, AlterarStatusRequest request)
    {
        var proposta = await _repository.ObterPorIdAsync(propostaId);
        
        if (proposta == null)
        {
            throw new InvalidOperationException($"Proposta {propostaId} nao encontrada");
        }

        var novoStatus = (StatusProposta)request.NovoStatus;
        proposta.AlterarStatus(novoStatus);

        await _repository.AtualizarAsync(proposta);

        // Publicar evento de status alterado
        var evento = new PropostaStatusAlteradoEvent(
            proposta.Id,
            (int)proposta.Status,
            DateTime.UtcNow
        );

        await _messagePublisher.PublishAsync(FilaMensagem.PropostaStatusAlterado, evento);

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
