using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseWebSockets();

app.Use(async (context, next) =>
{
  WriteRequestParam(context);

  if (context.WebSockets.IsWebSocketRequest)
  {
    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
    Console.WriteLine("WebSocket Connected");
  }
  else
  {
    Console.WriteLine("Hello from the second request delegate");
    await next();
  }
});

app.MapGet("/", () => "Hello World!");


app.Run(async (context) =>
{
  Console.WriteLine("Hello from the third request delegate");
  await context.Response.WriteAsync("Hello from the third request delegate");
});

app.Run();

void WriteRequestParam(HttpContext context)
{
  Console.WriteLine("Request method: {0}", context.Request.Method);
  Console.WriteLine("Request protocol: {0}", context.Request.Protocol);

  if (context.Request.Headers != null)
  {
    foreach (var h in context.Request.Headers)
    {
      Console.WriteLine("--> {0}:{1}", h.Key, h.Value);
    }
  }
}
