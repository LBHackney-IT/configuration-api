# Use the official .NET 8 SDK image as the base image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Disable Microsoft telemetry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

# Set the working directory
WORKDIR /app

# Copy the solution and project files
COPY ./ConfigurationApi.sln ./
COPY ./ConfigurationApi/ConfigurationApi.csproj ./ConfigurationApi/
COPY ./ConfigurationApi.Tests/ConfigurationApi.Tests.csproj ./ConfigurationApi.Tests/
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config

# Restore the .NET dependencies for the projects
RUN dotnet restore ./ConfigurationApi/ConfigurationApi.csproj
RUN dotnet restore ./ConfigurationApi.Tests/ConfigurationApi.Tests.csproj

# Copy everything else and build
COPY . ./
RUN dotnet build -c Release -o out
RUN dotnet publish -c Release -o out

# Expose the port for the application
EXPOSE ${PORT:-3000}

# Set the entry point for the container
CMD ASPNETCORE_URLS=http://+:${PORT:-3000} dotnet ./out/ConfigurationApi.dll
