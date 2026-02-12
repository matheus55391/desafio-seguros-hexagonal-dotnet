using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PropostaService.Domain.Ports;
using PropostaService.Infra.Messaging.Options;

namespace PropostaService.Infra.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddMessagingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

        return services;
    }
}
