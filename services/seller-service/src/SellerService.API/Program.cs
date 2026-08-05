using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SellerService.API;
using SellerService.API.Grpc;
using SellerService.Application;
using SellerService.Infrastructure;
using SellerService.Infrastructure.Data;

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

app.MapGrpcService<ShopGrpcServiceImpl>();

app.Run();
