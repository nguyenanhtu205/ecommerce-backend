FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "services/review-service/src/ReviewService.API/ReviewService.API.csproj"
RUN dotnet publish "services/review-service/src/ReviewService.API/ReviewService.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet","ReviewService.API.dll"]