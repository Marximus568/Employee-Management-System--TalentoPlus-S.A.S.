# ------------------------------
# Stage 1: Base runtime image
# ------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Ports are mapped by docker-compose, no EXPOSE needed

# ------------------------------
# Stage 2: Build
# ------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore dependencies
COPY ["API/API.csproj", "API/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "API/API.csproj"

# Copy all source files
COPY . .

WORKDIR "/src/API"

# Build the project
RUN dotnet build "API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ------------------------------
# Stage 3: Publish
# ------------------------------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ------------------------------
# Stage 4: Final image
# ------------------------------
FROM base AS final
WORKDIR /app

# Copy the published output
COPY --from=publish /app/publish .

# Entrypoint
ENTRYPOINT ["dotnet", "API.dll"]
