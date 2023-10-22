using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Formatting.Json;
using StoreApp.Models;
using StoreApp.Utilities;

var builder = WebApplication.CreateBuilder(args);


#region Context
builder.Services.AddDbContext<ProductContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DB_CONNECTION_STRING")));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("REDIS_CONNECTION_STRING");
});
#endregion

#region Logger
Log.Logger = new LoggerConfiguration()
                                .Enrich.FromLogContext()
                                .MinimumLevel.Is(Serilog.Events.LogEventLevel.Information)
                                .WriteTo.Console()
                                .WriteTo.File(
                                    new JsonFormatter(),
                                    Path.Combine(AppContext.BaseDirectory, "store-app.log"))
                                .CreateLogger();

builder.Host.UseSerilog();
#endregion

builder.Services.AddScoped<ICacheProvider, CacheProvider>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var productContext = scope.ServiceProvider.GetRequiredService<ProductContext>();
        productContext.Database.EnsureCreated();
    }

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
