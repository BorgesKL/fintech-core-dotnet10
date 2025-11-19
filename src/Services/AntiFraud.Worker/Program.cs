using FintechCore.AntiFraud.Worker.Consumers;
using FintechCore.BuildingBlocks.Extensions;
using Serilog;
using System.Reflection;

var builder = Host.CreateApplicationBuilder(args);

// 1. Configuração de Logs
builder.Services.AddSerilog(config => config.WriteTo.Console());

// 2. AQUI ESTÁ A MÁGICA (De novo):
// Diferente da API, aqui passamos o Assembly atual. 
// A extensão vai achar o 'PaymentCreatedConsumer' automaticamente.
builder.Services.AddCustomMassTransit(builder.Configuration, Assembly.GetExecutingAssembly());

var host = builder.Build();
host.Run();