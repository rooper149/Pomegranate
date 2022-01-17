using Pomegranate.Client;
using Pomegranate.Hashing;
using System;
using System.Threading.Tasks;

namespace Pomegranate.Transport
{
    /// <summary>
    /// This is a node that can be used by the same process that hosts the Pomegranate transport.
    /// This node does not use network communications or the loopback device, it instead
    /// directly communicates with the transport implementation, eliminating any extra steps what would have
    /// been if this was a regular client node.
    /// </summary>
    public sealed class TransportNode : Node
    {
        public override async Task SendAsync(byte[] buffer)
        {
            //ehhh
            await Task.Run(() => Controller.ProcessBuffer(buffer));
        }

        public override IPomegranateHandle? Subscribe<T>(Action<Guid, T> callback, string path, bool typeInheritance = false, bool namespaceInheritance = true)
        {
            var id = Guid.NewGuid();
            var hash = HashUtil.GetHashSet(path);
            var subscription = new ClientSubscription<T>(callback, NodeId, id, hash, typeof(T), typeInheritance, namespaceInheritance);

            if (!Subscriptions.TryAdd(id, subscription)) { return null; }

            Controller.Subscribe(subscription);
            return new SubscriptionHandle(id, this);
        }
    }
}
