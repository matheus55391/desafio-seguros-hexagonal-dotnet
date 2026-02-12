using PropostaService.Application.DTOs;
using PropostaService.Application.UseCases;
using PropostaService.Domain.Entities;
using PropostaService.Tests.Fakes;
using Xunit;

namespace PropostaService.Tests;

public class AlterarStatusPropostaUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_DeveAlterarStatusParaAprovada()
    {
        var repository = new FakePropostaRepository();
        var publisher = new FakeMessagePublisher();
        var proposta = new Proposta("Cliente", "123", "Residencial", 100000m, 200m);
        repository.Propostas.Add(proposta);

        var useCase = new AlterarStatusPropostaUseCase(repository, publisher);
        var request = new AlterarStatusRequest((int)StatusProposta.Aprovada);

        var result = await useCase.ExecuteAsync(proposta.Id, request);

        Assert.Equal("Aprovada", result.Status);
        Assert.True(publisher.Publicou);
    }

    [Fact]
    public async Task ExecuteAsync_DeveAlterarStatusParaRejeitada()
    {
        var repository = new FakePropostaRepository();
        var publisher = new FakeMessagePublisher();
        var proposta = new Proposta("Cliente", "123", "Automóvel", 50000m, 150m);
        repository.Propostas.Add(proposta);

        var useCase = new AlterarStatusPropostaUseCase(repository, publisher);
        var request = new AlterarStatusRequest((int)StatusProposta.Rejeitada);

        var result = await useCase.ExecuteAsync(proposta.Id, request);

        Assert.Equal("Rejeitada", result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_DeveFalharQuandoPropostaNaoExiste()
    {
        var repository = new FakePropostaRepository();
        var publisher = new FakeMessagePublisher();
        var useCase = new AlterarStatusPropostaUseCase(repository, publisher);
        var request = new AlterarStatusRequest((int)StatusProposta.Aprovada);

        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            useCase.ExecuteAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task ExecuteAsync_DeveFalharQuandoTransicaoInvalida()
    {
        var repository = new FakePropostaRepository();
        var publisher = new FakeMessagePublisher();
        var proposta = new Proposta("Cliente", "123", "Vida", 200000m, 300m);
        repository.Propostas.Add(proposta);

        var useCase = new AlterarStatusPropostaUseCase(repository, publisher);
        var request = new AlterarStatusRequest((int)StatusProposta.Contratada);

        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            useCase.ExecuteAsync(proposta.Id, request));
    }
}
