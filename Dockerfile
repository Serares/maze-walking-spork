# Multi-stage Dockerfile for MazeWalking application
# Combines React UI and .NET backend into a single container

# Stage 1: Build React UI
FROM node:20-alpine AS ui-builder

WORKDIR /app/ui

# Copy package files and install dependencies
COPY MazeWalking.UI/package*.json ./
RUN npm ci --only=production

# Copy source code and build
COPY MazeWalking.UI/ ./
RUN npm run build

# Stage 2: Build .NET application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dotnet-builder

WORKDIR /app

# Copy csproj and restore dependencies
COPY MazeWalking.Web/*.csproj ./MazeWalking.Web/
RUN dotnet restore MazeWalking.Web/MazeWalking.Web.csproj

# Copy all source code
COPY MazeWalking.Web/ ./MazeWalking.Web/

# Copy built React app to wwwroot
COPY --from=ui-builder /app/ui/dist ./MazeWalking.Web/wwwroot

# Publish the application
RUN dotnet publish MazeWalking.Web/MazeWalking.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 3: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

WORKDIR /app

# Copy published output from builder
COPY --from=dotnet-builder /app/publish .

# Create directories for logs and database
RUN mkdir -p /app/logs /app/db

# Expose port
EXPOSE 5180

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5180
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "MazeWalking.Web.dll"]
