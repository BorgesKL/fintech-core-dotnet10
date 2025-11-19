# 🚀 FintechCore - Sistema de Pagamentos Distribuído (.NET 10)

![.NET](https://img.shields.io/badge/.NET-10.0-purple)
![Docker](https://img.shields.io/badge/Docker-Enabled-blue)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-Messaging-orange)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Persistence-red)

Este projeto é uma Prova de Conceito (PoC) de uma arquitetura de microsserviços moderna, resiliente e escalável, desenvolvida para processar pagamentos. O sistema demonstra o uso de **Arquitetura Orientada a Eventos (EDA)** e o padrão **Pub/Sub**.

---

## 📐 Como Funciona (Arquitetura)

O sistema foi desenhado para ser desacoplado. A API não processa o pagamento diretamente; ela apenas recebe a intenção e avisa os trabalhadores (Workers) via mensagem.

 ` ```mermaid `
 
graph LR
    
    Client[Cliente/Swagger] -- POST Request --> API[Payment.API]
    API -- Publica Evento --> Bus[(RabbitMQ)]
    Bus -- Fan-Out --> AntiFraud[AntiFraud.Worker]
    Bus -- Fan-Out --> Ledger[Ledger.Worker]
    Ledger -- Grava Transação --> DB[(SQL Server)]

Fluxo de Dados:

    Payment.API: Recebe o pedido HTTP, valida e publica o evento PaymentCreatedEvent. Retorna 202 Accepted imediatamente (Fire-and-Forget).

    RabbitMQ: Distribui a mensagem para todas as filas interessadas (Fan-Out).

    AntiFraud.Worker: Consome a mensagem para analisar riscos (Simulação: Valores > 10k geram alerta).

    Ledger.Worker: Consome a mesma mensagem para persistir os dados financeiramente usando Dapper.

🛠️ Tecnologias e Dependências

Para rodar este projeto, você utiliza a stack mais moderna do mercado .NET:

    Core: .NET 10 (C# 13/14).

    Mensageria: MassTransit (Abstração robusta para RabbitMQ).

    Banco de Dados: SQL Server 2022 (Imagem Docker oficial).

    ORM: Dapper (Micro-ORM focado em alta performance).

    Infraestrutura: Docker & Docker Compose.

    Logs: Serilog.

📋 Pré-requisitos

Antes de começar, certifique-se de ter instalado:

    .NET SDK (Versão 10 ou a mais recente disponível).

    Docker Desktop (Essencial para rodar o RabbitMQ e o SQL Server).

🚀 Como Rodar o Projeto

Passo 1: Subir a Infraestrutura (Docker)

Abra o terminal na raiz do projeto (onde está o arquivo docker-compose.yml) e execute:
Bash

docker-compose up -d

Isso iniciará os containers do RabbitMQ e do SQL Server em segundo plano.

Passo 2: Executar os Microsserviços

Recomenda-se abrir 3 terminais diferentes para visualizar os logs simultâneos:

Terminal 1 - API (Produtor):
Bash

dotnet run --project src/Services/Payment.API/FintechCore.Payment.API.csproj

Terminal 2 - Antifraude (Consumidor):
Bash

dotnet run --project src/Services/AntiFraud.Worker/FintechCore.AntiFraud.Worker.csproj

Terminal 3 - Contabilidade (Consumidor/Banco):
Bash

dotnet run --project src/Services/Ledger.Worker/FintechCore.Ledger.Worker.csproj

(Nota: O Ledger contém um inicializador automático que criará o banco FintechDb e a tabela na primeira execução).

🧪 Como Testar

    Acesse o Swagger: Abra o navegador no link exibido no Terminal 1 (geralmente http://localhost:5xxx/swagger).

    Dispare uma Transação: Use o endpoint POST /api/payments com o seguinte JSON:
    JSON

{
  "fromAccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "toAccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "amount": 150.00
}

Verifique os Resultados:

    API: Deve retornar HTTP 202.

    AntiFraud: Log no terminal: ✅ APROVADO: Transação...

    Ledger: Log no terminal: 💰 LEDGER: Transação salva...

Teste de Fraude: Envie um valor acima de 10000. O AntiFraud deve logar 🚨 ALERTA.

Verificar no Banco de Dados (Opcional): Execute este comando no terminal para consultar a tabela direto do Docker:
Bash

    docker exec -it fintech-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Fintech@2025!" -C -Q "SELECT * FROM FintechDb.dbo.Transactions"

📂 Estrutura de Pastas

Plaintext

FintechCore/
├── src/
│   ├── BuildingBlocks/       # Contratos (Eventos) e Configurações comuns
│   ├── Services/
│   │   ├── Payment.API/      # Web API (Entrada)
│   │   ├── AntiFraud.Worker/ # Worker de Validação
│   │   └── Ledger.Worker/    # Worker de Banco de Dados
├── docker-compose.yml        # Orquestração
└── FintechCore.sln           # Solução .NET
