using ContratacaoService.Domain.Ports;

namespace ContratacaoService.Tests.Fakes;

internal sealed class FakeMessagePublisher : IMessagePublisher
{
    public bool Publicou { get; private set; }

    public Task PublishAsync<T>(string queue, T message) where T : class
    {
        Publicou = true;
        return Task.CompletedTask;
    }
}
