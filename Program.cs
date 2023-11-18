using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// var connectionString = "Server=127.0.0.1;Port=5432;Database=udsubot;User Id=postgres;Password=eder432;";

builder.Services.AddScoped<DbService>();
builder.Services.AddDbContext<DataContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("ConnectionString")));
builder.Services.AddScoped<TelegramService>();
builder.Services.AddScoped<AppLogicService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var tg = scope.ServiceProvider.GetRequiredService<TelegramService>();
    var dbService = scope.ServiceProvider.GetRequiredService<DbService>();
    tg.Start();

}

app.MapGet("/", () => "Hello World!");

app.Run();