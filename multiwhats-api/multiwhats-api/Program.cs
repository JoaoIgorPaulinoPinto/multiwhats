using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using multiwhats_api.src.data;
using multiwhats_api.src.data.db;
using multiwhats_api.src.data.strategies;
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
using multiwhats_api.src.usecases.usecases.OccurrenceUseCases;
using multiwhats_api.src.usecases.usecases.TaskUseCases;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// Application entry point. Configures services, middleware, authentication, and routes.

var builder = WebApplication.CreateBuilder(args);

// Max request body: 100MB for media messages
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024;
});

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();

// Allows frontend (localhost:3000, :5173) to access the API
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Config bindings
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection(AuthOptions.SectionName));
builder.Services.Configure<OccurrenceOptions>(builder.Configuration.GetSection(OccurrenceOptions.SectionName));
builder.Services.Configure<MediaOptions>(builder.Configuration.GetSection(MediaOptions.SectionName));

// PostgreSQL via EF Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IOccurrenceRepository, OccurrenceRepository>();
builder.Services.AddScoped<IClientTaskRepository, ClientTaskRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<IRegistrationCodeRepository, RegistrationCodeRepository>();
builder.Services.AddScoped<ISystemParameterRepository, SystemParameterRepository>();

// Auth
builder.Services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
builder.Services.AddScoped<ILoginUseCase, LoginUseCase>();
builder.Services.AddScoped<ILogoutUseCase, LogoutUseCase>();
builder.Services.AddScoped<IGenerateRegistrationCodeUseCase, GenerateRegistrationCodeUseCase>();
builder.Services.AddScoped<IUpdateUserUseCase, UpdateUserUseCase>();

// Chat
builder.Services.AddScoped<ICreateChatUseCase, CreateChatUseCase>();
builder.Services.AddScoped<IGetChatsUseCase, GetChatsUseCase>();
builder.Services.AddScoped<IMergeChatsUseCase, MergeChatsUseCase>();
builder.Services.AddScoped<IGetChatFullInfoUseCase, GetChatFullInfoUseCase>();

// Contacts
builder.Services.AddScoped<ICreateContactUseCase, CreateContactUseCase>();
builder.Services.AddScoped<IGetContactsUseCase, GetContactsUseCase>();
builder.Services.AddScoped<IDeleteContactUseCase, DeleteContactUseCase>();
builder.Services.AddScoped<IUpdateContactUseCase, UpdateContactUseCase>();
builder.Services.AddScoped<IAssignContactUseCase, AssignContactUseCase>();

// Message
builder.Services.AddHttpClient<ISendMessageUseCase, SendMessageUseCase>();
builder.Services.AddHttpClient<ISaveIncomingMessageUseCase, SaveIncomingMessageUseCase>();
builder.Services.AddScoped<IGetMessagesUseCase, GetMessagesUseCase>();

// Message Strategies
builder.Services.AddSingleton<IMessageStrategy, TextMessageStrategy>();
builder.Services.AddSingleton<IMessageStrategy, ImageMessageStrategy>();
builder.Services.AddSingleton<IMessageStrategy, VideoMessageStrategy>();
builder.Services.AddSingleton<IMessageStrategy, AudioMessageStrategy>();
builder.Services.AddSingleton<IMessageStrategy, DocumentMessageStrategy>();
builder.Services.AddSingleton<IMessageStrategy, StickerMessageStrategy>();
builder.Services.AddSingleton<MessageStrategyFactory>();

// Client
builder.Services.AddScoped<ICreateClientUseCase, CreateClientUseCase>();
builder.Services.AddScoped<IGetClientsUseCase, GetClientsUseCase>();
builder.Services.AddScoped<IUpdateClientUseCase, UpdateClientUseCase>();
builder.Services.AddScoped<IDeleteClientUseCase, DeleteClientUseCase>();

// Occurrence
builder.Services.AddScoped<ICreateOccurrenceUseCase, CreateOccurrenceUseCase>();
builder.Services.AddScoped<IGetOccurrencesUseCase, GetOccurrencesUseCase>();
builder.Services.AddScoped<IUpdateOccurrenceUseCase, UpdateOccurrenceUseCase>();
builder.Services.AddScoped<IDeleteOccurrenceUseCase, DeleteOccurrenceUseCase>();
builder.Services.AddScoped<IAdvanceOccurrenceStatusUseCase, AdvanceOccurrenceStatusUseCase>();
builder.Services.AddScoped<IGetOccurrenceMetricsUseCase, GetOccurrenceMetricsUseCase>();

// Task
builder.Services.AddScoped<ICreateTaskUseCase, CreateTaskUseCase>();
builder.Services.AddScoped<IGetTasksUseCase, GetTasksUseCase>();
builder.Services.AddScoped<IUpdateTaskUseCase, UpdateTaskUseCase>();
builder.Services.AddScoped<IDeleteTaskUseCase, DeleteTaskUseCase>();
builder.Services.AddScoped<IUpdateTaskStatusUseCase, UpdateTaskStatusUseCase>();

// Device
builder.Services.AddScoped<ISaveDeviceUseCase, SaveDeviceUseCase>();

// Auxiliary services
builder.Services.AddSingleton<TokenBlacklistService>();
builder.Services.AddSingleton<UseCaseLogger>();
builder.Services.AddSingleton<SystemConfigService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuditService>();

// Legacy DB sync (MySQL 4.1)
builder.Services.AddHttpClient<ILegacyDbSyncService, LegacyDbSyncService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["LegacyDb:BaseUrl"] ?? "http://localhost:3001");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var blacklist = context.HttpContext.RequestServices.GetRequiredService<TokenBlacklistService>();
            var jti = context.Principal?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            if (jti != null && blacklist.IsRevoked(jti))
            {
                context.Fail("Token foi revogado.");
                return;
            }

            // Refresh role/name from DB so role changes take effect immediately (no need to re-login)
            var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            {
                context.Fail("Token inválido.");
                return;
            }

            var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || !user.IsActive)
            {
                context.Fail("Usuário não encontrado ou inativo.");
                return;
            }

            var identity = context.Principal?.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var oldRole = identity.FindFirst(ClaimTypes.Role);
                if (oldRole != null) identity.RemoveClaim(oldRole);
                identity.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));

                var oldName = identity.FindFirst(ClaimTypes.Name);
                if (oldName != null) identity.RemoveClaim(oldName);
                identity.AddClaim(new Claim(ClaimTypes.Name, user.Name));
            }
        }
    };
});

// Swagger docs
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT: Bearer {seu-token}"
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

var app = builder.Build();

// Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware pipeline order matters
app.UseCors("SignalRPolicy");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// Seed default system parameters on startup
using (var scope = app.Services.CreateScope())
{
    var configService = scope.ServiceProvider.GetRequiredService<SystemConfigService>();
    await configService.SeedDefaultParametersAsync();
    await configService.LoadAsync();
}

app.MapControllers();
app.MapHub<WhatsappHub>("/whatsappHub");

app.Run();
