using ContratacaoService.Application.DTOs;
using ContratacaoService.Application.UseCases;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Tests.Fakes;
using Xunit;

namespace ContratacaoService.Tests;

public class ContratarPropostaUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_DeveCriarContratacaoQuandoAprovada()
    {
        var repository = new FakeContratacaoRepository();
        var client = new FakePropostaServiceClient(StatusProposta.Aprovada);
        var publisher = new FakeMessagePublisher();
        var useCase = new ContratarPropostaUseCase(repository, client, publisher);

        var propostaId = Guid.NewGuid();
        var response = await useCase.ExecuteAsync(new ContratarPropostaRequest(propostaId));

        Assert.Equal(propostaId, response.PropostaId);
        Assert.True(repository.Itens.Any());
        Assert.True(client.AtualizouStatus);
        Assert.True(publisher.Publicou);
    }

    [Fact]
    public async Task ExecuteAsync_DeveFalharQuandoNaoAprovada()
    {
        var repository = new FakeContratacaoRepository();
        var client = new FakePropostaServiceClient(StatusProposta.Rejeitada);
        var publisher = new FakeMessagePublisher();
        var useCase = new ContratarPropostaUseCase(repository, client, publisher);

        var propostaId = Guid.NewGuid();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(new ContratarPropostaRequest(propostaId)));
    }

    [Fact]
    public async Task ExecuteAsync_DeveFalharQuandoJaContratada()
    {
        var repository = new FakeContratacaoRepository();
        var client = new FakePropostaServiceClient(StatusProposta.Aprovada);
        var publisher = new FakeMessagePublisher();
        var useCase = new ContratarPropostaUseCase(repository, client, publisher);

        var propostaId = Guid.NewGuid();
        await useCase.ExecuteAsync(new ContratarPropostaRequest(propostaId));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(new ContratarPropostaRequest(propostaId)));
    }
}
