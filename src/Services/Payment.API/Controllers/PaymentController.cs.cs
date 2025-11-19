using FintechCore.BuildingBlocks.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace FintechCore.Payment.API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PaymentController> _logger;

    // Injetamos o IPublishEndpoint do MassTransit. 
    // Ele sabe rotear a mensagem para a fila certa baseada no tipo do evento.
    public PaymentController(IPublishEndpoint publishEndpoint, ILogger<PaymentController> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] PaymentRequest request)
    {
        // 1. Criação do ID da transação aqui na borda (Idempotência)
        var transactionId = Guid.NewGuid();

        _logger.LogInformation("Recebendo pagamento {Amount} de {From} para {To}", 
            request.Amount, request.FromAccountId, request.ToAccountId);

        // 2. Cria o evento (Contrato)
        var paymentEvent = new PaymentCreatedEvent(
            transactionId,
            request.FromAccountId,
            request.ToAccountId,
            request.Amount,
            DateTime.UtcNow
        );

        // 3. Publica no barramento (RabbitMQ)
        // O Controller não espera o processamento. Ele entrega pro RabbitMQ e devolve "Aceito".
        // Isso garante alta performance: a API não trava esperando banco de dados.
        await _publishEndpoint.Publish(paymentEvent);

        // Retorna 202 Accepted: "Recebi, mas ainda vou processar"
        return Accepted(new { TransactionId = transactionId, Status = "Processing" });
    }
}

// DTO simples para receber o JSON (pode ficar no mesmo arquivo por simplicidade agora)
public record PaymentRequest(Guid FromAccountId, Guid ToAccountId, decimal Amount);