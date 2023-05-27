<h1>Payroll Engine Backend</h1>

Das Backend ist die zentrale Komponente des Lohnrechners, welches über eine REST API gesteuert wird. Die Anwendung ist als Hintergrunddienst in einem geschlossenen Systeme konzipiert (Security). Neben den Enpunkten sind Webhooks die einzige Schnittpunkte zur Aussenwelt.

<br />

## Hosting
Für den Betrieb des Backend-Servers muss der Webhoster die Ausführung von .NET Core Applikationen unterstützen. Für die lokale Entwicklung dient [IIS Express](https://learn.microsoft.com/en-us/iis/extensions/introduction-to-iis-express/iis-express-overview) als Host in zwei Ausführungsvarianten:
- Console application using the batch ***PayrollEngine\Batches\Backend.Start.bat*** [CLI dotnet command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet)
- Visual Studio Solution ***PayrollEngine\PayrollEngine.Backend\PayrollEngine.Backend.sln*** using the debugger

<br/>

## Open API
Die Payroll Engine API unterstützt die [Open API](https://www.openapis.org/) Spezifikation und beschreibt die Schnittstelle mit dem Tool [Swagger](https://swagger.io/).

Das Dokument [REST Service Endpoints] beschreibt die vefügbaren Endpunkte.

<br/>

## Configuration
Die Server-Konfiguration *Server\appsetings.json* beinhaltet folgende Einstellungen:
- Culture
- Datenbankverbindung
- Health Check
- HTTP logs
- Timeouts for webhooks, transactions and assembly-cache
- Script compiler
- Systemlog mit [Serilog](https://serilog.net/), andere Logging-Tools können integriert werden

> It is recommended to save the server settings within your local [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets).
<br/>

## Security
*ToDo*
<br/>

## Script Compiler
Die vom Business definierte Businesslogik in C# wird vom Backend mit [Roslyn](https://github.com/dotnet/roslyn) in Binärdateien (Assemblies) kompiliert. Dieser Ansatz wirkt sich positiv auf die Laufzeitpeformance aus, so dass auch umfangreichere Berechnungen genügend schnell erfolgen.
Zur Laufzeit führt das Backend die Assemblies in einem Cache. Zur Speicheroptimierung werden ungenutzte Assemblies periodisch entfernt.
