using System.Net.Http.Json;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Ports;
using ContratacaoService.Infra.ExternalServices.Options;
using Microsoft.Extensions.Options;

namespace ContratacaoService.Infra.ExternalServices.Http;

public class PropostaServiceClient : IPropostaServiceClient
{
    private readonly HttpClient _httpClient;

    public PropostaServiceClient(HttpClient httpClient, IOptions<PropostaServiceOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        var settings = options?.Value ?? throw new ArgumentNullException(nameof(options));

        _httpClient.BaseAddress = new Uri(settings.BaseUrl);
    }

    public async Task<StatusProposta> ObterStatusAsync(Guid propostaId)
    {
        var response = await _httpClient.GetAsync($"/api/propostas/{propostaId}");
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Nao foi possivel obter o status da proposta");
        }

        var payload = await response.Content.ReadFromJsonAsync<PropostaResponse>();
        if (payload == null)
        {
            throw new InvalidOperationException("Resposta invalida do PropostaService");
        }

        if (!Enum.TryParse<StatusProposta>(payload.Status, true, out var status))
        {
            throw new InvalidOperationException("Status invalido recebido do PropostaService");
        }

        return status;
    }

    public async Task AtualizarStatusParaContratadaAsync(Guid propostaId)
    {
        var request = new AlterarStatusRequest((int)StatusProposta.Contratada);
        var response = await _httpClient.PatchAsJsonAsync($"/api/propostas/{propostaId}/status", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Nao foi possivel atualizar o status da proposta");
        }
    }

    private sealed record PropostaResponse(string Status);
    private sealed record AlterarStatusRequest(int NovoStatus);
}
