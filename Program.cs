var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<DbService>();


var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
