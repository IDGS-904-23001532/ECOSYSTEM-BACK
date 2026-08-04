using Ecosystem_backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("./v1/swagger.json", "Ecosystem API V1");
});

// app.UseHttpsRedirection();

// Habilita el uso de archivos estáticos por defecto
app.UseStaticFiles();

// 1. Apuntar a la carpeta física "Uploads" en la raíz del proyecto
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

// 2. Configurar el proveedor de archivos para exponer la carpeta "Uploads"
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/Uploads" // Esto permite que la URL misitio.com/Uploads/foto.jpeg funcione
});

app.UseAuthorization();
app.MapControllers();
app.Run();