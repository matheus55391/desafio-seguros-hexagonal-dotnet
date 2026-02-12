using Microsoft.EntityFrameworkCore;
using PropostaService.Domain.Entities;

namespace PropostaService.Infra.Data.Persistence;

public class PropostaDbContext : DbContext
{
    public PropostaDbContext(DbContextOptions<PropostaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Proposta> Propostas { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Proposta>(entity =>
        {
            entity.ToTable("propostas");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Id)
                .HasColumnName("id")
                .IsRequired();

            entity.Property(p => p.NomeCliente)
                .HasColumnName("nome_cliente")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(p => p.CpfCliente)
                .HasColumnName("cpf_cliente")
                .HasMaxLength(11)
                .IsRequired();

            entity.Property(p => p.TipoSeguro)
                .HasColumnName("tipo_seguro")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(p => p.ValorCobertura)
                .HasColumnName("valor_cobertura")
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(p => p.ValorPremio)
                .HasColumnName("valor_premio")
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(p => p.Status)
                .HasColumnName("status")
                .HasConversion<int>()
                .IsRequired();

            entity.Property(p => p.DataCriacao)
                .HasColumnName("data_criacao")
                .IsRequired();

            entity.Property(p => p.DataAtualizacao)
                .HasColumnName("data_atualizacao");

            entity.HasIndex(p => p.CpfCliente)
                .HasDatabaseName("ix_propostas_cpf_cliente");

            entity.HasIndex(p => p.Status)
                .HasDatabaseName("ix_propostas_status");
        });
    }
}
