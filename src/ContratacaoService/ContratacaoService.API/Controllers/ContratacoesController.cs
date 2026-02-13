using ContratacaoService.Application.DTOs;
using ContratacaoService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ContratacaoService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContratacoesController : ControllerBase
{
    private readonly IContratarPropostaUseCase _contratarPropostaUseCase;
    private readonly IListarContratacoesUseCase _listarContratacoesUseCase;
    private readonly IObterContratacaoPorIdUseCase _obterContratacaoPorIdUseCase;
    private readonly ILogger<ContratacoesController> _logger;

    public ContratacoesController(
        IContratarPropostaUseCase contratarPropostaUseCase,
        IListarContratacoesUseCase listarContratacoesUseCase,
        IObterContratacaoPorIdUseCase obterContratacaoPorIdUseCase,
        ILogger<ContratacoesController> logger)
    {
        _contratarPropostaUseCase = contratarPropostaUseCase;
        _listarContratacoesUseCase = listarContratacoesUseCase;
        _obterContratacaoPorIdUseCase = obterContratacaoPorIdUseCase;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ContratacaoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Contratar([FromBody] ContratarPropostaRequest request)
    {
        try
        {
            var response = await _contratarPropostaUseCase.ExecuteAsync(request);
            return CreatedAtAction(nameof(ObterPorId), new { id = response.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao contratar proposta {PropostaId}", request.PropostaId);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ContratacaoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar()
    {
        var contratacoes = await _listarContratacoesUseCase.ExecuteAsync();
        return Ok(contratacoes);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ContratacaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var contratacao = await _obterContratacaoPorIdUseCase.ExecuteAsync(id);
        if (contratacao == null)
        {
            return NotFound(new { message = $"Contratacao {id} nao encontrada" });
        }

        return Ok(contratacao);
    }
}
