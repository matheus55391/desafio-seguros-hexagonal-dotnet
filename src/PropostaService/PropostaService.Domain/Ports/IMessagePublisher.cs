namespace PropostaService.Domain.Ports;

/// <summary>
/// Port (interface) para publicacao de mensagens - parte da arquitetura hexagonal
/// </summary>
public interface IMessagePublisher
{
    Task PublishAsync<T>(string queue, T message) where T : class;
}
