// ============================================
// 1. Program.cs SIMPLIFICADO (Sem Autenticação)
// ============================================
using Microsoft.EntityFrameworkCore;
using Olimpo.ProductAPI.Model.Context;
using Olimpo.ProductAPI.Repository;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// CONFIGURAÇÃO DE SERVIÇOS
// ============================================

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173") // Vite ou CRA
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Database
var connectionString = builder.Configuration.GetConnectionString("MySQLConnectionString");
builder.Services.AddDbContext<MySQLContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Olimpo Product API",
        Version = "v1",
        Description = "API de Produtos da Loja Olimpo"
    });
});

var app = builder.Build();

// ============================================
// MIDDLEWARE PIPELINE
// ============================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Olimpo Product API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp"); // Importante para o React
app.UseAuthorization();
app.MapControllers();

app.Run();

