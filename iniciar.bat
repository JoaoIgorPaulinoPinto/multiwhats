@echo off

set ROOT=%~dp0

start "API" cmd /k "cd /d "%ROOT%multiwhats-api" && dotnet run"

timeout /t 10

start "Messageria" cmd /k "cd /d "%ROOT%messageria" && npm start"

timeout /t 10

start "LegacyDatabaseAdapter" cmd /k "cd /d "%ROOT%legacydatabaseadapter" && node server.ts"

timeout /t 10

start "Front" cmd /k "cd /d "%ROOT%..\multiwhats-front" && npm run dev"
