using ContratacaoService.Domain.Entities;
using ContratacaoService.Infra.ExternalServices.Http;
using ContratacaoService.Infra.ExternalServices.Options;
using FluentAssertions;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using System.Net;
using Xunit;

namespace ContratacaoService.Tests;

public class PropostaServiceClientHttpTests
{
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly PropostaServiceClient _client;
    private const string BaseUrl = "http://localhost:5001";

    public PropostaServiceClientHttpTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        var httpClient = new HttpClient(_mockHttp);

        var options = Options.Create(new PropostaServiceOptions
        {
            BaseUrl = BaseUrl
        });

        _client = new PropostaServiceClient(httpClient, options);
    }

    [Fact]
    public async Task ObterStatusAsync_DeveRetornarAprovada_QuandoPropostaAprovada()
    {
        var propostaId = Guid.NewGuid();
        var mockResponse = new
        {
            id = propostaId,
            nomeCliente = "João Silva",
            status = "Aprovada"
        };

        _mockHttp.When($"{BaseUrl}/api/propostas/{propostaId}")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(mockResponse));

        var status = await _client.ObterStatusAsync(propostaId);

        status.Should().Be(StatusProposta.Aprovada);
    }

    [Fact]
    public async Task ObterStatusAsync_DeveRetornarEmAnalise_QuandoPropostaEmAnalise()
    {
        var propostaId = Guid.NewGuid();
        var mockResponse = new { status = "EmAnalise" };

        _mockHttp.When($"{BaseUrl}/api/propostas/{propostaId}")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(mockResponse));

        var status = await _client.ObterStatusAsync(propostaId);

        status.Should().Be(StatusProposta.EmAnalise);
    }

    [Fact]
    public async Task ObterStatusAsync_DeveRetornarRejeitada_QuandoPropostaRejeitada()
    {
        var propostaId = Guid.NewGuid();
        var mockResponse = new { status = "Rejeitada" };

        _mockHttp.When($"{BaseUrl}/api/propostas/{propostaId}")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(mockResponse));

        var status = await _client.ObterStatusAsync(propostaId);

        status.Should().Be(StatusProposta.Rejeitada);
    }

    [Fact]
    public async Task ObterStatusAsync_DeveRetornarContratada_QuandoPropostaContratada()
    {
        var propostaId = Guid.NewGuid();
        var mockResponse = new { status = "Contratada" };

        _mockHttp.When($"{BaseUrl}/api/propostas/{propostaId}")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(mockResponse));

        var status = await _client.ObterStatusAsync(propostaId);

        status.Should().Be(StatusProposta.Contratada);
    }

    [Fact]
    public async Task ObterStatusAsync_DeveLancarExcecao_QuandoRequisicaoFalhar()
    {
        var propostaId = Guid.NewGuid();

        _mockHttp.When($"{BaseUrl}/api/propostas/{propostaId}")
            .Respond(HttpStatusCode.NotFound);

        Func<Task> act = async () => await _client.ObterStatusAsync(propostaId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*obter o status*");
    }

    [Fact]
    public async Task ObterStatusAsync_DeveLancarExcecao_QuandoRespostaInvalida()
    {
        var propostaId = Guid.NewGuid();

        _mockHttp.When($"{BaseUrl}/api/propostas/{propostaId}")
            .Respond("application/json", "{ \"invalid\": \"response\" }");

        Func<Task> act = async () => await _client.ObterStatusAsync(propostaId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*invalida*");
    }

    [Fact]
    public async Task ObterStatusAsync_DeveLancarExcecao_QuandoStatusInvalido()
    {
        var propostaId = Guid.NewGuid();
        var mockResponse = new { status = "StatusInexistente" };

        _mockHttp.When($"{BaseUrl}/api/propostas/{propostaId}")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(mockResponse));

        Func<Task> act = async () => await _client.ObterStatusAsync(propostaId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Status invalido*");
    }

    [Fact]
    public async Task AtualizarStatusParaContratadaAsync_DeveTerSucesso_QuandoRequisicaoBemSucedida()
    {
        var propostaId = Guid.NewGuid();

        var request = _mockHttp.When(HttpMethod.Patch, $"{BaseUrl}/api/propostas/{propostaId}/status")
            .WithContent("{\"novoStatus\":4}")
            .Respond(HttpStatusCode.OK);

        await _client.AtualizarStatusParaContratadaAsync(propostaId);

        _mockHttp.GetMatchCount(request).Should().Be(1);
    }

    [Fact]
    public async Task AtualizarStatusParaContratadaAsync_DeveLancarExcecao_QuandoRequisicaoFalhar()
    {
        var propostaId = Guid.NewGuid();

        _mockHttp.When(HttpMethod.Patch, $"{BaseUrl}/api/propostas/{propostaId}/status")
            .Respond(HttpStatusCode.BadRequest);

        Func<Task> act = async () => await _client.AtualizarStatusParaContratadaAsync(propostaId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*atualizar o status*");
    }

    [Fact]
    public async Task AtualizarStatusParaContratadaAsync_DeveEnviarPayloadCorreto()
    {
        var propostaId = Guid.NewGuid();
        string? capturedContent = null;

        _mockHttp.When(HttpMethod.Patch, $"{BaseUrl}/api/propostas/{propostaId}/status")
            .With(request =>
            {
                capturedContent = request.Content!.ReadAsStringAsync().Result;
                return true;
            })
            .Respond(HttpStatusCode.OK);

        await _client.AtualizarStatusParaContratadaAsync(propostaId);

        capturedContent.Should().Contain("\"novoStatus\":4");
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    public async Task ObterStatusAsync_DeveLancarExcecao_QuandoErroServidor(HttpStatusCode statusCode)
    {
        var propostaId = Guid.NewGuid();

        _mockHttp.When($"{BaseUrl}/api/propostas/{propostaId}")
            .Respond(statusCode);

        Func<Task> act = async () => await _client.ObterStatusAsync(propostaId);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ObterStatusAsync_DeveChamarEndpointCorreto()
    {
        var propostaId = Guid.NewGuid();
        var mockResponse = new { status = "Aprovada" };

        var request = _mockHttp.When(HttpMethod.Get, $"{BaseUrl}/api/propostas/{propostaId}")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(mockResponse));

        await _client.ObterStatusAsync(propostaId);

        _mockHttp.GetMatchCount(request).Should().Be(1);
    }

    [Fact]
    public async Task ObterStatusAsync_DeveSerCaseInsensitive_ParaNomeDoStatus()
    {
        var propostaId = Guid.NewGuid();
        var mockResponse = new { status = "aprovada" }; // minúsculo

        _mockHttp.When($"{BaseUrl}/api/propostas/{propostaId}")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(mockResponse));

        var status = await _client.ObterStatusAsync(propostaId);

        status.Should().Be(StatusProposta.Aprovada);
    }
}
