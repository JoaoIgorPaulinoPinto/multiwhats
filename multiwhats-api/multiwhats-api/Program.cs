using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using multiwhats_api.src.data.db;
using multiwhats_api.src.repositories.interfaces;
using multiwhats_api.src.repositories.repositories;
using multiwhats_api.src.services;
using multiwhats_api.src.usecases.interfaces.AuthInterfaces;
using multiwhats_api.src.usecases.interfaces.ContatoInterfaces;
using multiwhats_api.src.usecases.interfaces.MensagemInterfaces;
using multiwhats_api.src.usecases.usecases.AuthUseCases;
using multiwhats_api.src.usecases.usecases.ContatoUseCases;
using multiwhats_api.src.usecases.usecases.MensagemUseCases;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

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

builder.Services.AddScoped<IStatusRepository, StatusRepository>();
builder.Services.AddScoped<IGrupoRepository, GrupoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IOcorrenciaRepository, OcorrenciaRepository>();

builder.Services.AddScoped<IContatoRepository, ContatoRepository>();
builder.Services.AddScoped<IMensagemRepository, MensagemRepository>();

// Use Cases
builder.Services.AddHttpClient<IEnviarMensagemUseCase, EnviarMensagemUseCase>();
builder.Services.AddScoped<ISalvarMensagemRecebidaUseCase, SalvarMensagemRecebidaUseCase>();
builder.Services.AddScoped<IPegarContatoPorNumeroUseCase, PegarContatoPorNumero>();
builder.Services.AddScoped<ICriarContatoUseCase, CriarContatoUseCase>();

// Auth
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IRegistrarUsuarioUseCase, RegistrarUsuarioUseCase>();
builder.Services.AddScoped<ILogarUsuarioUseCase, LogarUsuarioUseCase>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Em prod, mude para true
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
        ClockSkew = TimeSpan.Zero // Remove o atraso padrão de 5 minutos na expiração
    };
}); 
builder.Services.AddAuthorization();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}


app.UseCors("SignalRPolicy");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<WhatsappHub>("/whatsappHub");

app.Run();