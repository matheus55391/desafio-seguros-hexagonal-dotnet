using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Ports;
using ContratacaoService.Infra.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContratacaoService.Infra.Data.Persistence.Repositories;

public class ContratacaoRepository : IContratacaoRepository
{
    private readonly ContratacaoDbContext _context;

    public ContratacaoRepository(ContratacaoDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Contratacao?> ObterPorIdAsync(Guid id)
    {
        return await _context.Contratacoes
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Contratacao>> ObterTodasAsync()
    {
        return await _context.Contratacoes
            .OrderByDescending(c => c.DataContratacao)
            .ToListAsync();
    }

    public async Task<Contratacao> CriarAsync(Contratacao contratacao)
    {
        await _context.Contratacoes.AddAsync(contratacao);
        await _context.SaveChangesAsync();
        return contratacao;
    }

    public async Task<bool> ExistePorPropostaIdAsync(Guid propostaId)
    {
        return await _context.Contratacoes.AnyAsync(c => c.PropostaId == propostaId);
    }
}
