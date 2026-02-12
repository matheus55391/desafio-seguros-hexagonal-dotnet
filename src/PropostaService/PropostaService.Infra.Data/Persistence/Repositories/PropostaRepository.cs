using Microsoft.EntityFrameworkCore;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Ports;
using PropostaService.Infra.Data.Persistence;

namespace PropostaService.Infra.Data.Persistence.Repositories;

/// <summary>
/// Adapter que implementa o port IPropostaRepository - Arquitetura Hexagonal
/// </summary>
public class PropostaRepository : IPropostaRepository
{
    private readonly PropostaDbContext _context;

    public PropostaRepository(PropostaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Proposta?> ObterPorIdAsync(Guid id)
    {
        return await _context.Propostas
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Proposta>> ObterTodasAsync()
    {
        return await _context.Propostas
            .OrderByDescending(p => p.DataCriacao)
            .ToListAsync();
    }

    public async Task<Proposta> CriarAsync(Proposta proposta)
    {
        await _context.Propostas.AddAsync(proposta);
        await _context.SaveChangesAsync();
        return proposta;
    }

    public async Task AtualizarAsync(Proposta proposta)
    {
        _context.Propostas.Update(proposta);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExisteAsync(Guid id)
    {
        return await _context.Propostas.AnyAsync(p => p.Id == id);
    }
}
