@echo off

@echo Build swagger.json...
@echo.
pushd ..\Backend.Server
dotnet swagger tofile --output ..\docs\swagger.json ..\Backend.Server\bin\Release\net7.0\PayrollEngine.Backend.Server.dll PayrollEngineBackendAPI
popd

pause
