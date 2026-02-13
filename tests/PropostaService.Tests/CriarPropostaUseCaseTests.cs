using FluentAssertions;
using PropostaService.Application.DTOs;
using PropostaService.Application.UseCases;
using PropostaService.Domain.Entities;
using PropostaService.Tests.Fakes;
using Xunit;

namespace PropostaService.Tests;

public class CriarPropostaUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_DeveCriarPropostaComStatusEmAnalise()
    {
        var repository = new FakePropostaRepository();
        var useCase = new CriarPropostaUseCase(repository);

        var request = new CriarPropostaRequest(
            "Maria Silva",
            "12345678901",
            "Residencial",
            100000m,
            250m
        );

        var response = await useCase.ExecuteAsync(request);

        response.Id.Should().NotBe(Guid.Empty);
        response.NomeCliente.Should().Be("Maria Silva");
        response.CpfCliente.Should().Be("12345678901");
        response.TipoSeguro.Should().Be("Residencial");
        response.ValorCobertura.Should().Be(100000m);
        response.ValorPremio.Should().Be(250m);
        response.Status.Should().Be("EmAnalise");
        repository.Propostas.Should().HaveCount(1);
    }

    [Fact]
    public async Task ExecuteAsync_DeveArmazenarPropostaNoRepositorio()
    {
        var repository = new FakePropostaRepository();
        var useCase = new CriarPropostaUseCase(repository);

        var request = new CriarPropostaRequest(
            "João Santos",
            "98765432100",
            "Automóvel",
            50000m,
            150m
        );

        await useCase.ExecuteAsync(request);

        var proposta = repository.Propostas.First();
        proposta.NomeCliente.Should().Be("João Santos");
        proposta.CpfCliente.Should().Be("98765432100");
        proposta.Status.Should().Be(StatusProposta.EmAnalise);
    }

    [Theory]
    [InlineData(null, "12345678901", "Auto", 100000, 500)]
    [InlineData("", "12345678901", "Auto", 100000, 500)]
    [InlineData("João", null, "Auto", 100000, 500)]
    [InlineData("João", "", "Auto", 100000, 500)]
    [InlineData("João", "12345678901", null, 100000, 500)]
    [InlineData("João", "12345678901", "", 100000, 500)]
    public async Task ExecuteAsync_DeveFalhar_DadosObrigatoriosInvalidos(
        string? nomeCliente,
        string? cpfCliente,
        string? tipoSeguro,
        decimal valorCobertura,
        decimal valorPremio)
    {
        var repository = new FakePropostaRepository();
        var useCase = new CriarPropostaUseCase(repository);

        var request = new CriarPropostaRequest(
            nomeCliente!,
            cpfCliente!,
            tipoSeguro!,
            valorCobertura,
            valorPremio
        );

        Func<Task> act = async () => await useCase.ExecuteAsync(request);

        await act.Should().ThrowAsync<Exception>();
        repository.Propostas.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0, 500)]
    [InlineData(-1, 500)]
    [InlineData(-100, 500)]
    [InlineData(100000, 0)]
    [InlineData(100000, -1)]
    [InlineData(100000, -500)]
    [InlineData(0, 0)]
    [InlineData(-100, -500)]
    public async Task ExecuteAsync_DeveFalhar_ValoresInvalidos(
        decimal valorCobertura,
        decimal valorPremio)
    {
        var repository = new FakePropostaRepository();
        var useCase = new CriarPropostaUseCase(repository);

        var request = new CriarPropostaRequest(
            "João Silva",
            "12345678901",
            "Auto",
            valorCobertura,
            valorPremio
        );

        Func<Task> act = async () => await useCase.ExecuteAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
        repository.Propostas.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_DeveGerarIdsUnicos_ParaMultiplasPropostas()
    {
        var repository = new FakePropostaRepository();
        var useCase = new CriarPropostaUseCase(repository);

        var request1 = new CriarPropostaRequest("Cliente 1", "111", "Auto", 10000m, 100m);
        var request2 = new CriarPropostaRequest("Cliente 2", "222", "Vida", 20000m, 200m);
        var request3 = new CriarPropostaRequest("Cliente 3", "333", "Residencial", 30000m, 300m);

        var response1 = await useCase.ExecuteAsync(request1);
        var response2 = await useCase.ExecuteAsync(request2);
        var response3 = await useCase.ExecuteAsync(request3);

        new[] { response1.Id, response2.Id, response3.Id }
            .Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task ExecuteAsync_DeveCriarProposta_ComValoresMuitoAltos()
    {
        var repository = new FakePropostaRepository();
        var useCase = new CriarPropostaUseCase(repository);

        var request = new CriarPropostaRequest(
            "Empresa XYZ",
            "12345678901234",
            "Empresarial",
            999_999_999.99m,
            9_999_999.99m
        );

        var response = await useCase.ExecuteAsync(request);

        response.ValorCobertura.Should().Be(999_999_999.99m);
        response.ValorPremio.Should().Be(9_999_999.99m);
    }

    [Fact]
    public async Task ExecuteAsync_DeveCriarProposta_ComValoresMinimosMaiorQueZero()
    {
        var repository = new FakePropostaRepository();
        var useCase = new CriarPropostaUseCase(repository);

        var request = new CriarPropostaRequest(
            "Cliente",
            "123",
            "Básico",
            0.01m,
            0.01m
        );

        var response = await useCase.ExecuteAsync(request);

        response.ValorCobertura.Should().Be(0.01m);
        response.ValorPremio.Should().Be(0.01m);
    }

    [Fact]
    public async Task ExecuteAsync_DeveDefinirDataCriacaoUtc()
    {
        var repository = new FakePropostaRepository();
        var useCase = new CriarPropostaUseCase(repository);

        var antesDeCrear = DateTime.UtcNow.AddSeconds(-1);
        
        var request = new CriarPropostaRequest(
            "João Silva",
            "12345678901",
            "Auto",
            100000m,
            500m
        );

        await useCase.ExecuteAsync(request);

        var depoisDeCrear = DateTime.UtcNow.AddSeconds(1);

        var proposta = repository.Propostas.First();
        proposta.DataCriacao.Should().BeOnOrAfter(antesDeCrear);
        proposta.DataCriacao.Should().BeOnOrBefore(depoisDeCrear);
        proposta.DataCriacao.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task ExecuteAsync_DevePermitirMesmoCpfEmPropostasDiferentes()
    {
        // No domínio atual, não há restrição de CPF único
        // Isso pode ser uma regra de negócio futura
        var repository = new FakePropostaRepository();
        var useCase = new CriarPropostaUseCase(repository);

        var cpfDuplicado = "12345678901";

        var request1 = new CriarPropostaRequest("Cliente 1", cpfDuplicado, "Auto", 10000m, 100m);
        var request2 = new CriarPropostaRequest("Cliente 2", cpfDuplicado, "Vida", 20000m, 200m);

        var response1 = await useCase.ExecuteAsync(request1);
        var response2 = await useCase.ExecuteAsync(request2);

        response1.Id.Should().NotBe(response2.Id);
        repository.Propostas.Should().HaveCount(2);
    }
}

