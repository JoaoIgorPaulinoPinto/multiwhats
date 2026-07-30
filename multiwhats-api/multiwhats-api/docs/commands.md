# Comandos Úteis

## Desenvolvimento

```bash
# Rodar a API
dotnet run

# Rodar com profile específico
dotnet run --launch-profile "http"
dotnet run --launch-profile "https"
dotnet run --launch-profile "Docker"
```

## Build e Publish

```bash
# Build (debug)
dotnet build

# Build (release)
dotnet build -c Release

# Publish
dotnet publish -c Release
```

## Entity Framework — Migrations

```bash
# Criar nova migration
dotnet ef migrations add NomeDaMigration

# Aplicar migrations no banco
dotnet ef database update

# Remover última migration (antes de aplicar)
dotnet ef migrations remove

# Listar migrations
dotnet ef migrations list
```

## Docker

```bash
# Build da imagem
docker build -t multiwhats-api .

# Rodar o container
docker run -p 5261:8080 multiwhats-api

# Com variáveis de ambiente
docker run -p 5261:8080 \
  -e "ConnectionStrings__DefaultConnection=..." \
  -e "JwtSettings__Secret=..." \
  multiwhats-api
```

## Serviço Node.js (Messageria)

```bash
# Navegar até o diretório
cd ../messageria

# Instalar dependências
npm install

# Rodar o serviço
npm start

# Rodar em desenvolvimento (com hot reload)
npm run dev
```

## Swagger

Quando a API está rodando, acesse:
- `http://localhost:5261/swagger` — Interface Swagger UI
- `http://localhost:5261/swagger/v1/swagger.json` — Especificação OpenAPI

## Portas

| Serviço | Porta HTTP | Porta HTTPS |
|---|---|---|
| ASP.NET API | 5261 | 7069 |
| Node.js | 3000 | — |
| Docker | 8080 | 8081 |
| MySQL (local/XAMPP) | 3306 | — |
| MySQL (Railway) | 40401 | — |

## Variáveis de Ambiente

Em produção, as configurações podem ser sobrescritas via variáveis de ambiente:

```bash
# Connection String
ConnectionStrings__DefaultConnection="server=...;database=...;..."

# JWT
JwtSettings__Secret="sua-chave-secreta"
JwtSettings__Issuer="seu-issuer"
JwtSettings__Audience="sua-audience"
```

## Verificação Rápida

```bash
# Verificar se a API está rodando
curl http://localhost:5261/swagger/v1/swagger.json

# Testar login
curl -X POST http://localhost:5261/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"name":"Joao","password":"123123"}'
```
