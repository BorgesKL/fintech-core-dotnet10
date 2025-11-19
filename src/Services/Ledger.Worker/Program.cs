using FintechCore.BuildingBlocks.Extensions;
using FintechCore.Ledger.Worker.Consumers;
using FintechCore.Ledger.Worker.Data;
using Serilog;
using System.Reflection;

var builder = Host.CreateApplicationBuilder(args);

// 1. Logs
builder.Services.AddSerilog(config => config.WriteTo.Console());

// 2. Registra o Inicializador de Banco como Singleton
builder.Services.AddSingleton<DbInitializer>();

// 3. MassTransit (Auto-discovery do LedgerConsumer)
builder.Services.AddCustomMassTransit(builder.Configuration, Assembly.GetExecutingAssembly());

var host = builder.Build();

// 4. Executa a inicialização do Banco antes de começar a processar mensagens
var dbInitializer = host.Services.GetRequiredService<DbInitializer>();
await dbInitializer.InitializeAsync();

host.Run();