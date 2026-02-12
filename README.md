# Plataforma de Seguros - Microserviços

Sistema de gerenciamento de propostas e contratações implementado com **Arquitetura Hexagonal**, **DDD** e **Clean Architecture**.

## Stack Tecnológica

- .NET 10
- PostgreSQL 16
- RabbitMQ 4
- Entity Framework Core
- Docker & Docker Compose
- xUnit

---

## Execução

### Opção 1: Docker Compose (Completo)

Inicia toda a infraestrutura e aplicações containerizadas:

```bash
docker compose up -d --build
```

**Endpoints disponíveis:**
- PropostaService API: http://localhost:5001/swagger
- ContratacaoService API: http://localhost:5002/swagger
- RabbitMQ Management: http://localhost:15672 (usuário: guest, senha: guest)

Para encerrar e remover volumes:
```bash
docker compose down -v
```

---

### Opção 2: Desenvolvimento Local

Executa apenas a infraestrutura via Docker, mantendo as APIs em ambiente local:

```bash
docker compose -f docker-compose.infra.yml up -d
```

**Executar APIs:**

Via Visual Studio (Multiple Startup Projects):
- Configurar `PropostaService.API` e `ContratacaoService.API` como startup projects

Via CLI:
```bash
# Terminal 1 - PropostaService
cd src/PropostaService/PropostaService.API
dotnet run

# Terminal 2 - ContratacaoService
cd src/ContratacaoService/ContratacaoService.API
dotnet run
```

**Endpoints locais:**
- PropostaService: http://localhost:5001 (HTTPS: 7001)
- ContratacaoService: http://localhost:5002 (HTTPS: 7002)
- RabbitMQ: http://localhost:15672

Encerrar infraestrutura:
```bash
docker compose -f docker-compose.infra.yml down
```

---

## Testes

```bash
dotnet build
dotnet test
```

**Cobertura:** 25 testes unitários (19 PropostaService + 6 ContratacaoService)

---

## API Reference

Documentação interativa via Swagger UI:

- **PropostaService:** http://localhost:5001/swagger
- **ContratacaoService:** http://localhost:5002/swagger

### Fluxo de Contratação

1. `POST /api/propostas` - Criar proposta (status inicial: `EmAnalise`)
2. `PATCH /api/propostas/{id}/status` - Alterar status (`novoStatus: 2` para aprovar)
3. `POST /api/contratacoes` - Criar contratação (requer proposta aprovada)

---

## Estrutura do Projeto

```
src/
├── PropostaService/
│   ├── API/                  # Controllers + Program.cs
│   ├── Application/          # Use Cases + DTOs
│   ├── Domain/               # Entidades + Ports (interfaces)
│   ├── Infra.Data/           # PostgreSQL + EF Core + Migrations
│   ├── Infra.Messaging/      # RabbitMQ Publisher
│   └── Infra.IoC/            # Dependency Injection
│
└── ContratacaoService/
    ├── API/
    ├── Application/
    ├── Domain/
    ├── Infra.Data/
    ├── Infra.ExternalServices/  # HTTP Client para PropostaService
    ├── Infra.Messaging/
    └── Infra.IoC/

tests/
├── PropostaService.Tests/       # 19 testes unitários
└── ContratacaoService.Tests/    # 6 testes unitários
```

---

## Arquitetura

**Padrões:**
- Arquitetura Hexagonal (Ports & Adapters)
- Domain-Driven Design (Bounded Contexts isolados)
- Clean Architecture (Domain sem dependências de infraestrutura)
- SOLID Principles

**Comunicação entre serviços:**
- **Síncrona:** HTTP REST (ContratacaoService → PropostaService)
- **Assíncrona:** RabbitMQ (event-driven messaging)

---

## Documentação Técnica

### Enumerações

#### StatusProposta

Define os estados do ciclo de vida de uma proposta de seguro.

```csharp
public enum StatusProposta
{
    EmAnalise = 1,    // Estado inicial ao criar proposta
    Aprovada = 2,     // Proposta aprovada pela análise de risco
    Rejeitada = 3,    // Proposta rejeitada pela análise
    Contratada = 4    // Proposta efetivada através de contratação
}
```

**Namespace:**
- `PropostaService.Domain.Entities.StatusProposta`
- `ContratacaoService.Domain.Entities.StatusProposta`

**Regras de Negócio:**
- Status inicial: `EmAnalise` (1)
- Apenas propostas com status `Aprovada` (2) são elegíveis para contratação
- Após contratação bem-sucedida, status atualizado para `Contratada` (4)
- Transição de estados gerenciada por `AlterarStatusPropostaUseCase`
- Alterações de status disparam eventos assíncronos via RabbitMQ

