namespace ContratacaoService.Domain.Entities;

public class Contratacao
{
    public Guid Id { get; }
    public Guid PropostaId { get; }
    public DateTime DataContratacao { get; }

    private Contratacao() { }

    public Contratacao(Guid propostaId)
    {
        if (propostaId == Guid.Empty)
        {
            throw new ArgumentException("PropostaId invalido", nameof(propostaId));
        }

        Id = Guid.NewGuid();
        PropostaId = propostaId;
        DataContratacao = DateTime.UtcNow;
    }
}
