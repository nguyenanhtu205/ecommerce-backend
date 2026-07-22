using UserService.API;
using UserService.Application;
using UserService.Infrastructure;
using UserService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
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
    ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.UseCors();

app.UseExceptionHandler();

app.UseRateLimiter();

app.MapOpenApi();

app.MapScalarApiReference();

app.Map("/", () => Results.Redirect("/scalar"));

app.MapEndpoints(typeof(Program).Assembly);

app.Run();
