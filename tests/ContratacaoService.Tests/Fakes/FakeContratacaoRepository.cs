using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Ports;

namespace ContratacaoService.Tests.Fakes;

internal sealed class FakeContratacaoRepository : IContratacaoRepository
{
    public List<Contratacao> Itens { get; } = new();

    public Task<Contratacao?> ObterPorIdAsync(Guid id)
    {
        return Task.FromResult(Itens.FirstOrDefault(i => i.Id == id));
    }

    public Task<IEnumerable<Contratacao>> ObterTodasAsync()
    {
        return Task.FromResult<IEnumerable<Contratacao>>(Itens);
    }

    public Task<Contratacao> CriarAsync(Contratacao contratacao)
    {
        Itens.Add(contratacao);
        return Task.FromResult(contratacao);
    }

    public Task<bool> ExistePorPropostaIdAsync(Guid propostaId)
    {
        return Task.FromResult(Itens.Any(i => i.PropostaId == propostaId));
    }
}
