FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "services/product-catalog-service/src/ProductCatalogService.API/ProductCatalogService.API.csproj"
RUN dotnet publish "services/product-catalog-service/src/ProductCatalogService.API/ProductCatalogService.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet","ProductCatalogService.API.dll"]