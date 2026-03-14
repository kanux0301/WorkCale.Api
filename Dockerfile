FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/WorkCale.Api/WorkCale.Api.csproj", "src/WorkCale.Api/"]
COPY ["src/WorkCale.Application/WorkCale.Application.csproj", "src/WorkCale.Application/"]
COPY ["src/WorkCale.Domain/WorkCale.Domain.csproj", "src/WorkCale.Domain/"]
COPY ["src/WorkCale.Infrastructure/WorkCale.Infrastructure.csproj", "src/WorkCale.Infrastructure/"]
RUN dotnet restore "src/WorkCale.Api/WorkCale.Api.csproj"

COPY . .
RUN dotnet publish "src/WorkCale.Api/WorkCale.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WorkCale.Api.dll"]
