using System;
using System.IO;
using System.Text.Json;
using Adq.Backend.Api.Application.Services;
using Adq.Backend.Domain.Ports;
using Adq.Backend.Infrastructure.DbContexts;
using Adq.Backend.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.SwaggerUI;

// Programa principal de la API
// --------------------------------------------------
// Este archivo configura servicios, dependencias, middleware
// y asegura la migración de la base de datos al iniciar.
// Se han añadido comentarios en español y manejo global de errores
// con logging para capturar y registrar excepciones no controladas.
// --------------------------------------------------

var builder = WebApplication.CreateBuilder(args);

// Configuración de logging (consola por defecto)
builder.Logging.AddConsole();

// Conexión a base de datos
var conn = builder.Configuration.GetConnectionString("SqlServer");
builder.Services.AddDbContext<AcquisitionDbContext>(options => options.UseSqlServer(conn));

// RUTA DE DATOS
// Construye la ruta donde se almacenan los datos en formato jsonl
var dataPath = Path.Combine(builder.Environment.ContentRootPath, "data", "acquisitions.jsonl");
Directory.CreateDirectory(Path.GetDirectoryName(dataPath)!);

// Registrar servicios y middlewares básicos
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Políticas CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocal", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        p => p
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// DI: registrar repositorios y servicio
// Repositorio de archivo como singleton (mantiene la misma instancia con la ruta)
builder.Services.AddSingleton<FileAcquisitionRepository>(new FileAcquisitionRepository(dataPath));

// Repositorio SQL como scoped
builder.Services.AddScoped<SqlServerAcquisitionRepository>();

// Proveer IAcquisitionRepository por defecto hacia el repo SQL
builder.Services.AddScoped<IAcquisitionRepository>(sp => sp.GetRequiredService<SqlServerAcquisitionRepository>());

// Servicio que requiere ambas implementaciones concretas
builder.Services.AddScoped<AcquisitionService>();

var app = builder.Build();

// Middleware CORS: se aplican las políticas definidas
app.UseCors("AllowLocal");
app.UseCors("AllowAngular");

// Asegura que la base de datos esté creada y migrada al iniciar
EnsureDatabaseCreatedAndMigrate(app);

// Middleware global para el manejo de excepciones no controladas
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        // Obtener logger y registrar el error
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Excepción no controlada atrapada por el middleware global.");

        // Devolver una respuesta JSON genérica de error
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new { error = "Ocurrió un error interno en el servidor." });
        await context.Response.WriteAsync(result);
    }
});

// Configuración de Swagger y Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Adq Backend API V1");
    c.RoutePrefix = string.Empty; // Servir Swagger UI en la raíz (/)
});

// Opcional: middleware para desarrollo (se puede ampliar si es necesario)
if (app.Environment.IsDevelopment())
{
    // Aquí se pueden agregar middlewares o servicios específicos de desarrollo
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Asegura que la base de datos esté migrada al iniciar la aplicación
static void EnsureDatabaseCreatedAndMigrate(IApplicationBuilder app)
{
    // Crear un scope para resolver servicios de DI
    using var scope = app.ApplicationServices.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var context = scope.ServiceProvider.GetRequiredService<AcquisitionDbContext>();
    try
    {
        // Ejecuta migraciones pendientes
        context.Database.Migrate();
        logger.LogInformation("Base de datos migrada correctamente.");
    }
    catch (Exception ex)
    {
        // Registrar el error durante la migración para diagnóstico
        logger.LogError(ex, "Error al migrar la base de datos: {Message}", ex.Message);
    }
}