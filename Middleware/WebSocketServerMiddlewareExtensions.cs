namespace WebSocketServer.Middleware
{
  public static class WebSocketServerMiddlewareExtensions
  {
    public static IApplicationBuilder UseWebSocketServer(this IApplicationBuilder builder)
    {
      return builder.UseMiddleware<WebSocketServerMiddleware>();
    }

    public static IServiceCollection AddWebSocketServerConnectionManager(this IServiceCollection services)
    {
      services.AddSingleton<WebSocketServerConnectionManager>();
      return services;
    }
  }
}