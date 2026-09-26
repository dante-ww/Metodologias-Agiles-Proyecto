using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Kiosco.Api;
using Kiosco.Api.Services;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// --- Servicios ---

// 1. Conectar EF Core con MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta la cadena de conexion 'DefaultConnection' en appsettings.");
builder.Services.AddDbContext<KioscoContext>(options =>
    options.UseMySQL(connectionString));

// 2. Agregar controllers
builder.Services.AddControllers();

// 3. Swagger (para probar la API en el navegador)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Kiosco API",
        Version = "v1"
    });

    // CONFIGURACIÓN DE SEGURIDAD JWT PARA SWAGGER UI
    // Esto crea el botón para agregar el token JWT (de el dueño o distintos admins al logearse)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pega aquí el token recibido del login (sin la palabra Bearer)"
    });
});


// 4. CORS - Permitir que el frontend llame a esta API
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// 5. Autenticación JWT
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Falta Jwt:Key en appsettings")))
        };
    });

// 6. Registrar la autenticación
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

// --- Pipeline (qué pasa cuando llega una petición) ---

// Swagger para todos los entornos

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Redirigir la raíz a Swagger automáticamente
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Urls.Clear();
// Leer puerto desde variable de entorno (necesario para Render)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");


app.Run();
