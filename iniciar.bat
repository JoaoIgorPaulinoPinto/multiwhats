@echo off

start "Messageria" cmd /k "cd /d C:\Users\User\source\multiwhtas-api\multiwhats-api\messageria && npm start"

timeout /t 10

start "API" cmd /k "cd /d C:\Users\User\source\multiwhtas-api\multiwhats-api\multiwhats-api && dotnet run"

timeout /t 10

start "LegacyDatabaseAdapter" cmd /k "cd /d C:\Users\User\source\multiwhtas-api\multiwhats-api\legacydatabaseadapter && npm start"

timeout /t 10

start "Front" cmd /k "cd /d C:\Users\User\source\multiwhtas-api\multiwhats-front && npm run dev"