@echo off

@echo Build swagger.json...
rem requires the dotnet tool swagger
rem dotnet tool install --version 6.5.0 Swashbuckle.AspNetCore.Cli
@echo.
pushd ..\Backend.Server
dotnet tool restore
dotnet swagger tofile --output ..\docs\swagger.json ..\Backend.Server\bin\Release\net9.0\PayrollEngine.Backend.Server.dll PayrollEngineBackendAPI
popd
