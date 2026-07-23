FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "services/promotion-service/src/PromotionService.API/PromotionService.API.csproj"
RUN dotnet publish "services/promotion-service/src/PromotionService.API/PromotionService.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet","PromotionService.API.dll"]