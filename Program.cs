using WebSocketServer.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebSocketServerConnectionManager();

var app = builder.Build();

app.UseWebSockets();
app.UseWebSocketServer(); ;

app.Run(async (context) =>
{
  Console.WriteLine("Hello from the third request delegate");
  await context.Response.WriteAsync("Hello from the third request delegate");
});

app.Run();