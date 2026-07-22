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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddHttpContextAccessor();

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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));

// Repositories
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IOccurrenceRepository, OccurrenceRepository>();
builder.Services.AddScoped<IClientTaskRepository, ClientTaskRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();

// Auth Use Cases
builder.Services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
builder.Services.AddScoped<ILoginUseCase, LoginUseCase>();
builder.Services.AddScoped<ILogoutUseCase, LogoutUseCase>();

// Chat Use Cases
builder.Services.AddScoped<ICreateChatUseCase, CreateChatUseCase>();
builder.Services.AddScoped<IGetChatsUseCase, GetChatsUseCase>();

// Contact Use Cases
builder.Services.AddScoped<ICreateContactUseCase, CreateContactUseCase>();
builder.Services.AddScoped<IGetContactsUseCase, GetContactsUseCase>();
builder.Services.AddScoped<IDeleteContactUseCase, DeleteContactUseCase>();
builder.Services.AddScoped<IUpdateContactUseCase, UpdateContactUseCase>();
builder.Services.AddScoped<IAssignContactUseCase, AssignContactUseCase>();

// Message Use Cases
builder.Services.AddHttpClient<ISendMessageUseCase, SendMessageUseCase>();
builder.Services.AddScoped<ISaveIncomingMessageUseCase, SaveIncomingMessageUseCase>();
builder.Services.AddScoped<IGetMessagesUseCase, GetMessagesUseCase>();

// Message Strategies
builder.Services.AddSingleton<IMessageStrategy, TextMessageStrategy>();
builder.Services.AddSingleton<IMessageStrategy, ImageMessageStrategy>();
builder.Services.AddSingleton<IMessageStrategy, VideoMessageStrategy>();
builder.Services.AddSingleton<IMessageStrategy, AudioMessageStrategy>();
builder.Services.AddSingleton<IMessageStrategy, DocumentMessageStrategy>();
builder.Services.AddSingleton<IMessageStrategy, StickerMessageStrategy>();
builder.Services.AddSingleton<MessageStrategyFactory>();

// Client Use Cases
builder.Services.AddScoped<ICreateClientUseCase, CreateClientUseCase>();
builder.Services.AddScoped<IGetClientsUseCase, GetClientsUseCase>();
builder.Services.AddScoped<IUpdateClientUseCase, UpdateClientUseCase>();
builder.Services.AddScoped<IDeleteClientUseCase, DeleteClientUseCase>();

// Occurrence Use Cases
builder.Services.AddScoped<ICreateOccurrenceUseCase, CreateOccurrenceUseCase>();
builder.Services.AddScoped<IGetOccurrencesUseCase, GetOccurrencesUseCase>();
builder.Services.AddScoped<IUpdateOccurrenceUseCase, UpdateOccurrenceUseCase>();
builder.Services.AddScoped<IDeleteOccurrenceUseCase, DeleteOccurrenceUseCase>();

// Task Use Cases
builder.Services.AddScoped<ICreateTaskUseCase, CreateTaskUseCase>();
builder.Services.AddScoped<IGetTasksUseCase, GetTasksUseCase>();
builder.Services.AddScoped<IUpdateTaskUseCase, UpdateTaskUseCase>();
builder.Services.AddScoped<IDeleteTaskUseCase, DeleteTaskUseCase>();
builder.Services.AddScoped<IUpdateTaskStatusUseCase, UpdateTaskStatusUseCase>();

// Device Use Cases
builder.Services.AddScoped<ISaveDeviceUseCase, SaveDeviceUseCase>();

// Services
builder.Services.AddSingleton<TokenBlacklistService>();
builder.Services.AddSingleton<UseCaseLogger>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuditService>();

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
        OnTokenValidated = context =>
        {
            var blacklist = context.HttpContext.RequestServices.GetRequiredService<TokenBlacklistService>();
            var jti = context.Principal?.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            if (jti != null && blacklist.IsRevoked(jti))
            {
                context.Fail("Token foi revogado.");
            }
            return Task.CompletedTask;
        }
    };
});
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

var app = builder.Build();




if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("SignalRPolicy");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<WhatsappHub>("/whatsappHub");


app.Run();
