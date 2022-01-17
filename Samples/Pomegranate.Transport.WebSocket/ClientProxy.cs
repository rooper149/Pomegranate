using System.Net.WebSockets;

namespace Pomegranate.Transport.WebSocket
{
    internal sealed class ClientProxy : IClientProxy
    {
        private readonly System.Net.WebSockets.WebSocket m_socket;

        internal ClientProxy(System.Net.WebSockets.WebSocket socket)
        {
            m_socket = socket;
        }

        public async Task Send(byte[] buffer)
        {
            await m_socket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Binary, true, CancellationToken.None);
        }
    }
}