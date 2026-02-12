namespace PropostaService.Application.DTOs;

public record CriarPropostaRequest(
    string NomeCliente,
    string CpfCliente,
    string TipoSeguro,
    decimal ValorCobertura,
    decimal ValorPremio
);

public record PropostaResponse(
    Guid Id,
    string NomeCliente,
    string CpfCliente,
    string TipoSeguro,
    decimal ValorCobertura,
    decimal ValorPremio,
    string Status,
    DateTime DataCriacao,
    DateTime? DataAtualizacao
);

public record AlterarStatusRequest(
    int NovoStatus
);

public record PropostaStatusAlteradoEvent(
    Guid PropostaId,
    int NovoStatus,
    DateTime DataAlteracao
);
