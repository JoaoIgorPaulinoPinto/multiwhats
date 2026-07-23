using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using multiwhats_api.src.data.db;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.repositories.repositories;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.AuthInterfaces;
using multiwhats_api.src.usecases.interfaces.ChatInterfaces;
using multiwhats_api.src.usecases.interfaces.ClientInterfaces;
using multiwhats_api.src.usecases.interfaces.ContactInterfaces;
using multiwhats_api.src.usecases.interfaces.DeviceInterfaces;
using multiwhats_api.src.usecases.interfaces.MessageInterfaces;
using multiwhats_api.src.usecases.interfaces.OccurrenceInterfaces;
using multiwhats_api.src.usecases.interfaces.TaskInterfaces;
using multiwhats_api.src.usecases.usecases.AuthUseCases;
using multiwhats_api.src.usecases.usecases.ChatUseCases;
using multiwhats_api.src.usecases.usecases.ClientUseCases;
using multiwhats_api.src.usecases.usecases.ContactUseCases;
using multiwhats_api.src.usecases.usecases.DeviceUseCases;
using multiwhats_api.src.usecases.usecases.MessageUseCases;
using multiwhats_api.src.data.strategies;
using multiwhats_api.src.usecases.usecases.OccurrenceUseCases;
using multiwhats_api.src.usecases.usecases.TaskUseCases;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// ═══════════════════════════════════════════════════════════════════════════════
// PROGRAM.CS - PONTO DE ENTRADA DA APLICAÇÃO
// ═══════════════════════════════════════════════════════════════════════════════
//
// Este arquivo é o "coração" da aplicação ASP.NET Core.
// Aqui configuramos TUDO: banco de dados, autenticação, serviços, rotas, etc.
//
// FLUXO DE INICIALIZAÇÃO:
// 1. Cria o builder (configuração da aplicação)
// 2. Registra todos os serviços (DI - Dependency Injection)
// 3. Configura autenticação JWT
// 4. Configura Swagger (documentação da API)
// 5. Cria o app (aplicação pronta)
// 6. Configura middleware (CORS, autenticação, roteamento)
// 7. Mapeia controllers e hubs
// 8. Roda a aplicação
//
// O QUE É DI (DEPENDENCY INJECTION):
// - É um padrão onde o ASP.NET "injeta" as dependências automaticamente
// - Em vez de criar manually: var repo = new ClientRepository(context);
// - Você pede: IClientRepository repo (e o ASP.NET cria para você)
// - Isso facilita testes e troca de implementações
//
// O QUE É MIDDLEWARE:
// - São "camadas" que processam cada requisição HTTP
// - Exemplo: CORS → Autenticação → Roteamento → Controller
// - Cada camada pode:processar, rejeitar, ou passar adiante
// ═══════════════════════════════════════════════════════════════════════════════

var builder = WebApplication.CreateBuilder(args);

// ── CONFIGURAÇÃO DO KESTREL (Servidor HTTP) ──
// Define o limite máximo de tamanho de requisição: 100MB
// Isso é necessário porque mensagens com mídia (imagens, vídeos) podem ser grandes
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
});

// ═══════════════════════════════════════════════════════════════════════════════
// REGISTRO DE SERVIÇOS (DEPENDENCY INJECTION)
// ═══════════════════════════════════════════════════════════════════════════════
//
// Cada linha "builder.Services.AddScoped<IA, B>()" significa:
// "Quando alguém pedir IA, crie uma instância de B"
//
// TIPOS DE ESCOPO:
// - AddScoped: Uma instância por REQUISIÇÃO (mais usado)
//   Ex: Se 10 pessoas fazem login ao mesmo tempo, cada uma tem seu TokenService
// - AddSingleton: Uma instância para TODA a aplicação (compartilhada)
//   Ex: TokenBlacklistService é único (a blacklist precisa ser compartilhada)
// - AddTransient: Uma instância a cada CHAMADA (raramente usado)
// ═══════════════════════════════════════════════════════════════════════════════

builder.Services.AddControllers();       // Habilita Controllers (endpoints HTTP)
builder.Services.AddSwaggerGen();        // Habilita Swagger (documentação automática)
builder.Services.AddSignalR();           // Habilita SignalR (comunicação em tempo real)
builder.Services.AddHttpContextAccessor(); // Permite acessar o usuário atual (HTTP context)

// ── CORS (Cross-Origin Resource Sharing) ──
// Permite que o Frontend (localhost:3000) acesse a API (localhost:5261)
// Sem isso, o navegador bloquearia as requisições por segurança
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // Frontend
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Necessário para SignalR
    });
});

