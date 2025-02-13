FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ENV DynamoDb_LocalMode='true'

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN

ARG SONAR_TOKEN
ENV SONAR_TOKEN=$SONAR_TOKEN

WORKDIR /app

RUN apt-get update && apt-get install -y openjdk-17-jdk
RUN dotnet tool install --global dotnet-sonarscanner --version 5.6.0
ENV PATH="$PATH:/root/.dotnet/tools"

RUN dotnet sonarscanner begin /k:"LBHackney-IT_configuration-api" /o:"lbhackney-it" /d:sonar.host.url=https://sonarcloud.io /d:sonar.login="${SONAR_TOKEN}"

# Copy csproj and nuget config and restore as distinct layers
COPY ./ConfigurationApi.sln ./
COPY ./ConfigurationApi/ConfigurationApi.csproj ./ConfigurationApi/
COPY ./ConfigurationApi.Tests/ConfigurationApi.Tests.csproj ./ConfigurationApi.Tests/
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./ConfigurationApi/ConfigurationApi.csproj
RUN dotnet restore ./ConfigurationApi.Tests/ConfigurationApi.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c Release -o out ConfigurationApi/ConfigurationApi.csproj
RUN dotnet build -c debug -o out ConfigurationApi.Tests/ConfigurationApi.Tests.csproj

CMD dotnet test
RUN dotnet sonarscanner end /d:sonar.login="${SONAR_TOKEN}"
