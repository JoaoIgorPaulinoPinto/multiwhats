using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using multiwhats_api.src.data.entities;

namespace multiwhats_api.src.services;

/// <summary>
/// SERVIÇO DE GERAÇÃO DE TOKENS JWT (JSON Web Token).
/// 
/// O QUE É JWT:
/// - É um "crachá digital" que o usuário recebe ao fazer login
/// - O Frontend salva esse crachá e envia em todas as requisições
/// - O Backend verifica o crachá para saber quem é o usuário e se ele está autenticado
/// 
/// COMO FUNCIONA:
/// 1. Usuário faz login (POST /api/auth/login)
/// 2. O Backend verifica nome/senha e gera um JWT com as informações do usuário
/// 3. O JWT é retornado ao Frontend
/// 4. O Frontend salva o JWT e envia em cada requisição: Authorization: Bearer {token}
/// 5. O Backend decodifica o JWT e sabe: "Esse é o Joao, role Admin, ID 1"
/// 
/// ESTRUTURA DO JWT (3 partes separadas por ponto):
/// HEADER.PAYLOAD.SIGNATURE
/// - Header: tipo do token e algoritmo (HS256)
/// - Payload: dados do usuário (ID, nome, role, expiração)
/// - Signature: assinatura digital (garante que o token não foi adulterado)
/// 
/// TEMPO DE EXPIRAÇÃO: 8 horas
/// - Após 8 horas, o token expira e o usuário precisa fazer login novamente
/// - Isso por segurança: se alguém roubar o token, ele só funciona por 8 horas
/// </summary>
public class TokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Gera um token JWT para o usuário informado.
    /// 
    /// O token contém:
    /// - NameIdentifier: ID do usuário (ex: "1")
    /// - Name: nome do usuário (ex: "Joao")
    /// - Role: perfil de acesso (ex: "Admin")
    /// - Jti: ID único do token (usado para blacklist/logout)
    /// - Iat: data de criação (issued at)
    /// 
    /// O token é assinado com HMAC-SHA256 usando uma chave secreta.
    /// Isso garante que ninguém pode forjar um token sem saber a chave.
    /// </summary>
    public string GenerateToken(User user)
    {
        // Lê as configurações JWT do appsettings.json
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

        // ── MONTANDO OS CLAIMS (DADOS DO USUÁRIO NO TOKEN) ──
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),   // ID do usuário
            new Claim(ClaimTypes.Name, user.Name),                       // Nome do usuário
            new Claim(ClaimTypes.Role, user.Role.ToString()),            // Role (Support/Dev/Admin)
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // ID único do token (para blacklist)
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()) // Data de criação
        };

        // ── CRIANDO O TOKEN ──
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],                               // Quem emitiu (ex: "MinhaApiEmissor")
            audience: jwtSettings["Audience"],                           // Para quem (ex: "MeuAppCliente")
            claims: claims,                                              // Dados do usuário
            expires: DateTime.UtcNow.AddHours(8),                        // Expira em 8 horas
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(secretKey),                      // Chave secreta para assinar
                SecurityAlgorithms.HmacSha256)                           // Algoritmo: HMAC-SHA256
        );

        // Converte o token para string (o "crachá" completo)
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
