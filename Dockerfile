# STAGE 1: Build and Test
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install ReportGenerator globally for test coverage HTML reports
RUN dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.1.10
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy solution and project files (for layer caching optimization)
COPY ["Supermarket.sln", "./"]
COPY ["src/Supermarket.Core/Supermarket.Core.csproj", "src/Supermarket.Core/"]
COPY ["src/Supermarket.Api/Supermarket.Api.csproj", "src/Supermarket.Api/"]
COPY ["src/Supermarket.Web/Supermarket.Web.csproj", "src/Supermarket.Web/"]
COPY ["tests/Supermarket.Tests/Supermarket.Tests.csproj", "tests/Supermarket.Tests/"]

# Restore dependencies
RUN dotnet restore "Supermarket.sln"

# Copy all source code
COPY . .

# Run Tests with Code Coverage
# If tests fail, Docker build fails (CI gate)
WORKDIR /src/tests/Supermarket.Tests
RUN dotnet test \
    --configuration Release \
    --no-restore \
    --collect:"XPlat Code Coverage" \
    --results-directory ./TestResults \
    --logger "console;verbosity=detailed"

# Generate HTML Coverage Report
WORKDIR /src
RUN reportgenerator \
    -reports:"tests/Supermarket.Tests/TestResults/**/coverage.cobertura.xml" \
    -targetdir:"TestReport" \
    -reporttypes:"Html;Badges"

# Build and Publish the API (includes Blazor WASM assets)
WORKDIR /src/src/Supermarket.Api
RUN dotnet publish \
    --configuration Release \
    --no-restore \
    -o /app/publish

# STAGE 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Copy test report to wwwroot/reports for web access
# This allows viewing test results at /reports/index.html
RUN mkdir -p wwwroot/reports
COPY --from=build /src/TestReport ./wwwroot/reports

# Railway-specific configuration
# Railway assigns dynamic ports, but we use 8080 as standard
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "Supermarket.Api.dll"]
