# Plataforma de Seguros - Microserviços

Sistema de gerenciamento de propostas e contratações usando **Arquitetura Hexagonal**, **DDD** e **Clean Architecture**.

## 🛠️ Stack

- .NET 10
- PostgreSQL 16
- RabbitMQ 4
- Entity Framework Core
- Docker

---

## 🚀 Como Executar

### 🐳 Opção 1: Docker Full (Produção)

Tudo containerizado (PostgreSQL + RabbitMQ + APIs):

```bash
docker compose up -d --build
```

**Portas:**
- PropostaService: http://localhost:5001/swagger
- ContratacaoService: http://localhost:5002/swagger
- RabbitMQ: http://localhost:15672 (guest/guest)

**Parar:**
```bash
docker compose down -v
```

---

### 💻 Opção 2: Infra Docker + APIs Local (Desenvolvimento)

Apenas PostgreSQL e RabbitMQ no Docker, APIs rodando localmente:

```bash
# 1. Subir infra
docker compose -f docker-compose.infra.yml up -d
```

**Depois escolha:**

**A) Visual Studio (Run All):**
1. Botão direito na Solution → Properties
2. Multiple Startup Projects
3. Marcar PropostaService.API e ContratacaoService.API como "Start"
4. Pressionar F5

**B) Linha de comando:**
```bash
# Terminal 1
cd src/PropostaService/PropostaService.API
dotnet run

# Terminal 2
cd src/ContratacaoService/ContratacaoService.API
dotnet run
```

**Portas (local):**
- PropostaService: http://localhost:5001/swagger (HTTPS: 7001)
- ContratacaoService: http://localhost:5002/swagger (HTTPS: 7002)
- RabbitMQ: http://localhost:15672

**Parar infra:**
```bash
docker compose -f docker-compose.infra.yml down
```

---

## 🧪 Build & Testes

```bash
# Build
dotnet build

# Testes unitários (7 testes)
dotnet test
```

---

## 📝 Testando o Fluxo

### 1️⃣ Criar Proposta
```http
POST http://localhost:5001/api/propostas
Content-Type: application/json

{
  "nomeCliente": "Maria Silva",
  "cpfCliente": "12345678901",
  "tipoSeguro": "Auto",
  "valorCobertura": 150000,
  "valorPremio": 350
}
```

### 2️⃣ Aprovar Proposta
```http
PATCH http://localhost:5001/api/propostas/{id}/status
Content-Type: application/json

{ "novoStatus": 2 }
```
**Status:** 1=EmAnalise | 2=Aprovada | 3=Rejeitada | 4=Contratada

### 3️⃣ Contratar Proposta
```http
POST http://localhost:5002/api/contratacoes
Content-Type: application/json

{ "propostaId": "{id-da-proposta-aprovada}" }
```

---

## 📁 Estrutura do Projeto

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
    ├── Infra.ExternalServices/  # HTTP Client (chama PropostaService)
    ├── Infra.Messaging/
    └── Infra.IoC/

tests/
├── PropostaService.Tests/       # 4 testes
└── ContratacaoService.Tests/    # 3 testes
```

---

## 🏗️ Arquitetura

- **Arquitetura Hexagonal** (Ports & Adapters)
- **DDD** (Bounded Contexts: Proposta + Contratação)
- **Clean Architecture** (Domain independente de infra)
- **SOLID** 
- **Microserviços** (2 serviços independentes)
- **Comunicação:** HTTP (sync) + RabbitMQ (async)

---

## ✅ Requisitos Implementados

### Obrigatórios
✅ Arquitetura Hexagonal  
✅ Microserviços  
✅ PostgreSQL (banco relacional)  
✅ Comunicação HTTP REST  
✅ DDD  
✅ Clean Architecture  
✅ SOLID  
✅ Testes unitários (7 testes)  
✅ .NET 10  

### BONUS
✅ Mensageria (RabbitMQ)  
✅ Docker Compose
