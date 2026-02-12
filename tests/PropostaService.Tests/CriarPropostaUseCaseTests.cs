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

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("Maria Silva", response.NomeCliente);
        Assert.Equal("12345678901", response.CpfCliente);
        Assert.Equal("Residencial", response.TipoSeguro);
        Assert.Equal(100000m, response.ValorCobertura);
        Assert.Equal(250m, response.ValorPremio);
        Assert.Equal("EmAnalise", response.Status);
        Assert.Single(repository.Propostas);
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
        Assert.Equal("João Santos", proposta.NomeCliente);
        Assert.Equal("98765432100", proposta.CpfCliente);
        Assert.Equal(StatusProposta.EmAnalise, proposta.Status);
    }
}
