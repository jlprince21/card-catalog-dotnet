FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

COPY . ./
RUN dotnet publish CardCatalog.Api -c Release -o CardCatalog.Api/out

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build-env /app/CardCatalog.Api/out .

ENTRYPOINT ["dotnet", "CardCatalog.Api.dll"]