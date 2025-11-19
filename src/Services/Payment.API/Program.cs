using FintechCore.BuildingBlocks.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração de Logs (Essencial para produção)
builder.Host.UseSerilog((context, config) => 
    config.WriteTo.Console());

// 2. Adiciona Controllers e Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. AQUI ESTÁ A MÁGICA: Adiciona o MassTransit configurado
// Como a API só *envia* mensagens (não tem consumers), não passamos assembly.
builder.Services.AddCustomMassTransit(builder.Configuration);

var app = builder.Build();

// Configura o pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();