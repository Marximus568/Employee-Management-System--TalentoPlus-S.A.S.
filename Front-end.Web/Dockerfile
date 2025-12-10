# ------------------------------
# Stage 1: Base runtime image
# ------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# No EXPOSE here, ports will be mapped by docker-compose

# ------------------------------
# Stage 2: Build
# ------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore dependencies
COPY ["Front-end.Web/Front-end.Web.csproj", "Front-end.Web/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "Front-end.Web/Front-end.Web.csproj"

# Copy all source files
COPY . .

WORKDIR "/src/Front-end.Web"

# Build the project
RUN dotnet build "Front-end.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ------------------------------
# Stage 3: Publish
# ------------------------------
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Front-end.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ------------------------------
# Stage 4: Final image
# ------------------------------
FROM base AS final
WORKDIR /app

# Copy the published output
COPY --from=publish /app/publish .

# Entrypoint
ENTRYPOINT ["dotnet", "Front-end.Web.dll"]
