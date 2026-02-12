using ContratacaoService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContratacaoService.Infra.Data.Persistence;

public class ContratacaoDbContext : DbContext
{
    public ContratacaoDbContext(DbContextOptions<ContratacaoDbContext> options)
        : base(options)
    {
    }

    public DbSet<Contratacao> Contratacoes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Contratacao>(entity =>
        {
            entity.ToTable("contratacoes");

            entity.HasKey(c => c.Id);

            entity.Property(c => c.Id)
                .HasColumnName("id")
                .IsRequired();

            entity.Property(c => c.PropostaId)
                .HasColumnName("proposta_id")
                .IsRequired();

            entity.Property(c => c.DataContratacao)
                .HasColumnName("data_contratacao")
                .IsRequired();

            entity.HasIndex(c => c.PropostaId)
                .HasDatabaseName("ix_contratacoes_proposta_id")
                .IsUnique();
        });
    }
}
