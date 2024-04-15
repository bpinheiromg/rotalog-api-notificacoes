# RotaLog - API Notificações

Microsserviço de notificações do sistema RotaLog. Responsável por enviar notificações (email, SMS) sobre eventos dos outros serviços.

## Tech Stack

- **.NET Core 6** com **ASP.NET Core Web API**
- **Entity Framework Core 6** com **Npgsql** (PostgreSQL)
- **MediatR 11** (configurado mas não utilizado - Clean Architecture abandonada)
- **Swagger/OpenAPI** para documentação

## Pré-requisitos

- .NET SDK 6.0+
- PostgreSQL rodando (via docker-compose do rotalog-workspace)

## Como rodar

```bash
# Subir o banco de dados
cd ../rotalog-workspace && docker-compose up -d postgres

# Executar migrations
psql -h localhost -U rotalog_admin -d rotalog -f Data/migration.sql

# Executar seed data (opcional)
psql -h localhost -U rotalog_admin -d rotalog -f Data/seed.sql

# Restaurar pacotes e rodar
dotnet restore
dotnet run
```

A API estará disponível em `http://localhost:5000`
Swagger UI em `http://localhost:5000/swagger`

## Endpoints

### Health Check

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/health` | Status do serviço e dependências (inclui acesso direto a schemas de outros serviços) |

### Notificações (`/api/notificacoes`)

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/notificacoes` | Listar com filtros (`tipo`, `status`, `canal`, `servicoOrigem`) |
| GET | `/api/notificacoes/{id}` | Buscar por ID |
| POST | `/api/notificacoes` | Criar e enviar notificação (endpoint principal para integrações) |
| POST | `/api/notificacoes/{id}/reenviar` | Reenviar notificação (com novo destinatário/canal opcional) |
| GET | `/api/notificacoes/stats` | Estatísticas (total, por tipo, por canal) |
| GET | `/api/notificacoes/templates` | Listar templates (`apenasAtivos=true`) |
| POST | `/api/notificacoes/processar` | Processar pendentes em batch |

## Tipos de Notificação

| Tipo | Origem | Descrição |
|------|--------|-----------|
| `VEICULO_CADASTRADO` | api-frotas | Novo veículo cadastrado |
| `VEICULO_DESATIVADO` | api-frotas | Veículo desativado |
| `MANUTENCAO_AGENDADA` | api-frotas | Manutenção agendada |
| `ALERTA_MANUTENCAO` | api-frotas | Alerta de manutenção (SMS) |
| `CNH_VENCIDA` | api-frotas | CNH de motorista vencida |
| `ENTREGA_CRIADA` | api-entregas | Nova entrega criada |
| `STATUS_ENTREGA` | api-entregas | Mudança de status |
| `ENTREGA_CONCLUIDA` | api-entregas | Entrega finalizada |
| `ENTREGA_ATRASADA` | api-entregas | Alerta de atraso |

## Canais Suportados

| Canal | Status | Observação |
|-------|--------|------------|
| `email` | Fake (simulado) | Loga no console, simula delay e falha aleatória (10%) |
| `sms` | Fake (simulado) | Loga no console, simula delay e falha aleatória (20%) |
| `push` | Não implementado | Configuração existe mas sem implementação |

## Templates

O sistema suporta templates com variáveis `{{placeholder}}` que são substituídas no momento do envio. Exemplo:

```json
{
    "tipo": "ENTREGA_CRIADA",
    "canal": "email",
    "destinatario": "operacao@rotalog.com",
    "mensagem": "",
    "servicoOrigem": "api-entregas",
    "referenciaId": "entrega-11",
    "variaveis": {
        "numero_pedido": "PED-2024-011",
        "origem": "Rua Augusta, 1500",
        "destino": "Av. Paulista, 2000"
    }
}
```

## Integração com outros serviços

### Quem chama este serviço:

| Serviço | Método | Endpoint | Quando |
|---------|--------|----------|--------|
| api-frotas | POST | `/api/notificacoes` | Cadastro de veículo, manutenção, CNH vencida |
| api-entregas | POST | `/api/notificacoes` | Criação de entrega, mudança de status, conclusão |

### Violação de bounded context (dívida técnica):

O health check (`/api/health`) acessa diretamente os schemas `frotas` e `entregas` no banco de dados, violando o princípio de bounded context. Deveria chamar as APIs dos respectivos serviços.

## Exemplos com curl

### Criar notificação simples

```bash
curl -X POST http://localhost:5000/api/notificacoes \
  -H "Content-Type: application/json" \
  -d '{
    "tipo": "VEICULO_CADASTRADO",
    "canal": "email",
    "destinatario": "operacao@rotalog.com",
    "mensagem": "Novo veículo cadastrado: XYZ9A99",
    "servicoOrigem": "api-frotas",
    "referenciaId": "veiculo-11"
  }'
```

### Criar notificação com template

```bash
curl -X POST http://localhost:5000/api/notificacoes \
  -H "Content-Type: application/json" \
  -d '{
    "tipo": "ENTREGA_CRIADA",
    "canal": "email",
    "destinatario": "operacao@rotalog.com",
    "mensagem": "",
    "servicoOrigem": "api-entregas",
    "variaveis": {
      "numero_pedido": "PED-2024-011",
      "origem": "Rua Augusta, 1500",
      "destino": "Av. Paulista, 2000"
    }
  }'
```

### Processar pendentes

```bash
curl -X POST http://localhost:5000/api/notificacoes/processar
```

## Testes via HTTP

O arquivo `requests.http` contém todos os requests prontos para testar com o REST Client do VS Code. Inclui:
- CRUD completo de notificações
- Criação com mensagem direta e com templates
- Simulação de chamadas do api-frotas e api-entregas
- Reenvio e processamento batch
- Fluxo completo de integração

## Dívida Técnica Conhecida

- [ ] Clean Architecture abandonada no meio (MediatR configurado mas não usado)
- [ ] God class: NotificacaoService faz tudo (CRUD, envio, template, retry)
- [ ] Sem interface para NotificacaoService (impossível mockar)
- [ ] Credenciais SMTP e SMS API key no appsettings.json
- [ ] Connection string com senha no appsettings.json
- [ ] Health check acessa schemas de outros serviços diretamente
- [ ] Envio de email e SMS é fake (simula com delay)
- [ ] Falha aleatória simulada no envio
- [ ] Status como string em vez de enum
- [ ] Sem validação com FluentValidation
- [ ] Sem AutoMapper (mapeamento manual)
- [ ] Sem paginação nos endpoints de listagem
- [ ] Sem cache
- [ ] Sem rate limiting
- [ ] Sem autenticação
- [ ] CORS permite todas as origens
- [ ] EnsureCreated em vez de migrations do EF Core
- [ ] Processamento batch como endpoint em vez de job agendado
- [ ] Template engine com String.Replace simples
- [ ] Sem circuit breaker para envio
- [ ] Sem dead letter queue
- [ ] Sem testes unitários ou de integração
- [ ] Swagger exposto em todos os ambientes
- [ ] Expõe detalhes de erro interno nos responses
