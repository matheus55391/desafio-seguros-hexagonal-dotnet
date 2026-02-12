namespace ContratacaoService.Domain.Entities;

public class Contratacao
{
    public Guid Id { get; private set; }
    public Guid PropostaId { get; private set; }
    public DateTime DataContratacao { get; private set; }

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
