using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;

namespace Pomegranate.Transport.WebSocket
{
    public class Server : IDisposable
    {
        private readonly IHost m_host;
        private bool m_disposed = false;

        public Server(IPAddress endpoint, int port, string[]? args = null)
        {
            m_host = CreateHostBuilder(endpoint, port, args).Build();
        }

        private static IHostBuilder CreateHostBuilder(IPAddress endpoint, int port, string[]? args) => Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.ConfigureKestrel(options =>
            {
                options.Listen(endpoint, port, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;//whatever else you would need or want to change
                                                                          //also would need to make secure/hook up to authentication
                                                                          //listenOptions.UseHttps("<path to .pfx file>", 
                                                                          //"<certificate password>");
                });
            });

            webBuilder.UseStartup<Transport>();
        });

        public void Run() { m_host?.RunAsync(); }
        public void Shutdown() { m_host?.StopAsync(); }

        public void Dispose()
        {
            if (m_disposed) { return; }

            m_disposed = true;
            m_host?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
