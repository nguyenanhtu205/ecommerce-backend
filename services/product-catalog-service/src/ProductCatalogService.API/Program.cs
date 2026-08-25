using Microsoft.AspNetCore.Server.Kestrel.Core;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using ProductCatalogService.API;
using ProductCatalogService.API.Grpc;
using ProductCatalogService.Application;
using ProductCatalogService.Infrastructure;
using ProductCatalogService.Infrastructure.Data;
using Scalar.AspNetCore;

BsonSerializer.RegisterSerializer(
    new DateTimeOffsetSerializer(BsonType.DateTime));

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080, o =>
    {
        o.Protocols = HttpProtocols.Http1;
    });

    options.ListenAnyIP(8081, o =>
    {
        o.Protocols = HttpProtocols.Http2;
    });
});

builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddApiServices();

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    MongoIndexInitializer initializer = scope.ServiceProvider.GetRequiredService<MongoIndexInitializer>();
    await initializer.InitializeAsync();
}

app.UseCors();

app.UseExceptionHandler();

app.UseRateLimiter();

app.MapOpenApi();

app.MapScalarApiReference();

app.Map("/", () => Results.Redirect("/scalar"));

app.MapEndpoints(typeof(Program).Assembly);

app.MapGrpcService<ProductGrpcServiceImpl>();

app.Run();
