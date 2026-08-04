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
app.UseStaticFiles();

var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

if (!Directory.Exists(webRootPath))
{
    Directory.CreateDirectory(webRootPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(webRootPath),
    RequestPath = "" // Permite que la URL sea directa: misitio.com/Uploads/foto.png
});

app.UseAuthorization();

app.MapControllers();

app.Run();
