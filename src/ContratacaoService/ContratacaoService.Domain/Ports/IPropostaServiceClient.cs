using ContratacaoService.Domain.Entities;

namespace ContratacaoService.Domain.Ports;

public interface IPropostaServiceClient
{
    Task<StatusProposta> ObterStatusAsync(Guid propostaId);
    Task AtualizarStatusParaContratadaAsync(Guid propostaId);
}