// ── BANCO DE DADOS (MySQL via Entity Framework Core) ──
// Lê a connection string do appsettings.json e configura o MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)  // Detecta versão do MySQL automaticamente
    ));

// ═══════════════════════════════════════════════════════════════════════════════
// REGISTRO DOS REPOSITORIES (Acesso ao banco de dados)
// ═══════════════════════════════════════════════════════════════════════════════
// Cada repository abstrai as consultas SQL para uma tabela específica
// Exemplo: ClientRepository → tabela Clients
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IOccurrenceRepository, OccurrenceRepository>();
builder.Services.AddScoped<IClientTaskRepository, ClientTaskRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();

// ═══════════════════════════════════════════════════════════════════════════════
// REGISTRO DOS USE CASES (Lógica de negócio)
// ═══════════════════════════════════════════════════════════════════════════════
// Cada Use Case representa uma operação: criar, buscar, atualizar, deletar

// Auth (Autenticação)
builder.Services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
builder.Services.AddScoped<ILoginUseCase, LoginUseCase>();
builder.Services.AddScoped<ILogoutUseCase, LogoutUseCase>();

// Chat (Conversas)
builder.Services.AddScoped<ICreateChatUseCase, CreateChatUseCase>();
builder.Services.AddScoped<IGetChatsUseCase, GetChatsUseCase>();

// Contact (Contatos)
builder.Services.AddScoped<ICreateContactUseCase, CreateContactUseCase>();
builder.Services.AddScoped<IGetContactsUseCase, GetContactsUseCase>();
builder.Services.AddScoped<IDeleteContactUseCase, DeleteContactUseCase>();
builder.Services.AddScoped<IUpdateContactUseCase, UpdateContactUseCase>();
builder.Services.AddScoped<IAssignContactUseCase, AssignContactUseCase>();

// Message (Mensagens)
// NOTA: SendMessageUseCase usa AddHttpClient porque precisa fazer HTTP POST para o Node.js
builder.Services.AddHttpClient<ISendMessageUseCase, SendMessageUseCase>();
builder.Services.AddScoped<ISaveIncomingMessageUseCase, SaveIncomingMessageUseCase>();
builder.Services.AddScoped<IGetMessagesUseCase, GetMessagesUseCase>();

// ═══════════════════════════════════════════════════════════════════════════════
// REGISTRO DAS MESSAGE STRATEGIES (Padrão Strategy)
// ═══════════════════════════════════════════════════════════════════════════════
// Cada strategy sabe como tratar um tipo específico de mensagem
// O MessageStrategyFactory escolhe a strategy correta em tempo de execução
builder.Services.AddSingleton<IMessageStrategy, TextMessageStrategy>();
builder.Services.AddSingleton<IMessageStrategy, ImageMessageStrategy>();
builder.Services.AddSingleton<IMessageStrategy, VideoMessageStrategy>();
builder.Services.AddSingleton<IMessageStrategy, AudioMessageStrategy>();
builder.Services.AddSingleton<IMessageStrategy, DocumentMessageStrategy>();
builder.Services.AddSingleton<IMessageStrategy, StickerMessageStrategy>();
builder.Services.AddSingleton<MessageStrategyFactory>();  // Factory que escolhe a strategy

// Client (Clientes)
builder.Services.AddScoped<ICreateClientUseCase, CreateClientUseCase>();
builder.Services.AddScoped<IGetClientsUseCase, GetClientsUseCase>();
builder.Services.AddScoped<IUpdateClientUseCase, UpdateClientUseCase>();
builder.Services.AddScoped<IDeleteClientUseCase, DeleteClientUseCase>();

// Occurrence (Ocorrências/Chamados)
builder.Services.AddScoped<ICreateOccurrenceUseCase, CreateOccurrenceUseCase>();
builder.Services.AddScoped<IGetOccurrencesUseCase, GetOccurrencesUseCase>();
builder.Services.AddScoped<IUpdateOccurrenceUseCase, UpdateOccurrenceUseCase>();
builder.Services.AddScoped<IDeleteOccurrenceUseCase, DeleteOccurrenceUseCase>();

// Task (Tarefas)
builder.Services.AddScoped<ICreateTaskUseCase, CreateTaskUseCase>();
builder.Services.AddScoped<IGetTasksUseCase, GetTasksUseCase>();
builder.Services.AddScoped<IUpdateTaskUseCase, UpdateTaskUseCase>();
builder.Services.AddScoped<IDeleteTaskUseCase, DeleteTaskUseCase>();
builder.Services.AddScoped<IUpdateTaskStatusUseCase, UpdateTaskStatusUseCase>();

