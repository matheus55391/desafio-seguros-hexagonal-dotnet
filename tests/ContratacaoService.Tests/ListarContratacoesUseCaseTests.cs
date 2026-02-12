using ContratacaoService.Application.UseCases;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Tests.Fakes;
using Xunit;

namespace ContratacaoService.Tests;

public class ListarContratacoesUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_DeveRetornarListaVaziaQuandoNaoHaContratacoes()
    {
        var repository = new FakeContratacaoRepository();
        var useCase = new ListarContratacoesUseCase(repository);

        var result = await useCase.ExecuteAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarTodasAsContratacoes()
    {
        var repository = new FakeContratacaoRepository();
        var contratacao1 = new Contratacao(Guid.NewGuid());
        var contratacao2 = new Contratacao(Guid.NewGuid());
        
        repository.Itens.Add(contratacao1);
        repository.Itens.Add(contratacao2);

        var useCase = new ListarContratacoesUseCase(repository);

        var result = (await useCase.ExecuteAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Id == contratacao1.Id);
        Assert.Contains(result, c => c.Id == contratacao2.Id);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarDadosCorretosDeContratacao()
    {
        var repository = new FakeContratacaoRepository();
        var propostaId = Guid.NewGuid();
        var contratacao = new Contratacao(propostaId);
        
        repository.Itens.Add(contratacao);

        var useCase = new ListarContratacoesUseCase(repository);

        var result = (await useCase.ExecuteAsync()).ToList();

        Assert.Single(result);
        Assert.Equal(contratacao.Id, result[0].Id);
        Assert.Equal(propostaId, result[0].PropostaId);
        Assert.Equal(contratacao.DataContratacao, result[0].DataContratacao);
    }
}
