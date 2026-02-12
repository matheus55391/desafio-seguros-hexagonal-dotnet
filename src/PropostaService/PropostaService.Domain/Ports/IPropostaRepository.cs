using PropostaService.Domain.Entities;

namespace PropostaService.Domain.Ports;

/// <summary>
/// Port (interface) para o repositorio de propostas - parte da arquitetura hexagonal
/// </summary>
public interface IPropostaRepository
{
    Task<Proposta?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<Proposta>> ObterTodasAsync();
    Task<Proposta> CriarAsync(Proposta proposta);
    Task AtualizarAsync(Proposta proposta);
    Task<bool> ExisteAsync(Guid id);
}
