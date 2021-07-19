FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

ENV DynamoDb_LocalMode='true'

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN

WORKDIR /app

# Copy csproj and nuget config and restore as distinct layers
COPY ./CongigurationApi.sln ./
COPY ./CongigurationApi/CongigurationApi.csproj ./CongigurationApi/
COPY ./CongigurationApi.Tests/CongigurationApi.Tests.csproj ./CongigurationApi.Tests/
COPY /NuGet.Config /root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./CongigurationApi/CongigurationApi.csproj
RUN dotnet restore ./CongigurationApi.Tests/CongigurationApi.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out CongigurationApi.Tests/CongigurationApi.Tests.csproj

CMD dotnet test
