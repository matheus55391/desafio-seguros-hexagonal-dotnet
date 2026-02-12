using PropostaService.Domain.Entities;
using PropostaService.Domain.Ports;

namespace PropostaService.Tests.Fakes;

internal sealed class FakePropostaRepository : IPropostaRepository
{
    public List<Proposta> Propostas { get; } = new();

    public Task<Proposta?> ObterPorIdAsync(Guid id)
    {
        return Task.FromResult(Propostas.FirstOrDefault(p => p.Id == id));
    }

    public Task<IEnumerable<Proposta>> ObterTodasAsync()
    {
        return Task.FromResult<IEnumerable<Proposta>>(Propostas);
    }

    public Task<Proposta> CriarAsync(Proposta proposta)
    {
        Propostas.Add(proposta);
        return Task.FromResult(proposta);
    }

    public Task AtualizarAsync(Proposta proposta)
    {
        var existing = Propostas.FirstOrDefault(p => p.Id == proposta.Id);
        if (existing != null)
        {
            Propostas.Remove(existing);
            Propostas.Add(proposta);
        }
        return Task.CompletedTask;
    }

    public Task<bool> ExisteAsync(Guid id)
    {
        return Task.FromResult(Propostas.Any(p => p.Id == id));
    }
}
