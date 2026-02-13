using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PropostaService.Domain.Entities;
using PropostaService.Infra.Data.Persistence;
using PropostaService.Infra.Data.Persistence.Repositories;
using Testcontainers.PostgreSql;
using Xunit;

namespace PropostaService.Tests;

public class PropostaRepositoryIntegrationTests : IAsyncLifetime
{
    private PostgreSqlContainer _postgres = null!;
    private PropostaDbContext _dbContext = null!;
    private PropostaRepository _repository = null!;

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("proposta_test_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<PropostaDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _dbContext = new PropostaDbContext(options);
        await _dbContext.Database.MigrateAsync();

        _repository = new PropostaRepository(_dbContext);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task CriarAsync_DevePersistirPropostaNoBanco()
    {
        var proposta = new Proposta(
            "João Silva",
            "12345678901",
            "Auto",
            100000m,
            500m
        );

        await _repository.CriarAsync(proposta);
        await _dbContext.SaveChangesAsync();

        var propostaRecuperada = await _repository.ObterPorIdAsync(proposta.Id);

        propostaRecuperada.Should().NotBeNull();
        propostaRecuperada!.Id.Should().Be(proposta.Id);
        propostaRecuperada.NomeCliente.Should().Be("João Silva");
        propostaRecuperada.CpfCliente.Should().Be("12345678901");
        propostaRecuperada.Status.Should().Be(StatusProposta.EmAnalise);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarNull_QuandoNaoExistir()
    {
        var resultado = await _repository.ObterPorIdAsync(Guid.NewGuid());

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObterTodasAsync_DeveRetornarTodasPropostas()
    {
        var proposta1 = new Proposta("Cliente 1", "111", "Auto", 10000m, 100m);
        var proposta2 = new Proposta("Cliente 2", "222", "Residencial", 20000m, 200m);
        var proposta3 = new Proposta("Cliente 3", "333", "Vida", 30000m, 300m);

        await _repository.CriarAsync(proposta1);
        await _repository.CriarAsync(proposta2);
        await _repository.CriarAsync(proposta3);
        await _dbContext.SaveChangesAsync();

        var propostas = await _repository.ObterTodasAsync();

        propostas.Should().HaveCountGreaterOrEqualTo(3);
        propostas.Should().Contain(p => p.Id == proposta1.Id);
        propostas.Should().Contain(p => p.Id == proposta2.Id);
        propostas.Should().Contain(p => p.Id == proposta3.Id);
    }

    [Fact]
    public async Task AtualizarAsync_DeveModificarPropostaExistente()
    {
        var proposta = new Proposta("Cliente", "123", "Auto", 10000m, 100m);
        await _repository.CriarAsync(proposta);
        await _dbContext.SaveChangesAsync();

        proposta.AlterarStatus(StatusProposta.Aprovada);
        await _repository.AtualizarAsync(proposta);
        await _dbContext.SaveChangesAsync();

        var propostaAtualizada = await _repository.ObterPorIdAsync(proposta.Id);

        propostaAtualizada.Should().NotBeNull();
        propostaAtualizada!.Status.Should().Be(StatusProposta.Aprovada);
        propostaAtualizada.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public async Task CriarAsync_DeveManterIsolamentoDeDados()
    {
        var proposta1 = new Proposta("Cliente 1", "111", "Auto", 10000m, 100m);
        var proposta2 = new Proposta("Cliente 2", "222", "Vida", 20000m, 200m);

        await _repository.CriarAsync(proposta1);
        await _dbContext.SaveChangesAsync();

        proposta1.AlterarStatus(StatusProposta.Aprovada);
        await _repository.AtualizarAsync(proposta1);
        await _dbContext.SaveChangesAsync();

        await _repository.CriarAsync(proposta2);
        await _dbContext.SaveChangesAsync();

        var proposta1Recuperada = await _repository.ObterPorIdAsync(proposta1.Id);
        var proposta2Recuperada = await _repository.ObterPorIdAsync(proposta2.Id);

        proposta1Recuperada!.Status.Should().Be(StatusProposta.Aprovada);
        proposta2Recuperada!.Status.Should().Be(StatusProposta.EmAnalise);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveFazerTracking()
    {
        var proposta = new Proposta("Cliente", "123", "Auto", 10000m, 100m);
        await _repository.CriarAsync(proposta);
        await _dbContext.SaveChangesAsync();

        var propostaTracked = await _repository.ObterPorIdAsync(proposta.Id);
        propostaTracked!.AlterarStatus(StatusProposta.Aprovada);
        await _dbContext.SaveChangesAsync();

        var propostaVerificacao = await _repository.ObterPorIdAsync(proposta.Id);
        propostaVerificacao!.Status.Should().Be(StatusProposta.Aprovada);
    }

    [Fact]
    public async Task ObterTodasAsync_DeveRetornarListaVaziaQuandoNaoHaDados()
    {
        var propostas = await _repository.ObterTodasAsync();

        propostas.Should().BeEmpty();
    }

    [Fact]
    public async Task CriarAsync_DevePersistirDatasCriacao()
    {
        var antesDeCrear = DateTime.UtcNow;
        var proposta = new Proposta("Cliente", "123", "Auto", 10000m, 100m);
        await _repository.CriarAsync(proposta);
        await _dbContext.SaveChangesAsync();
        var depoisDeCrear = DateTime.UtcNow;

        var propostaRecuperada = await _repository.ObterPorIdAsync(proposta.Id);

        propostaRecuperada.DataCriacao.Should().BeOnOrAfter(antesDeCrear);
        propostaRecuperada.DataCriacao.Should().BeOnOrBefore(depoisDeCrear);
    }
}
