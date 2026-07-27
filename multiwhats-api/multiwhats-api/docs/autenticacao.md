# Autenticação e Autorização

## JWT Bearer Authentication

### Configuração

| Parâmetro | Valor |
|---|---|
| Scheme | `JwtBearerDefaults.AuthenticationScheme` |
| Algoritmo | HMAC-SHA256 |
| Issuer | `MinhaApiEmissor` |
| Audience | `MeuAppCliente` |
| Expiração | 8 horas (hardcoded no `TokenService`) |
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
2. LoginUseCase valida credenciais
3. TokenService.GenerateToken(user) → gera JWT
4. Retorna { token, user }
5. Cliente usa: Authorization: Bearer {token}
```

---

## Autorização por Role

| Role | PATCH /api/tasks/{id}/status | Demais endpoints |
|---|---|---|
| Admin | ✅ Permitido | ✅ Permitido |
| Dev | ✅ Permitido | ✅ Permitido |
| Support | ❌ Negado | ✅ Permitido |

### Endpoints Anônimos (sem auth)

| Rota | Descrição |
|---|---|
| `POST /api/auth/register` | Registro |
| `POST /api/auth/login` | Login |
| `POST /api/webhook/whatsapp` | Webhook do Node.js |
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

1. **Senha em texto puro:** A entidade `User` armazena `Password` como string sem hash. Não há uso de BCrypt ou similar.

2. **Secret hardcoded:** O JWT secret está em `appsettings.json` no repositório. Deveria usar User Secrets ou variáveis de ambiente em produção.

3. **Webhook aberto:** O endpoint `/api/webhook/whatsapp` não tem autenticação nem filtragem de IP.

4. **Config vs código:** `JwtSettings.ExpiryInMinutes` está definido como 60 no appsettings, mas `TokenService` usa 8 horas hardcoded — o valor do config não é utilizado.
