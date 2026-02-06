# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy CPM files
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]

# Copy Project
COPY ["src/AdvancedBuild/AdvancedBuild.csproj", "src/AdvancedBuild/"]
RUN dotnet restore "src/AdvancedBuild/AdvancedBuild.csproj"

# Copy Source
COPY . .
WORKDIR "/src/src/AdvancedBuild"
RUN dotnet build "AdvancedBuild.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "AdvancedBuild.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime (Chiseled / Distroless)
# "noble-chiseled" is based on Ubuntu 24.04, stripped down (no shell, no package manager)
# It runs as non-root user (app) by default.
FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Metadata Labels (OCI Standard)
LABEL org.opencontainers.image.source="https://github.com/StartYour/Project"
LABEL org.opencontainers.image.description="Advanced Build Demo with Chiseled Images"
LABEL org.opencontainers.image.licenses="MIT"

# Expose port (8080 is default for non-root .NET 8/9 images)
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "AdvancedBuild.dll"]
