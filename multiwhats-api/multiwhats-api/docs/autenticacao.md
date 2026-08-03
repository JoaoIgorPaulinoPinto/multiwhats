# Autenticação e Autorização

## JWT Bearer Authentication

### Configuração

| Parâmetro | Valor |
|---|---|
| Scheme | `JwtBearerDefaults.AuthenticationScheme` |
| Algoritmo | HMAC-SHA256 |
| Issuer | `MinhaApiEmissor` |
| Audience | `MeuAppCliente` |
| Expiração | `JwtSettings.ExpiryInMinutes` (60 min) |
| Clock Skew | `TimeSpan.Zero` |
| RequireHttpsMetadata | `false` (desenvolvimento) |

### Claims no Token

| Claim | Conteúdo |
|---|---|
| `ClaimTypes.NameIdentifier` | ID do usuário (ex: `"1"`) |
| `ClaimTypes.Name` | Nome do usuário (ex: `"Joao"`) |
| `ClaimTypes.Role` | Role (ex: `"Support"`, `"Dev"`, `"Admin"`) |
| `jti` | ID único do token (GUID, usado na blacklist) |
| `iat` | Timestamp de emissão |

### Fluxo de Login

```
1. POST /api/auth/login { name, password }
2. LoginUseCase valida credenciais (BCrypt.Verify; senhas legadas em texto puro
   são re-hasheadas no primeiro login)
3. Verifica IsActive
4. TokenService.GenerateToken(user) → gera JWT (expira conforme ExpiryInMinutes)
5. Retorna { token, user }
6. Cliente usa: Authorization: Bearer {token}
7. OnTokenValidated relê role/status do usuário no banco a cada request
```

### Registro com Código de Permissão

```
1. Admin/Dev → POST /api/auth/codes (Admin,Dev) → gera código hex (64 bytes), expira em Auth:RegistrationCodeExpiryHours
2. Usuário → POST /api/auth/register { name, password, registrationCode? }
   → RegisterUserUseCase normaliza (Trim().ToUpperInvariant()), valida o código
     (existente, não usado, não expirado), marca como usado e cria o usuário
     com role padrão (Support) e senha BCrypt
3. Código expirado/usado/inexistente → 400 com mensagem
```

---

## Autorização por Role

| Role | Rotas Admin/Dev | Demais endpoints |
|---|---|---|
| Admin | ✅ Permitido | ✅ Permitido |
| Dev | ✅ Permitido | ✅ Permitido |
| Support | ❌ Negado | ✅ Permitido |

Rotas restritas a **Admin/Dev**:
- `PATCH /api/tasks/{id}/status`
- `POST /api/auth/codes` (gerar código de permissão)
- `PUT /api/users/{id}` (editar usuário)
- `GET /api/admin/config` + `PUT /api/admin/config/{key}` + `POST /api/admin/config/reload`

### Endpoints Anônimos (sem auth)

| Rota | Descrição |
|---|---|
| `POST /api/auth/register` | Registro |
| `POST /api/auth/login` | Login |
| `POST /api/webhook/whatsapp` | Webhook do Node.js (mensagens) |
| `POST /api/webhook/status` | Webhook do Node.js (status de entrega) |
| `POST /api/device` | Salvar dispositivo |
| `GET /api/device` | Obter dispositivo |

### Implementação

```csharp
// Endpoints que exigem auth:
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase { ... }

// Endpoint que exige roles específicas:
[HttpPatch("{id}/status")]
[Authorize(Roles = "Admin,Dev")]
public async Task<IActionResult> UpdateStatus(...) { ... }

// Endpoints anônimos:
[AllowAnonymous]
[HttpPost("whatsapp")]
public async Task<IActionResult> Webhook(...) { ... }
```

---

## Token Blacklist (Logout)

### Mecanismo

- **Serviço:** `TokenBlacklistService`
- **Armazenamento:** `ConcurrentDictionary` (in-memory)
- Ao fazer logout, o `jti` do token é adicionado à blacklist
- A cada requisição, o evento `OnTokenValidated` verifica a blacklist

### Limitações

- ⚠️ Blacklist é **in-memory** — perdida ao reiniciar o servidor
- ⚠️ Método `Cleanup()` existe mas **não é chamado automaticamente**
- ⚠️ Não persiste entre instâncias (problema em ambientes com múltiplos pods)

### Fluxo

```
1. POST /api/auth/logout (com token válido)
2. Extrai claim "jti" do token
3. Adiciona jti ao ConcurrentDictionary (blacklist)
4. Próxima requisição com esse token:
   → OnTokenValidated verifica blacklist
   → Se encontrado: 401 Unauthorized
```

---

## ⚠️ Avisos de Segurança

1. **Secret no repositório:** O JWT secret está em `appsettings.json` no repositório. Deveria usar User Secrets ou variáveis de ambiente em produção.

2. **Webhook aberto:** Os endpoints `/api/webhook/whatsapp` e `/api/webhook/status` não têm autenticação nem filtragem de IP (o Node.js chama localmente).

3. **Blacklist em memória:** A blacklist de tokens é in-memory — perdida ao reiniciar o servidor.

> **Senhas:** armazenadas com **BCrypt** (work factor 12). Usuários criados antes da implementação podem ter senha em texto puro — elas são detectadas e migradas para hash automaticamente no primeiro login.
