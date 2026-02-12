namespace ContratacaoService.Domain.Ports;

public interface IMessagePublisher
{
    Task PublishAsync<T>(string queue, T message) where T : class;
}
