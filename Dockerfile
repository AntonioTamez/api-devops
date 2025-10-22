# =============================================================================
# Stage 1: Build
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies (better layer caching)
COPY src/*.csproj ./src/
RUN dotnet restore ./src/DevOpsApi.csproj

# Copy everything else and build
COPY src/ ./src/
WORKDIR /app/src
RUN dotnet publish DevOpsApi.csproj -c Release -o /app/publish --no-restore

# =============================================================================
# Stage 2: Runtime
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

# Install curl for health checks (Alpine uses apk instead of apt-get)
RUN apk add --no-cache curl

# Create a non-root user
RUN addgroup -S appuser && \
    adduser -S -G appuser -u 1000 appuser && \
    chown -R appuser:appuser /app

# Copy published app from build stage
COPY --from=build --chown=appuser:appuser /app/publish .

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "DevOpsApi.dll"]
