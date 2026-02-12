using ContratacaoService.Application.DTOs;
using ContratacaoService.Application.Interfaces;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Ports;

namespace ContratacaoService.Application.UseCases;

public class ContratarPropostaUseCase : IContratarPropostaUseCase
{
    private readonly IContratacaoRepository _repository;
    private readonly IPropostaServiceClient _propostaServiceClient;
    private readonly IMessagePublisher _messagePublisher;

    public ContratarPropostaUseCase(
        IContratacaoRepository repository,
        IPropostaServiceClient propostaServiceClient,
        IMessagePublisher messagePublisher)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _propostaServiceClient = propostaServiceClient ?? throw new ArgumentNullException(nameof(propostaServiceClient));
        _messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
    }

    public async Task<ContratacaoResponse> ExecuteAsync(ContratarPropostaRequest request)
    {
        if (request.PropostaId == Guid.Empty)
        {
            throw new InvalidOperationException("PropostaId invalido");
        }

        if (await _repository.ExistePorPropostaIdAsync(request.PropostaId))
        {
            throw new InvalidOperationException("Proposta ja contratada");
        }

        var status = await _propostaServiceClient.ObterStatusAsync(request.PropostaId);
        if (status != StatusProposta.Aprovada)
        {
            throw new InvalidOperationException("Apenas propostas aprovadas podem ser contratadas");
        }

        var contratacao = new Contratacao(request.PropostaId);
        await _repository.CriarAsync(contratacao);

        await _propostaServiceClient.AtualizarStatusParaContratadaAsync(request.PropostaId);

        var evento = new ContratacaoCriadaEvent(
            contratacao.Id,
            contratacao.PropostaId,
            contratacao.DataContratacao
        );

        await _messagePublisher.PublishAsync(FilaMensagem.ContratacaoCriada, evento);

        return new ContratacaoResponse(
            contratacao.Id,
            contratacao.PropostaId,
            contratacao.DataContratacao
        );
    }
}
