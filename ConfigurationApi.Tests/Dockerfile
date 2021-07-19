FROM mcr.microsoft.com/dotnet/core/sdk:3.1

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN

WORKDIR /app

# Copy csproj and restore as distinct layers

COPY ./ConfigurationApi.sln ./
COPY ./ConfigurationApi/ConfigurationApi.csproj ./ConfigurationApi/
COPY ./ConfigurationApi.Tests/ConfigurationApi.Tests.csproj ./ConfigurationApi.Tests/
COPY ./NuGet.Config ./root/.nuget/NuGet/NuGet.Config

RUN dotnet restore ./ConfigurationApi/ConfigurationApi.csproj
RUN dotnet restore ./ConfigurationApi.Tests/ConfigurationApi.Tests.csproj

RUN dotnet restore ./ConfigurationApi/ConfigurationApi.csproj
RUN dotnet restore ./ConfigurationApi.Tests/ConfigurationApi.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out ConfigurationApi.Tests/ConfigurationApi.Tests.csproj

CMD dotnet test
