using FluentAssertions;
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

        result.Status.Should().Be("Aprovada");
        publisher.Publicou.Should().BeTrue();
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

        result.Status.Should().Be("Rejeitada");
    }

    [Fact]
    public async Task ExecuteAsync_DeveFalharQuandoPropostaNaoExiste()
    {
        var repository = new FakePropostaRepository();
        var publisher = new FakeMessagePublisher();
        var useCase = new AlterarStatusPropostaUseCase(repository, publisher);
        var request = new AlterarStatusRequest((int)StatusProposta.Aprovada);

        Func<Task> act = async () => await useCase.ExecuteAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*nao encontrada*");
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

        Func<Task> act = async () => await useCase.ExecuteAsync(proposta.Id, request);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [InlineData(StatusProposta.EmAnalise, StatusProposta.Aprovada)]
    [InlineData(StatusProposta.EmAnalise, StatusProposta.Rejeitada)]
    [InlineData(StatusProposta.Aprovada, StatusProposta.Contratada)]
    public async Task ExecuteAsync_DevePermitirTransicoesValidas(
        StatusProposta statusInicial,
        StatusProposta statusFinal)
    {
        var repository = new FakePropostaRepository();
        var publisher = new FakeMessagePublisher();
        var proposta = new Proposta("Cliente", "123", "Auto", 100000m, 500m);

        if (statusInicial == StatusProposta.Aprovada)
        {
            proposta.AlterarStatus(StatusProposta.Aprovada);
        }

        repository.Propostas.Add(proposta);

        var useCase = new AlterarStatusPropostaUseCase(repository, publisher);
        var request = new AlterarStatusRequest((int)statusFinal);

        var result = await useCase.ExecuteAsync(proposta.Id, request);

        result.Status.Should().Be(statusFinal.ToString());
    }

    [Theory]
    [InlineData(StatusProposta.Rejeitada, StatusProposta.Aprovada)]
    [InlineData(StatusProposta.Rejeitada, StatusProposta.EmAnalise)]
    [InlineData(StatusProposta.Contratada, StatusProposta.Aprovada)]
    [InlineData(StatusProposta.Contratada, StatusProposta.EmAnalise)]
    [InlineData(StatusProposta.Aprovada, StatusProposta.EmAnalise)]
    public async Task ExecuteAsync_DeveBloquearTransicoesInvalidas(
        StatusProposta statusInicial,
        StatusProposta statusFinal)
    {
        var repository = new FakePropostaRepository();
        var publisher = new FakeMessagePublisher();
        var proposta = new Proposta("Cliente", "123", "Auto", 100000m, 500m);

        // Configurar status inicial
        if (statusInicial == StatusProposta.Aprovada)
        {
            proposta.AlterarStatus(StatusProposta.Aprovada);
        }
        else if (statusInicial == StatusProposta.Rejeitada)
        {
            proposta.AlterarStatus(StatusProposta.Rejeitada);
        }
        else if (statusInicial == StatusProposta.Contratada)
        {
            proposta.AlterarStatus(StatusProposta.Aprovada);
            proposta.MarcarComoContratada();
        }

        repository.Propostas.Add(proposta);

        var useCase = new AlterarStatusPropostaUseCase(repository, publisher);
        var request = new AlterarStatusRequest((int)statusFinal);

        Func<Task> act = async () => await useCase.ExecuteAsync(proposta.Id, request);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ExecuteAsync_DevePublicarEventoAposAlteracao()
    {
        var repository = new FakePropostaRepository();
        var publisher = new FakeMessagePublisher();
        var proposta = new Proposta("Cliente", "123", "Auto", 100000m, 500m);
        repository.Propostas.Add(proposta);

        var useCase = new AlterarStatusPropostaUseCase(repository, publisher);
        var request = new AlterarStatusRequest((int)StatusProposta.Aprovada);

        await useCase.ExecuteAsync(proposta.Id, request);

        publisher.Publicou.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_DeveAtualizarDataAtualizacao()
    {
        var repository = new FakePropostaRepository();
        var publisher = new FakeMessagePublisher();
        var proposta = new Proposta("Cliente", "123", "Auto", 100000m, 500m);
        repository.Propostas.Add(proposta);

        var useCase = new AlterarStatusPropostaUseCase(repository, publisher);
        var request = new AlterarStatusRequest((int)StatusProposta.Aprovada);

        var antesAlteracao = DateTime.UtcNow.AddSeconds(-1);
        await useCase.ExecuteAsync(proposta.Id, request);
        var depoisAlteracao = DateTime.UtcNow.AddSeconds(1);

        var propostaAtualizada = repository.Propostas.First(p => p.Id == proposta.Id);
        propostaAtualizada.DataAtualizacao.Should().NotBeNull();
        propostaAtualizada.DataAtualizacao.Should().BeOnOrAfter(antesAlteracao);
        propostaAtualizada.DataAtualizacao.Should().BeOnOrBefore(depoisAlteracao);
    }

    [Fact]
    public async Task ExecuteAsync_NaoDevePublicarEventoQuandoFalhar()
    {
        var repository = new FakePropostaRepository();
        var publisher = new FakeMessagePublisher();
        var proposta = new Proposta("Cliente", "123", "Auto", 100000m, 500m);
        repository.Propostas.Add(proposta);

        var useCase = new AlterarStatusPropostaUseCase(repository, publisher);
        var request = new AlterarStatusRequest((int)StatusProposta.Contratada); // Transição inválida

        try
        {
            await useCase.ExecuteAsync(proposta.Id, request);
        }
        catch
        {
            // Ignorar exceção esperada
        }

        publisher.Publicou.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_DeveRetornarDadosAtualizados()
    {
        var repository = new FakePropostaRepository();
        var publisher = new FakeMessagePublisher();
        var proposta = new Proposta("João Silva", "12345678901", "Auto", 100000m, 500m);
        repository.Propostas.Add(proposta);

        var useCase = new AlterarStatusPropostaUseCase(repository, publisher);
        var request = new AlterarStatusRequest((int)StatusProposta.Aprovada);

        var result = await useCase.ExecuteAsync(proposta.Id, request);

        result.Id.Should().Be(proposta.Id);
        result.NomeCliente.Should().Be("João Silva");
        result.CpfCliente.Should().Be("12345678901");
        result.TipoSeguro.Should().Be("Auto");
        result.ValorCobertura.Should().Be(100000m);
        result.ValorPremio.Should().Be(500m);
        result.Status.Should().Be("Aprovada");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100)]
    [InlineData(999)]
    public async Task ExecuteAsync_DeveFalhar_StatusInvalido(int statusInvalido)
    {
        var repository = new FakePropostaRepository();
        var publisher = new FakeMessagePublisher();
        var proposta = new Proposta("Cliente", "123", "Auto", 100000m, 500m);
        repository.Propostas.Add(proposta);

        var useCase = new AlterarStatusPropostaUseCase(repository, publisher);
        var request = new AlterarStatusRequest(statusInvalido);

        Func<Task> act = async () => await useCase.ExecuteAsync(proposta.Id, request);

        await act.Should().ThrowAsync<Exception>();
    }
}

