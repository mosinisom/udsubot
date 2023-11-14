using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = "Server=127.0.0.1;Port=5432;Database=udsubot;User Id=postgres;Password=eder432;";

builder.Services.AddScoped<DbService>();
builder.Services.AddDbContext<DataContext>(options => options.UseNpgsql(connectionString));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbService = scope.ServiceProvider.GetRequiredService<DbService>();
    var user = await dbService.GetUserByChatIdAsync(1001);
    Console.WriteLine(user?.StudentCardNumber);
}

app.MapGet("/", () => "Hello World!");

app.Run();