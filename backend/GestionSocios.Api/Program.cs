using GestionSocios.Api.Data;
using GestionSocios.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer; // Nuevo
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; // Nuevo
using Microsoft.OpenApi.Models; // Nuevo
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de Controladores y DbContext
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2. CONFIGURACIÓN DE SWAGGER (PASO 2) - Aquí es donde pones el bloque del candado
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GestionSocios API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. CONFIGURACIÓN DE AUTENTICACIÓN (Para que la API entienda el Token)
var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("Jwt:Key").Value!);
// Agregá esto antes de builder.Services.AddAuthentication(...)
builder.Services.AddIdentity<Socio, IdentityRole>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4; // Configuracion relajada de password
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

var app = builder.Build();

// 4. PIPELINE (ORDEN CRÍTICO)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// IMPORTANTE: Authentication va antes que Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();