using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Olimpo.ProductAPI.Model.Context;
using Olimpo.ProductAPI.Repository;
using Olimpo.ProductAPI.Services;

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
builder.Services.AddSingleton<IMapper>(_ =>
{
    var config = new MapperConfiguration(cfg =>
    {
        cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
    });

    return config.CreateMapper();
});

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IStockReservationRepository, StockReservationRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddHttpClient<IMercadoPagoService, MercadoPagoService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHostedService<StockReservationCleanupService>();
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
app.UseCors("AllowReactApp");
app.UseAuthorization();
app.MapControllers();

app.Run();

