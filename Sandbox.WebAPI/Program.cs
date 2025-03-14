using Microsoft.EntityFrameworkCore;
using Sandbox.Infrastructure.Data;
using Sandbox.Application.Services;
using Sandbox.Core.Interfaces;
using Sandbox.Infrastructure.Services;
using Sandbox.WebAPI.Middleware;
using Sandbox.WebAPI.Services;
using Sandbox.Application.Mapping; 

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddKeyedScoped<IOrderService, SpotTradingService>("Spot");
builder.Services.AddKeyedScoped<IOrderService, MarginTradingService>("Margin");
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddSingleton<WalletWebSocketService>();
builder.Services.AddHostedService<BackgroundTrackingService>();
builder.Services.AddSingleton<BackgroundTrackingService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sandbox API v1");
        c.RoutePrefix = "swagger"; 
    });
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseWebSockets();
app.MapControllers();
app.MapGet("/ws/wallet", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var walletId = Guid.Parse(context.Request.Query["walletId"]);
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var walletWebSocketService = context.RequestServices.GetRequiredService<WalletWebSocketService>();
        await walletWebSocketService.HandleWebSocketAsync(webSocket, walletId);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});


using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider.GetRequiredService<BackgroundTrackingService>();
    await service.StartTrackingForRunAsync();
}

app.Run();

