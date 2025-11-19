using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FintechCore.BuildingBlocks.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddCustomMassTransit(
        this IServiceCollection services, 
        IConfiguration configuration, 
        Assembly? assemblyWithConsumers = null)
    {
        services.AddMassTransit(busConfig =>
        {
            // 1. Se houver Consumers (Workers), registra eles automaticamente
            if (assemblyWithConsumers != null)
            {
                busConfig.AddConsumers(assemblyWithConsumers);
            }

            // 2. Configura a conexão com o RabbitMQ usando o appsettings.json
            busConfig.UsingRabbitMq((context, rabbitConfig) =>
            {
                rabbitConfig.Host(configuration["RabbitMq:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMq:Username"] ?? "guest");
                    h.Password(configuration["RabbitMq:Password"] ?? "guest");
                });

                // Isso garante nomes de filas legíveis e organizados
                rabbitConfig.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}