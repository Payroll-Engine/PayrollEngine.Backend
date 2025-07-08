FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy solution and project files
COPY ["PayrollEngine.Backend.sln", "./"]
COPY ["Api/Api.Controller/Api.Controller.csproj", "Api/Api.Controller/"]
COPY ["Api/Api.Core/Api.Core.csproj", "Api/Api.Core/"]
COPY ["Api/Api.Map/Api.Map.csproj", "Api/Api.Map/"]
COPY ["Api/Api.Model/Api.Model.csproj", "Api/Api.Model/"]
COPY ["Backend.Controller/Backend.Controller.csproj", "Backend.Controller/"]
COPY ["Backend.Server/PayrollEngine.Backend.Server.csproj", "Backend.Server/"]
COPY ["Domain/Domain.Application/Domain.Application.csproj", "Domain/Domain.Application/"]
COPY ["Domain/Domain.Model/Domain.Model.csproj", "Domain/Domain.Model/"]
COPY ["Domain/Domain.Scripting/Domain.Scripting.csproj", "Domain/Domain.Scripting/"]
COPY ["Persistence/Persistence/Persistence.csproj", "Persistence/Persistence/"]
COPY ["Persistence/Persistence.SqlServer/Persistence.SqlServer.csproj", "Persistence/Persistence.SqlServer/"]

# copy Directory.Build.props files
COPY ["Directory.Build.props", "./"]
COPY ["Api/Directory.Build.props", "Api/"]
COPY ["Domain/Directory.Build.props", "Domain/"]
COPY ["Persistence/Directory.Build.props", "Persistence/"]

RUN dotnet restore "PayrollEngine.Backend.sln"

# copy everything else
COPY . .
WORKDIR "/src/Backend.Server"
RUN dotnet publish "PayrollEngine.Backend.Server.csproj" -c Release -o /app/publish --no-restore

# final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PayrollEngine.Backend.Server.dll"] 