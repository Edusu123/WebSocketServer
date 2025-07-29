using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace WebSocketServer.Middleware
{
  public class WebSocketServerMiddleware(RequestDelegate next, WebSocketServerConnectionManager manager)
  {
    private readonly RequestDelegate _next = next;
    private readonly WebSocketServerConnectionManager _manager = manager;

    public async Task InvokeAsync(HttpContext context)
    {
      if (context.WebSockets.IsWebSocketRequest)
      {
        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

        Console.WriteLine("WebSocket Connected");

        string connID = _manager.AddSocket(webSocket);

        await SendConnIDAsync(webSocket, connID); //Call to new method here

        await Receive(webSocket, async (result, buffer) =>
        {
          if (result.MessageType == WebSocketMessageType.Text)
          {
            Console.WriteLine($"Receive->Text");
            Console.WriteLine($"Message: {Encoding.UTF8.GetString(buffer, 0, result.Count)}");
            await RouteJSONMessageAsync(Encoding.UTF8.GetString(buffer, 0, result.Count));
            return;
          }
          else if (result.MessageType == WebSocketMessageType.Close)
          {
            Console.WriteLine($"Receive->Close");

            string id = _manager.GetAllSockets().FirstOrDefault(s => s.Value == webSocket).Key;

            _manager.GetAllSockets().TryRemove(id, out WebSocket sock);

            await sock.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

            Console.WriteLine("Managed Connections: " + _manager.GetAllSockets().Count.ToString());

            return;
          }
        });
      }
      else
      {
        Console.WriteLine("Hello from 2nd Request Delegate - No WebSocket");
        await _next(context);
      }
    }

    private static async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
    {
      var buffer = new byte[1024 * 4];

      while (socket.State == WebSocketState.Open)
      {
        var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                                               cancellationToken: CancellationToken.None);

        handleMessage(result, buffer);
      }
    }

    private static async Task SendConnIDAsync(WebSocket socket, string connID)
    {
      var buffer = Encoding.UTF8.GetBytes("ConnID: " + connID);
      await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private async Task RouteJSONMessageAsync(string message)
    {

      var routeOb = JsonConvert.DeserializeObject<dynamic>(message);

      if (Guid.TryParse(routeOb.To.ToString(), out Guid guidOutput))
      {
        Console.WriteLine("Targeted");
        var sock = _manager.GetAllSockets().FirstOrDefault(s => s.Key == routeOb.To.ToString());
        if (sock.Value != null)
        {
          if (sock.Value.State == WebSocketState.Open)
            await sock.Value.SendAsync(Encoding.UTF8.GetBytes(routeOb.Message.ToString()), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        else
        {
          Console.WriteLine("Invalid Recipient");
        }
      }
      else
      {
        Console.WriteLine("Broadcast");
        foreach (var sock in _manager.GetAllSockets())
        {
          if (sock.Value.State == WebSocketState.Open)
            await sock.Value.SendAsync(Encoding.UTF8.GetBytes(routeOb.Message.ToString()), WebSocketMessageType.Text, true, CancellationToken.None);
        }
      }
    }

  }
}