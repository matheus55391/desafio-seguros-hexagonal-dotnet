using ContratacaoService.Application.UseCases;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Tests.Fakes;
using Xunit;

namespace ContratacaoService.Tests;

public class ObterContratacaoPorIdUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_DeveRetornarNullQuandoContratacaoNaoExiste()
    {
        var repository = new FakeContratacaoRepository();
        var useCase = new ObterContratacaoPorIdUseCase(repository);

        var result = await useCase.ExecuteAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarContratacaoQuandoExiste()
    {
        var repository = new FakeContratacaoRepository();
        var propostaId = Guid.NewGuid();
        var contratacao = new Contratacao(propostaId);
        repository.Itens.Add(contratacao);

        var useCase = new ObterContratacaoPorIdUseCase(repository);

        var result = await useCase.ExecuteAsync(contratacao.Id);

        Assert.NotNull(result);
        Assert.Equal(contratacao.Id, result.Id);
        Assert.Equal(propostaId, result.PropostaId);
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarDataContratacaoCorreta()
    {
        var repository = new FakeContratacaoRepository();
        var propostaId = Guid.NewGuid();
        var contratacao = new Contratacao(propostaId);
        repository.Itens.Add(contratacao);

        var useCase = new ObterContratacaoPorIdUseCase(repository);

        var result = await useCase.ExecuteAsync(contratacao.Id);

        Assert.NotNull(result);
        Assert.Equal(contratacao.DataContratacao, result.DataContratacao);
        Assert.InRange(result.DataContratacao, 
            DateTime.UtcNow.AddMinutes(-1), 
            DateTime.UtcNow.AddMinutes(1));
    }
}
