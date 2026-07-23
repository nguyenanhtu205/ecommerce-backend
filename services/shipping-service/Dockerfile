FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "services/shipping-service/src/ShippingService.API/ShippingService.API.csproj"
RUN dotnet publish "services/shipping-service/src/ShippingService.API/ShippingService.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet","ShippingService.API.dll"]