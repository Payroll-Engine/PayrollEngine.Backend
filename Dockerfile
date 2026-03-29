# Build stage with multi-platform support
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TARGETARCH
ARG BUILDPLATFORM
ARG GITHUB_TOKEN
ARG NUGET_SOURCE=github
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
COPY ["Domain/Domain.Model.Tests/PayrollEngine.Domain.Model.Tests.csproj", "Domain/Domain.Model.Tests/"]
COPY ["Domain/Domain.Scripting/PayrollEngine.Domain.Scripting.csproj", "Domain/Domain.Scripting/"]
COPY ["Persistence/Persistence/PayrollEngine.Persistence.csproj", "Persistence/Persistence/"]
COPY ["Persistence/Persistence.DbQuery.Tests/PayrollEngine.Persistence.DbQuery.Tests.csproj", "Persistence/Persistence.DbQuery.Tests/"]
COPY ["Persistence/Persistence.MySql/PayrollEngine.Persistence.MySql.csproj", "Persistence/Persistence.MySql/"]
COPY ["Persistence/Persistence.SqlServer/PayrollEngine.Persistence.SqlServer.csproj", "Persistence/Persistence.SqlServer/"]

# copy Directory.Build.props files
COPY ["Directory.Build.props", "./"]

# Configure NuGet source
# NUGET_SOURCE=github (default): adds GitHub Packages — used for lib builds and dry-run
# NUGET_SOURCE=nuget.org: NuGet.org only — live app builds, identical to external PE users
RUN if [ "${NUGET_SOURCE}" = "github" ]; then \
      dotnet nuget add source "https://nuget.pkg.github.com/Payroll-Engine/index.json" \
        --name github \
        --username github-actions \
        --password ${GITHUB_TOKEN} \
        --store-password-in-clear-text; \
    fi

# Restore with architecture-specific runtime
RUN if [ "$TARGETARCH" = "arm64" ]; then \
      dotnet restore "PayrollEngine.Backend.sln" --runtime linux-arm64; \
    else \
      dotnet restore "PayrollEngine.Backend.sln" --runtime linux-x64; \
    fi

# copy everything else
COPY . .
WORKDIR "/src/Backend.Server"

# Publish with architecture-specific runtime and restore included
RUN if [ "$TARGETARCH" = "arm64" ]; then \
      dotnet publish "PayrollEngine.Backend.Server.csproj" -c Release -o /app/publish --runtime linux-arm64 --self-contained false; \
    else \
      dotnet publish "PayrollEngine.Backend.Server.csproj" -c Release -o /app/publish --runtime linux-x64 --self-contained false; \
    fi

# final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PayrollEngine.Backend.Server.dll"]
