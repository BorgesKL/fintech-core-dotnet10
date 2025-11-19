using Dapper;
using FintechCore.BuildingBlocks.Contracts;
using MassTransit;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FintechCore.Ledger.Worker.Consumers;

public class LedgerConsumer : IConsumer<PaymentCreatedEvent>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LedgerConsumer> _logger;

    public LedgerConsumer(IConfiguration configuration, ILogger<LedgerConsumer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentCreatedEvent> context)
    {
        var message = context.Message;
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        var sql = @"
            INSERT INTO Transactions (Id, FromAccountId, ToAccountId, Amount, CreatedAt)
            VALUES (@TransactionId, @FromAccountId, @ToAccountId, @Amount, @CreatedAt)";

        using var connection = new SqlConnection(connectionString);
        
        // O Dapper mapeia as propriedades do objeto 'message' para os parâmetros @ do SQL automaticamente
        await connection.ExecuteAsync(sql, message);

        _logger.LogInformation("💰 LEDGER: Transação {Id} salva no banco com sucesso!", message.TransactionId);
    }
}