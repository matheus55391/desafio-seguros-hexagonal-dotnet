using ContratacaoService.Domain.Entities;
using ContratacaoService.Infra.Data.Persistence;
using ContratacaoService.Infra.Data.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace ContratacaoService.Tests;

public class ContratacaoRepositoryIntegrationTests : IAsyncLifetime
{
    private PostgreSqlContainer _postgres = null!;
    private ContratacaoDbContext _dbContext = null!;
    private ContratacaoRepository _repository = null!;

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("contratacao_test_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<ContratacaoDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _dbContext = new ContratacaoDbContext(options);
        await _dbContext.Database.MigrateAsync();

        _repository = new ContratacaoRepository(_dbContext);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task CriarAsync_DevePersistirContratacaoNoBanco()
    {
        var propostaId = Guid.NewGuid();
        var contratacao = new Contratacao(propostaId);

        await _repository.CriarAsync(contratacao);
        await _dbContext.SaveChangesAsync();

        var contratacaoRecuperada = await _repository.ObterPorIdAsync(contratacao.Id);

        contratacaoRecuperada.Should().NotBeNull();
        contratacaoRecuperada!.Id.Should().Be(contratacao.Id);
        contratacaoRecuperada.PropostaId.Should().Be(propostaId);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarNull_QuandoNaoExistir()
    {
        var resultado = await _repository.ObterPorIdAsync(Guid.NewGuid());

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObterTodasAsync_DeveRetornarTodasContratacoes()
    {
        var contratacao1 = new Contratacao(Guid.NewGuid());
        var contratacao2 = new Contratacao(Guid.NewGuid());
        var contratacao3 = new Contratacao(Guid.NewGuid());

        await _repository.CriarAsync(contratacao1);
        await _repository.CriarAsync(contratacao2);
        await _repository.CriarAsync(contratacao3);
        await _dbContext.SaveChangesAsync();

        var contratacoes = await _repository.ObterTodasAsync();

        contratacoes.Should().HaveCountGreaterOrEqualTo(3);
        contratacoes.Should().Contain(c => c.Id == contratacao1.Id);
        contratacoes.Should().Contain(c => c.Id == contratacao2.Id);
        contratacoes.Should().Contain(c => c.Id == contratacao3.Id);
    }

    [Fact]
    public async Task ExistePorPropostaIdAsync_DeveRetornarTrue_QuandoExistir()
    {
        var propostaId = Guid.NewGuid();
        var contratacao = new Contratacao(propostaId);

        await _repository.CriarAsync(contratacao);
        await _dbContext.SaveChangesAsync();

        var existe = await _repository.ExistePorPropostaIdAsync(propostaId);

        existe.Should().BeTrue();
    }

    [Fact]
    public async Task ExistePorPropostaIdAsync_DeveRetornarFalse_QuandoNaoExistir()
    {
        var existe = await _repository.ExistePorPropostaIdAsync(Guid.NewGuid());

        existe.Should().BeFalse();
    }

    [Fact]
    public async Task CriarAsync_DevePersistirDataContratacao()
    {
        var antesDeCrear = DateTime.UtcNow;
        var contratacao = new Contratacao(Guid.NewGuid());
        await _repository.CriarAsync(contratacao);
        await _dbContext.SaveChangesAsync();
        var depoisDeCrear = DateTime.UtcNow;

        var contratacaoRecuperada = await _repository.ObterPorIdAsync(contratacao.Id);

        contratacaoRecuperada!.DataContratacao.Should().BeOnOrAfter(antesDeCrear);
        contratacaoRecuperada.DataContratacao.Should().BeOnOrBefore(depoisDeCrear);
    }

    [Fact]
    public async Task ObterTodasAsync_DeveRetornarListaVaziaQuandoNaoHaDados()
    {
        var contratacoes = await _repository.ObterTodasAsync();

        contratacoes.Should().BeEmpty();
    }

    [Fact]
    public async Task CriarAsync_DeveGerarIdsUnicosNoBanco()
    {
        var contratacao1 = new Contratacao(Guid.NewGuid());
        var contratacao2 = new Contratacao(Guid.NewGuid());

        await _repository.CriarAsync(contratacao1);
        await _repository.CriarAsync(contratacao2);
        await _dbContext.SaveChangesAsync();

        var todasContratacoes = await _repository.ObterTodasAsync();

        todasContratacoes.Select(c => c.Id).Should().OnlyHaveUniqueItems();
    }
}
