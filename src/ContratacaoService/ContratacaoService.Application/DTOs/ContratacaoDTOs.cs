namespace ContratacaoService.Application.DTOs;

public record ContratarPropostaRequest(Guid PropostaId);

public record ContratacaoResponse(
    Guid Id,
    Guid PropostaId,
    DateTime DataContratacao
);

public record ContratacaoCriadaEvent(
    Guid ContratacaoId,
    Guid PropostaId,
    DateTime DataContratacao
);
