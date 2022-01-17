using System.Net.WebSockets;

namespace Pomegranate.Transport.WebSocket
{
    internal sealed class Transport : PomegranateTransport
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection _)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseWebSockets();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == @$"/{Constants.NAMESPACE_PREFIX}")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        using var socket = await context.WebSockets.AcceptWebSocketAsync();
                        await Proc(socket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }
            });
        }

        private static async Task Proc(System.Net.WebSockets.WebSocket socket)
        {
            var nodeId = Guid.Empty;
            WebSocketReceiveResult? result = null;
            var clientProxy = new ClientProxy(socket);
            var buffer = Pools._BUFFER_POOL.Rent(1316);
            var ms = Pools._MEMORY_STREAM_POOL.GetStream();

            try
            {
                result = await socket.ReceiveAsync(buffer, CancellationToken.None);

                while (!result.CloseStatus.HasValue)
                {
                    ms.Write(buffer, 0, result.Count);

                    if (result.EndOfMessage)
                    {
                        if (nodeId.Equals(Guid.Empty)) { nodeId = Receive(ms.ToArray(), clientProxy); }
                        else { Receive(ms.ToArray(), clientProxy); }

                        ms.SetLength(0);
                    }

                    result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                }
            }
            catch (Exception ex) { Console.WriteLine(ex); }
            finally
            {
                Drop(nodeId);
                await ms.DisposeAsync();
                Pools._BUFFER_POOL.Return(buffer);
                if (result is not null)
                {
                    await socket.CloseAsync(result.CloseStatus!.Value, result.CloseStatusDescription, CancellationToken.None);
                }
            }
        }
    }
}
