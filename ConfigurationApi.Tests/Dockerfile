FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ENV DynamoDb_LocalMode='true'

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN

WORKDIR /app

# Copy everything else and build
COPY . .

RUN dotnet restore ./ConfigurationApi/ConfigurationApi.csproj
RUN dotnet restore ./ConfigurationApi.Tests/ConfigurationApi.Tests.csproj

RUN dotnet build -c Release -o out ConfigurationApi/ConfigurationApi.csproj
RUN dotnet build -c debug -o out ConfigurationApi.Tests/ConfigurationApi.Tests.csproj

ENTRYPOINT ["dotnet", "test", "--collect", "XPlat Code Coverage;Format=opencover", "--results-directory", "./coverage"]
