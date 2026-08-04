@echo off
set ROOT=%~dp0

start /B cmd /c "cd /d "%ROOT%multiwhats-api\multiwhats-api" && dotnet run"

timeout /t 10 >nul

start /B cmd /c "cd /d "%ROOT%multiwhats-api\messageria" && npm start"

timeout /t 10 >nul

start /B cmd /c "cd /d "%ROOT%multiwhats-api\legacydatabaseadapter" && npm start"

timeout /t 10 >nul

start /B cmd /c "cd /d "%ROOT%multiwhats-front" && npm start"