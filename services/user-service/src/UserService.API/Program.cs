using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using UserService.API;
using UserService.API.Grpc;
using UserService.Application;
using UserService.Infrastructure;
using UserService.Infrastructure.Data;

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

app.MapGrpcService<UserGrpcServiceImpl>();

app.Run();
