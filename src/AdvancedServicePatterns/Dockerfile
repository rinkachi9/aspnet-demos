# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy CPM config
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]

# Copy Project
COPY ["src/AdvancedServicePatterns/AdvancedServicePatterns.csproj", "src/AdvancedServicePatterns/"]
RUN dotnet restore "src/AdvancedServicePatterns/AdvancedServicePatterns.csproj"

# Copy Rest
COPY . .
WORKDIR "/src/src/AdvancedServicePatterns"
RUN dotnet build "AdvancedServicePatterns.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "AdvancedServicePatterns.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AdvancedServicePatterns.dll"]
