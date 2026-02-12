using Microsoft.AspNetCore.Mvc;
using PropostaService.Application.DTOs;
using PropostaService.Application.Interfaces;

namespace PropostaService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropostasController : ControllerBase
{
    private readonly ICriarPropostaUseCase _criarPropostaUseCase;
    private readonly IListarPropostasUseCase _listarPropostasUseCase;
    private readonly IObterPropostaPorIdUseCase _obterPropostaPorIdUseCase;
    private readonly IAlterarStatusPropostaUseCase _alterarStatusPropostaUseCase;
    private readonly ILogger<PropostasController> _logger;

    public PropostasController(
        ICriarPropostaUseCase criarPropostaUseCase,
        IListarPropostasUseCase listarPropostasUseCase,
        IObterPropostaPorIdUseCase obterPropostaPorIdUseCase,
        IAlterarStatusPropostaUseCase alterarStatusPropostaUseCase,
        ILogger<PropostasController> logger)
    {
        _criarPropostaUseCase = criarPropostaUseCase;
        _listarPropostasUseCase = listarPropostasUseCase;
        _obterPropostaPorIdUseCase = obterPropostaPorIdUseCase;
        _alterarStatusPropostaUseCase = alterarStatusPropostaUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Criar uma nova proposta de seguro
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PropostaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CriarProposta([FromBody] CriarPropostaRequest request)
    {
        try
        {
            var response = await _criarPropostaUseCase.ExecuteAsync(request);
            return CreatedAtAction(nameof(ObterPorId), new { id = response.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar proposta");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Listar todas as propostas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PropostaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPropostas()
    {
        var propostas = await _listarPropostasUseCase.ExecuteAsync();
        return Ok(propostas);
    }

    /// <summary>
    /// Obter proposta por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PropostaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var proposta = await _obterPropostaPorIdUseCase.ExecuteAsync(id);
        
        if (proposta == null)
        {
            return NotFound(new { message = $"Proposta {id} nao encontrada" });
        }

        return Ok(proposta);
    }

    /// <summary>
    /// Alterar status de uma proposta
    /// Status: 1-EmAnalise, 2-Aprovada, 3-Rejeitada, 4-Contratada
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(PropostaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AlterarStatus(Guid id, [FromBody] AlterarStatusRequest request)
    {
        try
        {
            var response = await _alterarStatusPropostaUseCase.ExecuteAsync(id, request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operacao invalida ao alterar status da proposta {PropostaId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar status da proposta {PropostaId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }
}
