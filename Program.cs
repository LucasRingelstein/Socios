using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Socios.Data;
using Socios.Options;
using Socios.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

#if DEBUG
builder.Configuration.AddUserSecrets<Program>();
#endif

builder.Services.Configure<MercadoPagoSettings>(builder.Configuration.GetSection("MercadoPago"));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=app.db";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<SubscriptionService>();
builder.Services.AddScoped<WebhookSignatureValidator>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Socios API",
        Version = "v1",
        Description = "Subscriptions and Mercado Pago integration"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
