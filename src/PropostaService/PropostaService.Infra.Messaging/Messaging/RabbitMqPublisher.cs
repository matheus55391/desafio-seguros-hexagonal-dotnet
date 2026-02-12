using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PropostaService.Domain.Ports;
using PropostaService.Infra.Messaging.Options;
using RabbitMQ.Client;

namespace PropostaService.Infra.Messaging;

/// <summary>
/// Adapter que implementa o port IMessagePublisher - Arquitetura Hexagonal
/// </summary>
public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public RabbitMqPublisher(IOptions<RabbitMqOptions> options)
    {
        var settings = options?.Value ?? throw new ArgumentNullException(nameof(options));

        var factory = new ConnectionFactory
        {
            HostName = settings.HostName,
            Port = settings.Port,
            UserName = settings.UserName,
            Password = settings.Password
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
    }

    public async Task PublishAsync<T>(string queue, T message) where T : class
    {
        await _channel.QueueDeclareAsync(
            queue: queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        await _channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queue,
            body: body);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}
