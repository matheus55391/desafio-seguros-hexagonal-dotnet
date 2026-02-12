using PropostaService.Application.UseCases;
using PropostaService.Domain.Entities;
using PropostaService.Tests.Fakes;
using Xunit;

namespace PropostaService.Tests;

public class ObterPropostaPorIdUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_DeveRetornarNullQuandoPropostaNaoExiste()
    {
        var repository = new FakePropostaRepository();
        var useCase = new ObterPropostaPorIdUseCase(repository);

        var result = await useCase.ExecuteAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarPropostaQuandoExiste()
    {
        var repository = new FakePropostaRepository();
        var proposta = new Proposta("Maria Silva", "12345678901", "Residencial", 100000m, 250m);
        repository.Propostas.Add(proposta);

        var useCase = new ObterPropostaPorIdUseCase(repository);

        var result = await useCase.ExecuteAsync(proposta.Id);

        Assert.NotNull(result);
        Assert.Equal(proposta.Id, result.Id);
        Assert.Equal("Maria Silva", result.NomeCliente);
        Assert.Equal("12345678901", result.CpfCliente);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarStatusCorreto()
    {
        var repository = new FakePropostaRepository();
        var proposta = new Proposta("Cliente", "123", "Automóvel", 50000m, 150m);
        proposta.AlterarStatus(StatusProposta.Aprovada);
        repository.Propostas.Add(proposta);

        var useCase = new ObterPropostaPorIdUseCase(repository);

        var result = await useCase.ExecuteAsync(proposta.Id);

        Assert.NotNull(result);
        Assert.Equal("Aprovada", result.Status);
    }
}
