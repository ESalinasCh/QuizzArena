# syntax=docker/dockerfile:1

# ============================================================
# QuizzArena backend (Host) container image
#
# Build context MUST be the repository root because the build
# relies on the root Directory.Build.props / Directory.Build.targets
# (discovered by MSBuild walking up the directory tree).
#
#   docker build -t quizzarena-backend .
#   docker run --rm -p 8080:8080 --env-file QuizzArena.Backend/.env quizzarena-backend
# ============================================================

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Skip the Husky git-hook install that runs on restore (Directory.Build.targets).
ENV HUSKY=0

# Copy solution-wide build configuration first for better layer caching.
COPY Directory.Build.props Directory.Build.targets ./
COPY QuizzArena.Backend/QuizzArena.Backend.slnx QuizzArena.Backend/

# Copy only project files to restore dependencies (cached unless a .csproj changes).
COPY QuizzArena.Backend/Host/Host.csproj QuizzArena.Backend/Host/
COPY QuizzArena.Backend/Shared/Shared.csproj QuizzArena.Backend/Shared/
COPY QuizzArena.Backend/Modules/QuizzArena.Quizzing/QuizzArena.Quizzing.csproj QuizzArena.Backend/Modules/QuizzArena.Quizzing/
COPY QuizzArena.Backend/Modules/QuizzArena.Users/QuizzArena.Users.csproj QuizzArena.Backend/Modules/QuizzArena.Users/
COPY QuizzArena.Backend/Modules/QuizzArena.DocumentProcessing/QuizzArena.DocumentProcessing.csproj QuizzArena.Backend/Modules/QuizzArena.DocumentProcessing/

RUN dotnet restore QuizzArena.Backend/Host/Host.csproj

# Copy the remaining source and publish a release build.
COPY . .
RUN dotnet publish QuizzArena.Backend/Host/Host.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Kerberos GSSAPI library probed by Npgsql when opening connections.
# Without it the connection still works but Npgsql logs a noisy error.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

# Run as the non-root user provided by the base image.
USER $APP_UID

# Kestrel listens on 8080 by default (ASPNETCORE_HTTP_PORTS) in .NET 8+.
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Host.dll"]
