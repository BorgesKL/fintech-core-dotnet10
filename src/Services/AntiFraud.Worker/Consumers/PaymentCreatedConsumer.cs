using FintechCore.BuildingBlocks.Contracts;
using MassTransit;

namespace FintechCore.AntiFraud.Worker.Consumers;

public class PaymentCreatedConsumer : IConsumer<PaymentCreatedEvent>
{
    private readonly ILogger<PaymentCreatedConsumer> _logger;

    public PaymentCreatedConsumer(ILogger<PaymentCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation("🕵️ ANTIFRAUDE: Analisando transação {Id} de valor R$ {Amount}", 
            message.TransactionId, message.Amount);

        // Simulação de processamento (IO Bound)
        await Task.Delay(1000);

        if (message.Amount > 10000)
        {
            _logger.LogWarning("🚨 ALERTA: Transação {Id} suspeita! Valor muito alto.", message.TransactionId);
            // Aqui você lançaria um evento de "PaymentRejectedEvent" (Fica para o desafio futuro)
        }
        else
        {
            _logger.LogInformation("✅ APROVADO: Transação {Id} segura.", message.TransactionId);
            // Aqui você lançaria um evento de "PaymentApprovedEvent"
        }
    }
}