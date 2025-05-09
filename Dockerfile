# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["easysave.csproj", "./"]
RUN dotnet restore "easysave.csproj"

# Copy the rest of the code
COPY . .

# Build and publish
RUN dotnet build "easysave.csproj" -c Release -o /app/build
RUN dotnet publish "easysave.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Create directories for logs and configs
RUN mkdir -p /app/logs /app/config /app/projects

# Copy the published app
COPY --from=build /app/publish .

# Set environment variables
ENV DOTNET_ENVIRONMENT=Production
ENV CONFIG_PATH=/app/config
ENV LOG_PATH=/app/logs
ENV PROJECTS_PATH=/app/projects

# Run the app
ENTRYPOINT ["dotnet", "easysave.dll"] 