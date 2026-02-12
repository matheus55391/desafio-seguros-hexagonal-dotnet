using PropostaService.Application.UseCases;
using PropostaService.Domain.Entities;
using PropostaService.Tests.Fakes;
using Xunit;

namespace PropostaService.Tests;

public class ListarPropostasUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_DeveRetornarListaVaziaQuandoNaoHaPropostas()
    {
        var repository = new FakePropostaRepository();
        var useCase = new ListarPropostasUseCase(repository);

        var result = await useCase.ExecuteAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarTodasAsPropostas()
    {
        var repository = new FakePropostaRepository();
        var proposta1 = new Proposta("Cliente 1", "111", "Residencial", 100000m, 200m);
        var proposta2 = new Proposta("Cliente 2", "222", "Automóvel", 50000m, 150m);
        
        repository.Propostas.Add(proposta1);
        repository.Propostas.Add(proposta2);

        var useCase = new ListarPropostasUseCase(repository);

        var result = (await useCase.ExecuteAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.NomeCliente == "Cliente 1");
        Assert.Contains(result, p => p.NomeCliente == "Cliente 2");
    }

    [Fact]
    public async Task ExecuteAsync_DeveMantereStatusCorretoNaResposta()
    {
        var repository = new FakePropostaRepository();
        var proposta = new Proposta("Cliente", "123", "Vida", 200000m, 300m);
        proposta.AlterarStatus(StatusProposta.Aprovada);
        
        repository.Propostas.Add(proposta);

        var useCase = new ListarPropostasUseCase(repository);

        var result = (await useCase.ExecuteAsync()).ToList();

        Assert.Single(result);
        Assert.Equal("Aprovada", result[0].Status);
    }
}