// Device (Dispositivo WhatsApp)
builder.Services.AddScoped<ISaveDeviceUseCase, SaveDeviceUseCase>();

// ═══════════════════════════════════════════════════════════════════════════════
// REGISTRO DOS SERVIÇOS AUXILIARES
// ═══════════════════════════════════════════════════════════════════════════════
builder.Services.AddSingleton<TokenBlacklistService>(); // Singleton: blacklist compartilhada
builder.Services.AddSingleton<UseCaseLogger>();         // Singleton: logger compartilhado
builder.Services.AddScoped<TokenService>();             // Scoped: um por requisição
builder.Services.AddScoped<AuditService>();             // Scoped: um por requisição

// ═══════════════════════════════════════════════════════════════════════════════
// CONFIGURAÇÃO DE AUTENTICAÇÃO JWT
// ═══════════════════════════════════════════════════════════════════════════════
//
// O QUE É JWT:
// - "JSON Web Token" - um padrão para autenticação stateless
// - O usuário faz login → recebe um token → envia em cada requisição
// - O Backend verifica o token sem precisar consultar o banco
//
// COMO FUNCIONA A VERIFICAÇÃO:
// 1. O token vem no header: Authorization: Bearer eyJhbG...
// 2. O ASP.NET decodifica o token usando a chave secreta
// 3. Verifica: assinatura válida? Não expirou? Emissor correto?
// 4. Se tudo OK → extrai os claims (ID, nome, role) → disponibiliza via User
// 5. Se algo errado → rejeita com 401 Unauthorized
//
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;  // Permite HTTP (para desenvolvimento)
    options.SaveToken = true;              // Salva o token no HttpContext
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,                    // Verifica a assinatura
        IssuerSigningKey = new SymmetricSecurityKey(secretKey), // Chave secreta
        ValidateIssuer = true,                              // Verifica quem emitiu
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,                            // Verifica para quem é
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,                            // Verifica se não expirou
        ClockSkew = TimeSpan.Zero,                         // Sem tolerância de tempo
        RoleClaimType = ClaimTypes.Role                     // Claim de role
    };

    // ── EVENTO: VALIDAÇÃO DO TOKEN ──
    // Chamado DEPOIS de decodificar o token, MAS antes de aceitá-lo
    // Aqui verificamos se o token foi revogado (logout)
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            // Pega o serviço de blacklist (lista negra de tokens)
            var blacklist = context.HttpContext.RequestServices.GetRequiredService<TokenBlacklistService>();

            // Extrai o JTI (ID único) do token
            var jti = context.Principal?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

            // Se o JTI está na blacklist → token foi revogado → rejeita
            if (jti != null && blacklist.IsRevoked(jti))
            {
                context.Fail("Token foi revogado.");
            }
            return Task.CompletedTask;
        }
    };
});

// ── CONFIGURAÇÃO DO SWAGGER ──
// Swagger gera documentação interativa da API automaticamente
// Disponível em: http://localhost:5261/swagger
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT desta maneira: Bearer {seu-token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddAuthorization();

// ═══════════════════════════════════════════════════════════════════════════════
// CRIAÇÃO E CONFIGURAÇÃO DA APLICAÇÃO
// ═══════════════════════════════════════════════════════════════════════════════
var app = builder.Build();

// ── SWAGGER (apenas em desenvolvimento) ──
// Em produção, a documentação Swagger não fica pública
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();     // Gera o JSON da API
    app.UseSwaggerUI();   // Cria a interface visual do Swagger
}

// ── MIDDLEWARE (processadores de requisição) ──
// A ORDEM IMPORTA! Cada middleware processa a requisição na ordem:
app.UseCors("SignalRPolicy");     // 1. Permite CORS (origin)

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();    // 2. Redireciona HTTP→HTTPS (só em produção)
}

app.UseAuthentication();          // 3. Verifica o token JWT
app.UseAuthorization();           // 4. Verifica permissões (roles)

// ── MAPEAMENTO DE ROTAS ──
app.MapControllers();             // Mapeia todos os Controllers (endpoints HTTP)
app.MapHub<WhatsappHub>("/whatsappHub"); // Mapeia o Hub SignalR (comunicação em tempo real)

// ── INICIALIZAÇÃO ──
// app.Run() inicia o servidor e aguarda requisições
app.Run();
