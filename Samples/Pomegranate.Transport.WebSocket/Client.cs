using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;

namespace Pomegranate.Transport.WebSocket
{
    public class Client : Node, IDisposable
    {
        private readonly int m_port;
        private bool m_disposed = false;
        private ClientWebSocket? m_socket;
        private readonly IPAddress m_endpoint;

        public Client(IPAddress endpoint, int port)
        {
            m_port = port;
            m_endpoint = endpoint;
        }

        public void Open()
        {
            m_socket = new ClientWebSocket();

            try
            {
                m_socket.ConnectAsync(new Uri($@"ws://{m_endpoint}:{m_port}/{Constants.NAMESPACE_PREFIX}"), CancellationToken.None).Wait();
                BeginReceive();
            }
            catch { throw; }
        }

        public async void Close()
        {
            if (m_socket is null) { throw new NullReferenceException(nameof(m_socket)); }
            await m_socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }

        public override async Task SendAsync(byte[] buffer)
        {
            if (m_socket is null) { throw new NullReferenceException(nameof(m_socket)); }
            if (m_socket.State != WebSocketState.Open) { throw new WebSocketException(WebSocketError.Faulted); }
            await m_socket.SendAsync(buffer, WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        public void Dispose()
        {
            if (m_disposed) { return; }

            m_disposed = true;
            m_socket?.Dispose();
            GC.SuppressFinalize(this);
        }

        private async void BeginReceive()
        {
            var buffer = Pools._BUFFER_POOL.Rent(1316);
            var ms = Pools._MEMORY_STREAM_POOL.GetStream();

            try
            {
                if (m_socket is null) { throw new NullReferenceException(nameof(m_socket)); }
                var result = await m_socket.ReceiveAsync(buffer, CancellationToken.None);

                while (!result.CloseStatus.HasValue)
                {
                    ms.Write(buffer, 0, result.Count);

                    if (result.EndOfMessage)
                    {
                        Receive(ms.ToArray());//ToArray trims the data for us, but makes a copy while doing so, maybe there is a btter way to do this using the GetBuffer method
                        ms.SetLength(0);
                    }

                    result = await m_socket.ReceiveAsync(buffer, CancellationToken.None);
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex); }
            finally
            {
                await ms.DisposeAsync();
                Pools._BUFFER_POOL.Return(buffer);
            }
        }   
    }
}
