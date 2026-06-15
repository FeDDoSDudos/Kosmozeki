FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Kosmozeki.Api/Kosmozeki.Api.csproj Kosmozeki.Api/
COPY Kosmozeki.Application/Kosmozeki.Application.csproj Kosmozeki.Application/
COPY Kosmozeki.Contracts/Kosmozeki.Contracts.csproj Kosmozeki.Contracts/
COPY Kosmozeki.Core/Kosmozeki.Core.csproj Kosmozeki.Core/
COPY Kosmozeki.Domain/Kosmozeki.Domain.csproj Kosmozeki.Domain/
COPY Kosmozeki.Infrastructure/Kosmozeki.Infrastructure.csproj Kosmozeki.Infrastructure/
RUN dotnet restore Kosmozeki.Api/Kosmozeki.Api.csproj

COPY . .
RUN dotnet publish Kosmozeki.Api/Kosmozeki.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 5010
ENV ASPNETCORE_URLS=http://0.0.0.0:5010
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Kosmozeki.Api.dll"]
