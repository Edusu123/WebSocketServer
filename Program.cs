var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseWebSockets();

app.MapGet("/", () => "Hello World!");

app.Run();
