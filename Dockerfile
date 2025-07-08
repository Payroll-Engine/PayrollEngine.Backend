FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy solution and project files
COPY ["PayrollEngine.Backend.sln", "./"]
COPY ["Api/Api.Controller/PayrollEngine.Api.Controller.csproj", "Api/Api.Controller/"]
COPY ["Api/Api.Core/PayrollEngine.Api.Core.csproj", "Api/Api.Core/"]
COPY ["Api/Api.Map/PayrollEngine.Api.Map.csproj", "Api/Api.Map/"]
COPY ["Api/Api.Model/PayrollEngine.Api.Model.csproj", "Api/Api.Model/"]
COPY ["Backend.Controller/PayrollEngine.Backend.Controller.csproj", "Backend.Controller/"]
COPY ["Backend.Server/PayrollEngine.Backend.Server.csproj", "Backend.Server/"]
COPY ["Domain/Domain.Application/PayrollEngine.Domain.Application.csproj", "Domain/Domain.Application/"]
COPY ["Domain/Domain.Model/PayrollEngine.Domain.Model.csproj", "Domain/Domain.Model/"]
COPY ["Domain/Domain.Scripting/PayrollEngine.Domain.Scripting.csproj", "Domain/Domain.Scripting/"]
COPY ["Persistence/Persistence/PayrollEngine.Persistence.csproj", "Persistence/Persistence/"]
COPY ["Persistence/Persistence.SqlServer/PayrollEngine.Persistence.SqlServer.csproj", "Persistence/Persistence.SqlServer/"]
COPY ["Domain/Domain.Model.Tests/PayrollEngine.Domain.Model.Tests.csproj", "Domain/Domain.Model.Tests/"]

# copy Directory.Build.props files
COPY ["Directory.Build.props", "./"]

RUN dotnet restore "PayrollEngine.Backend.sln"

# copy everything else
COPY . .
WORKDIR "/src/Backend.Server"
RUN dotnet publish "PayrollEngine.Backend.Server.csproj" -c Release -o /app/publish --no-restore

# final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PayrollEngine.Backend.Server.dll"]