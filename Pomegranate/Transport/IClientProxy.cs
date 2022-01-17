using System.Threading.Tasks;

namespace Pomegranate.Transport
{
    /// <summary>
    /// Used by the transport implementation to expose the ability for the server to send data to a client
    /// </summary>
    public interface IClientProxy
    {
        /// <summary>
        /// Sends the serialized contract buffer to the client
        /// </summary>
        /// <param name="buffer">Serialized contract</param>
        public Task Send(byte[] buffer);
    }
}
