namespace PropostaService.Domain.Entities;

public class Proposta
{
    public Guid Id { get; private set; }
    public string NomeCliente { get; private set; } = null!;
    public string CpfCliente { get; private set; } = null!;
    public string TipoSeguro { get; private set; } = null!;
    public decimal ValorCobertura { get; private set; }
    public decimal ValorPremio { get; private set; }
    public StatusProposta Status { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }

    private Proposta() { } // EF Core

    public Proposta(string nomeCliente, string cpfCliente, string tipoSeguro, decimal valorCobertura, decimal valorPremio)
    {
        Id = Guid.NewGuid();
        NomeCliente = nomeCliente ?? throw new ArgumentNullException(nameof(nomeCliente));
        CpfCliente = cpfCliente ?? throw new ArgumentNullException(nameof(cpfCliente));
        TipoSeguro = tipoSeguro ?? throw new ArgumentNullException(nameof(tipoSeguro));
        ValorCobertura = valorCobertura > 0 ? valorCobertura : throw new ArgumentException("Valor de cobertura deve ser maior que zero");
        ValorPremio = valorPremio > 0 ? valorPremio : throw new ArgumentException("Valor do premio deve ser maior que zero");
        Status = StatusProposta.EmAnalise;
        DataCriacao = DateTime.UtcNow;
    }

    public void AlterarStatus(StatusProposta novoStatus)
    {
        if (!PodeAlterarPara(novoStatus))
        {
            throw new InvalidOperationException($"Nao e possivel alterar de {Status} para {novoStatus}");
        }

        Status = novoStatus;
        DataAtualizacao = DateTime.UtcNow;
    }

    private bool PodeAlterarPara(StatusProposta novoStatus)
    {
        return Status switch
        {
            StatusProposta.EmAnalise => novoStatus is StatusProposta.Aprovada or StatusProposta.Rejeitada,
            StatusProposta.Aprovada => novoStatus == StatusProposta.Contratada,
            StatusProposta.Rejeitada => false,
            StatusProposta.Contratada => false,
            _ => false
        };
    }

    public void MarcarComoContratada()
    {
        if (Status != StatusProposta.Aprovada)
        {
            throw new InvalidOperationException("Apenas propostas aprovadas podem ser contratadas");
        }
        AlterarStatus(StatusProposta.Contratada);
    }
}
