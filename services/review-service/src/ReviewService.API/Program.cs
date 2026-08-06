using ReviewService.API;
using ReviewService.Application;
using ReviewService.Infrastructure;
using ReviewService.Infrastructure.Data;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

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

app.Run();
