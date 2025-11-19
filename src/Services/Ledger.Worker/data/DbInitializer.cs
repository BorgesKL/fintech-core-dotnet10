using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FintechCore.Ledger.Worker.Data;

public class DbInitializer
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(IConfiguration configuration, ILogger<DbInitializer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        // 1. Manipular a string de conexão para apontar para o 'master'
        // Precisamos conectar no master para ter permissão de criar um novo banco
        var builder = new SqlConnectionStringBuilder(connectionString);
        var originalDatabase = builder.InitialCatalog; // Salva o nome "FintechDb"
        builder.InitialCatalog = "master"; // Muda para "master"
        
        using var masterConnection = new SqlConnection(builder.ConnectionString);
        
        try 
        {
            // Cria o DATABASE se não existir
            var createDbSql = $@"
                IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{originalDatabase}')
                BEGIN
                    CREATE DATABASE [{originalDatabase}];
                END";
            
            await masterConnection.ExecuteAsync(createDbSql);
            _logger.LogInformation($"✅ Banco de dados '{originalDatabase}' garantido.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao criar o banco de dados.");
            throw; // Se falhar aqui, nem adianta continuar
        }

        // 2. Agora conectamos no banco correto (FintechDb) para criar as tabelas
        using var connection = new SqlConnection(connectionString);

        var createTableSql = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Transactions' AND xtype='U')
            BEGIN
                CREATE TABLE Transactions (
                    Id UNIQUEIDENTIFIER PRIMARY KEY,
                    FromAccountId UNIQUEIDENTIFIER NOT NULL,
                    ToAccountId UNIQUEIDENTIFIER NOT NULL,
                    Amount DECIMAL(18, 2) NOT NULL,
                    CreatedAt DATETIME NOT NULL
                )
                PRINT 'Tabela Transactions criada com sucesso!'
            END";

        try
        {
            await connection.ExecuteAsync(createTableSql);
            _logger.LogInformation("✅ Tabela Transactions verificada/criada.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao criar tabelas.");
        }
    }
}