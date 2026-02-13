using ContratacaoService.Application.DTOs;
using ContratacaoService.Application.UseCases;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Tests.Fakes;
using FluentAssertions;
using Xunit;

namespace ContratacaoService.Tests;

public class ContratarPropostaUseCaseExtendedTests
{
    [Fact]
    public async Task ExecuteAsync_DeveValidarPropostaIdNaoVazio()
    {
        var repository = new FakeContratacaoRepository();
        var client = new FakePropostaServiceClient(StatusProposta.Aprovada);
        var publisher = new FakeMessagePublisher();
        var useCase = new ContratarPropostaUseCase(repository, client, publisher);

        var request = new ContratarPropostaRequest(Guid.Empty);

        Func<Task> act = async () => await useCase.ExecuteAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*PropostaId*");
    }

    [Theory]
    [InlineData(StatusProposta.EmAnalise)]
    [InlineData(StatusProposta.Rejeitada)]
    [InlineData(StatusProposta.Contratada)]
    public async Task ExecuteAsync_DeveFalhar_QuandoPropostaNaoEstaAprovada(StatusProposta status)
    {
        var repository = new FakeContratacaoRepository();
        var client = new FakePropostaServiceClient(status);
        var publisher = new FakeMessagePublisher();
        var useCase = new ContratarPropostaUseCase(repository, client, publisher);

        var request = new ContratarPropostaRequest(Guid.NewGuid());

        Func<Task> act = async () => await useCase.ExecuteAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*aprovadas*contratadas*");
    }

    [Fact]
    public async Task ExecuteAsync_DeveAtualizarStatusDaProposta_AposContratacao()
    {
        var propostaId = Guid.NewGuid();
        var repository = new FakeContratacaoRepository();
        var client = new FakePropostaServiceClient(StatusProposta.Aprovada);
        var publisher = new FakeMessagePublisher();
        var useCase = new ContratarPropostaUseCase(repository, client, publisher);

        var request = new ContratarPropostaRequest(propostaId);

        await useCase.ExecuteAsync(request);

        client.AtualizouStatus.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_DevePublicarEvento_AposContratacao()
    {
        var repository = new FakeContratacaoRepository();
        var client = new FakePropostaServiceClient(StatusProposta.Aprovada);
        var publisher = new FakeMessagePublisher();
        var useCase = new ContratarPropostaUseCase(repository, client, publisher);

        var request = new ContratarPropostaRequest(Guid.NewGuid());

        await useCase.ExecuteAsync(request);

        publisher.Publicou.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarDadosContratacao()
    {
        var propostaId = Guid.NewGuid();
        var repository = new FakeContratacaoRepository();
        var client = new FakePropostaServiceClient(StatusProposta.Aprovada);
        var publisher = new FakeMessagePublisher();
        var useCase = new ContratarPropostaUseCase(repository, client, publisher);

        var request = new ContratarPropostaRequest(propostaId);

        var antesContratacao = DateTime.UtcNow.AddSeconds(-1);
        var response = await useCase.ExecuteAsync(request);
        var depoisContratacao = DateTime.UtcNow.AddSeconds(1);

        response.Id.Should().NotBe(Guid.Empty);
        response.PropostaId.Should().Be(propostaId);
        response.DataContratacao.Should().BeOnOrAfter(antesContratacao);
        response.DataContratacao.Should().BeOnOrBefore(depoisContratacao);
    }

    [Fact]
    public async Task ExecuteAsync_NaoDevePublicarEvento_QuandoFalhar()
    {
        var repository = new FakeContratacaoRepository();
        var client = new FakePropostaServiceClient(StatusProposta.Rejeitada);
        var publisher = new FakeMessagePublisher();
        var useCase = new ContratarPropostaUseCase(repository, client, publisher);

        var request = new ContratarPropostaRequest(Guid.NewGuid());

        try
        {
            await useCase.ExecuteAsync(request);
        }
        catch
        {
            // Ignorar exceção esperada
        }

        publisher.Publicou.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_NaoDeveAtualizarStatus_QuandoFalhar()
    {
        var repository = new FakeContratacaoRepository();
        var client = new FakePropostaServiceClient(StatusProposta.EmAnalise);
        var publisher = new FakeMessagePublisher();
        var useCase = new ContratarPropostaUseCase(repository, client, publisher);

        var request = new ContratarPropostaRequest(Guid.NewGuid());

        try
        {
            await useCase.ExecuteAsync(request);
        }
        catch
        {
            // Ignorar exceção esperada
        }

        client.AtualizouStatus.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_NaoDevePersistir_QuandoFalharValidacao()
    {
        var repository = new FakeContratacaoRepository();
        var client = new FakePropostaServiceClient(StatusProposta.Rejeitada);
        var publisher = new FakeMessagePublisher();
        var useCase = new ContratarPropostaUseCase(repository, client, publisher);

        var request = new ContratarPropostaRequest(Guid.NewGuid());

        try
        {
            await useCase.ExecuteAsync(request);
        }
        catch
        {
            // Ignorar exceção esperada
        }

        repository.Itens.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_DevePermitirContratacoesSequenciais_PropostasDiferentes()
    {
        var repository = new FakeContratacaoRepository();
        var client = new FakePropostaServiceClient(StatusProposta.Aprovada);
        var publisher = new FakeMessagePublisher();
        var useCase = new ContratarPropostaUseCase(repository, client, publisher);

        var proposta1 = Guid.NewGuid();
        var proposta2 = Guid.NewGuid();
        var proposta3 = Guid.NewGuid();

        await useCase.ExecuteAsync(new ContratarPropostaRequest(proposta1));
        await useCase.ExecuteAsync(new ContratarPropostaRequest(proposta2));
        await useCase.ExecuteAsync(new ContratarPropostaRequest(proposta3));

        repository.Itens.Should().HaveCount(3);
        repository.Itens.Select(c => c.PropostaId).Should().Contain(new[] { proposta1, proposta2, proposta3 });
    }

    [Fact]
    public async Task ExecuteAsync_DeveGerarIdsUnicos_ParaMultiplasContratacoes()
    {
        var repository = new FakeContratacaoRepository();
        var client = new FakePropostaServiceClient(StatusProposta.Aprovada);
        var publisher = new FakeMessagePublisher();
        var useCase = new ContratarPropostaUseCase(repository, client, publisher);

        await useCase.ExecuteAsync(new ContratarPropostaRequest(Guid.NewGuid()));
        await useCase.ExecuteAsync(new ContratarPropostaRequest(Guid.NewGuid()));
        await useCase.ExecuteAsync(new ContratarPropostaRequest(Guid.NewGuid()));

        repository.Itens.Select(c => c.Id).Should().OnlyHaveUniqueItems();
    }
}
