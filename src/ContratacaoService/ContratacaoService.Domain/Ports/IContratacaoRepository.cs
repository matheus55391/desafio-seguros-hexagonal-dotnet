using ContratacaoService.Domain.Entities;

namespace ContratacaoService.Domain.Ports;

public interface IContratacaoRepository
{
    Task<Contratacao?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<Contratacao>> ObterTodasAsync();
    Task<Contratacao> CriarAsync(Contratacao contratacao);
    Task<bool> ExistePorPropostaIdAsync(Guid propostaId);
}
