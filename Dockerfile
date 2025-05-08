# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["EasySave/EasySave.csproj", "EasySave/"]
RUN dotnet restore "EasySave/EasySave.csproj"

# Copy the rest of the code
COPY . .

# Build and publish
WORKDIR "/src/EasySave"
RUN dotnet build "EasySave.csproj" -c Release -o /app/build
RUN dotnet publish "EasySave.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create directories for logs and configs
RUN mkdir -p /app/logs /app/config

# Copy the published app
COPY --from=build /app/publish .

# Set environment variables
ENV DOTNET_ENVIRONMENT=Production
ENV CONFIG_PATH=/app/config
ENV LOG_PATH=/app/logs

# Run the app
ENTRYPOINT ["dotnet", "EasySave.dll"] 