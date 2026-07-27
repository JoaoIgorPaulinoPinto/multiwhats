# Sistema de Auditoria

## Visão Geral

O sistema de auditoria rastreia automaticamente todas as alterações feitas no banco de dados, registrando quem fez, o que fez, e os valores antigos/novos.

---

## Fluxo

```
Requisição POST/PUT/PATCH/DELETE
        │
        ▼
AppDbContext.SaveChangesAsync()
        │
        ├── 1. ApplyAudit()
        │      ├── Seta CreatedByUserId / LastUpdatedByUserId
        │      ├── Seta CreatedAt / LastUpdate
        │      └── Converte hard delete → soft delete (IsDeleted = true)
        │
        └── 2. AuditService.LogChanges()
               ├── Compara ChangeTracker Entries
               ├── Gera JSON: OldValues / NewValues
               ├── Salva registro na tabela AuditLogs
               └── Escreve no console com cor
```

---

## AppDbContext — ApplyAudit()

Sobrescrito em `SaveChanges()` e `SaveChangesAsync()`:

| Operação | O que faz |
|---|---|
| **Insert** | Seta `CreatedAt`, `CreatedByUserId` (do JWT) |
| **Update** | Seta `LastUpdate`, `LastUpdatedByUserId` (do JWT) |
| **Delete** | Converte para `IsDeleted = true` + `LastUpdate` (nunca remove) |

---

## AuditService

Serviço que:
1. Itera sobre `ChangeTracker.Entries()` após `SaveChanges`
2. Para cada entidade alterada, extrai:
   - Nome da entidade
   - ID da entidade
   - Tipo de ação (Created/Updated/Deleted)
   - Valores antigos (para Update/Delete)
   - Valores novos (para Create/Update)
3. Cria registro `AuditLog` com:
   - `UserId`, `UserName`, `UserRole` (do contexto HTTP)
   - `EntityType`, `EntityId`, `Action`
   - `Description` (texto legível)
   - `OldValues` e `NewValues` (JSON serializado)
   - `Timestamp`
4. Salva no banco

### Exemplo de Output no Console

```
[AUDIT] 12:34:56 | User "Joao" (Admin) | Created Contact #5 | Maria - 5511999999999
[AUDIT] 12:35:10 | User "Maria" (Support) | Updated Occurrence #3 | Status: Open → InProgress
[AUDIT] 12:36:22 | User "Joao" (Admin) | Deleted Client #12 | Timontec
```

---

## UseCaseLogger

Complemento ao `AuditService` que:
- Registra ações de uso de forma manual
- Emite notificações via **SignalR** em tempo real para o frontend

---

## Tabela AuditLogs

| Campo | Tipo | Descrição |
|---|---|---|
| Id | int | PK |
| UserId | int? | ID do usuário |
| UserName | string? | Nome do usuário |
| UserRole | string? | Role do usuário |
| EntityType | string | Nome da entidade (`"Contact"`) |
| EntityId | int? | ID da entidade |
| Action | string | `Created` \| `Updated` \| `Deleted` |
| Description | string | Descrição legível |
| OldValues | string? | JSON com valores antigos |
| NewValues | string? | JSON com valores novos |
| Timestamp | DateTime | Data/hora da operação |

**Índices:** `Timestamp`, `EntityType`

---

## Campos Excluídos do Audit

O `AuditService` ignora campos sensíveis ao gerar logs:
- `Password` (usuário)
- `CreatedAt`, `LastUpdate` (auto-gerados)
- `IsDeleted` (soft delete flag)
- `CreatedByUserId`, `LastUpdatedByUserId` (auto-gerados)

---

## Exemplo de OldValues / NewValues

### Update de Occurrence

```json
{
  "oldValues": {
    "Status": "Open",
    "Priority": "Medium"
  },
  "newValues": {
    "Status": "InProgress",
    "Priority": "High"
  }
}
```

### Create de Contact

```json
{
  "oldValues": null,
  "newValues": {
    "Jid": "5511999999999@c.us",
    "PhoneNumber": "5511999999999",
    "Name": "Maria"
  }
}
```
