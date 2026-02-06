# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy CPM config
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]

# Copy Project
COPY ["src/MinimalApiPipeline/MinimalApiPipeline.csproj", "src/MinimalApiPipeline/"]
RUN dotnet restore "src/MinimalApiPipeline/MinimalApiPipeline.csproj"

# Copy Rest
COPY . .
WORKDIR "/src/src/MinimalApiPipeline"
RUN dotnet build "MinimalApiPipeline.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "MinimalApiPipeline.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MinimalApiPipeline.dll"]
