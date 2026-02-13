using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Ports;

namespace ContratacaoService.Tests.Fakes;

internal sealed class FakeContratacaoRepository : IContratacaoRepository
{
    private readonly object _lock = new();
    public List<Contratacao> Itens { get; } = new();

    public Task<Contratacao?> ObterPorIdAsync(Guid id)
    {
        lock (_lock)
        {
            return Task.FromResult(Itens.FirstOrDefault(i => i.Id == id));
        }
    }

    public Task<IEnumerable<Contratacao>> ObterTodasAsync()
    {
        lock (_lock)
        {
            return Task.FromResult<IEnumerable<Contratacao>>(Itens.ToList().AsEnumerable());
        }
    }

    public Task<Contratacao> CriarAsync(Contratacao contratacao)
    {
        lock (_lock)
        {
            if (Itens.Any(i => i.PropostaId == contratacao.PropostaId))
            {
                throw new InvalidOperationException("Já existe uma contratação para esta proposta");
            }
            Itens.Add(contratacao);
            return Task.FromResult(contratacao);
        }
    }

    public Task<bool> ExistePorPropostaIdAsync(Guid propostaId)
    {
        lock (_lock)
        {
            return Task.FromResult(Itens.Any(i => i.PropostaId == propostaId));
        }
    }
}
