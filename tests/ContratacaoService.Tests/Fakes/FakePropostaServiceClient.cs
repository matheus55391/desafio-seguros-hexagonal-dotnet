using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Ports;

namespace ContratacaoService.Tests.Fakes;

internal sealed class FakePropostaServiceClient : IPropostaServiceClient
{
    private readonly StatusProposta _status;
    public bool AtualizouStatus { get; private set; }

    public FakePropostaServiceClient(StatusProposta status)
    {
        _status = status;
    }

    public Task<StatusProposta> ObterStatusAsync(Guid propostaId)
    {
        return Task.FromResult(_status);
    }

    public Task AtualizarStatusParaContratadaAsync(Guid propostaId)
    {
        AtualizouStatus = true;
        return Task.CompletedTask;
    }
}
