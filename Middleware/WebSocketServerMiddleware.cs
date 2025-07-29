using System.Net.WebSockets;
using System.Text;

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
            return;
          }
          else if (result.MessageType == WebSocketMessageType.Close)
          {
            Console.WriteLine($"Receive->Close");

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

    private async Task SendConnIDAsync(WebSocket socket, string connID)
    {
      var buffer = Encoding.UTF8.GetBytes("ConnID: " + connID);
      await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }
  }
}