---

### Mensageria (RabbitMQ)

O sistema utiliza mensageria assíncrona para desacoplamento entre serviços. As filas são criadas automaticamente com as seguintes características:
- **Durabilidade:** `durable: true` (sobrevivem a reinicializações)
- **Exclusividade:** `exclusive: false` (acessíveis por múltiplas conexões)
- **Auto-delete:** `autoDelete: false` (persistem mesmo sem consumidores)

**Gerenciamento:** Classes estáticas `FilaMensagem` centralizam os nomes das filas em cada serviço.

---

#### Fila: proposta-status-alterado

**Publisher:** PropostaService  
**Consumer:** Não implementado (arquitetura preparada para sistemas downstream)  
**Use Case:** `AlterarStatusPropostaUseCase`

**Estrutura da Mensagem:**
```json
{
  "propostaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "novoStatus": 2,
  "dataAlteracao": "2026-02-12T10:30:00.000Z"
}
```

**Event Model:** `PropostaStatusAlteradoEvent`  
**Namespace:** `PropostaService.Application.DTOs`

**Trigger:** Disparado após atualização bem-sucedida do status no repositório.

**Casos de uso downstream:**
- Auditoria e logging de mudanças
- Notificações ao cliente (email/SMS)
- Dashboards de monitoramento em tempo real
- Integrações com sistemas legados

**Constante:**
```csharp
// PropostaService.Domain.Entities.FilaMensagem
public const string PropostaStatusAlterado = "proposta-status-alterado";
```

---

#### Fila: contratacao-criada

**Publisher:** ContratacaoService  
**Consumer:** Não implementado (arquitetura preparada para processos posteriores)  
**Use Case:** `ContratarPropostaUseCase`

**Estrutura da Mensagem:**
```json
{
  "contratacaoId": "8e5a9c21-4b3d-4f2a-9c7e-1d8f6e3b5a72",
  "propostaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "dataContratacao": "2026-02-12T11:00:00.000Z"
}
```

**Event Model:** `ContratacaoCriadaEvent`  
**Namespace:** `ContratacaoService.Application.DTOs`

**Trigger:** Disparado após:
1. Validação de status da proposta (deve ser `Aprovada`)
2. Persistência da contratação no banco de dados
3. Atualização do status da proposta para `Contratada`

**Casos de uso downstream:**
- Emissão de apólice de seguro
- Processamento de pagamento
- Notificação ao cliente sobre contratação bem-sucedida
- Sincronização com sistemas de faturamento
- Acionamento de workflows de onboarding

**Constante:**
```csharp
// ContratacaoService.Domain.Entities.FilaMensagem
public const string ContratacaoCriada = "contratacao-criada";
```

---

### Configuração RabbitMQ

**Parâmetros de conexão (appsettings.json):**
```json
{
  "RabbitMq": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest"
  }
}
```

**Management Console:** http://localhost:15672  
Permite monitoramento de filas, mensagens, conexões, consumers e métricas de performance.

---

### Ports (Hexagonal Architecture)

Interfaces que definem contratos entre camadas, permitindo inversão de dependência.

**PropostaService.Domain.Ports:**
- `IPropostaRepository` - Contrato para persistência de propostas
- `IMessagePublisher` - Contrato para publicação de eventos assíncronos

**ContratacaoService.Domain.Ports:**
- `IContratacaoRepository` - Contrato para persistência de contratações
- `IPropostaServiceClient` - Contrato para comunicação síncrona com PropostaService
- `IMessagePublisher` - Contrato para publicação de eventos assíncronos

---

## Requisitos Implementados

### Obrigatórios
✅ Arquitetura Hexagonal (Ports & Adapters pattern)  
✅ Microserviços com bounded contexts isolados  
✅ PostgreSQL como banco de dados relacional  
✅ Comunicação HTTP REST entre serviços  
✅ Domain-Driven Design (DDD)  
✅ Clean Architecture  
✅ Princípios SOLID  
✅ Cobertura de testes unitários (25 testes)  
✅ .NET 10  

### Diferenciais
✅ Mensageria assíncrona com RabbitMQ  
✅ Docker Compose para orquestração de containers  
✅ Event-Driven Architecture  
✅ Documentação OpenAPI/Swagger  
✅ Separação de bancos de dados por serviço  
✅ Injeção de dependências modular  

---

## Licença

Projeto desenvolvido como desafio técnico.
