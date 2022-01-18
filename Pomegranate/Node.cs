using Pomegranate.Client;
using Pomegranate.Contracts;
using Pomegranate.Hashing;
using Pomegranate.Serialization;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Pomegranate
{
    /// <summary>
    /// A node represents a communication endpoint in Pomegranate - whether is be a server or a client node.
    /// Nodes hold a dictionary of subscriptions which are used to determine if a Contract is to be handled by
    /// this node.
    /// </summary>
    public abstract class Node : INode
    {
        private INode m_thisNode => this;

        /// <summary>
        /// The ID of the node
        /// </summary>
        public Guid NodeId { get; } = Guid.NewGuid();

        protected readonly ConcurrentDictionary<Guid, Subscription> Subscriptions = new();

        /// <summary>
        /// Closes the node and cleans up any resources that were created
        /// </summary>
        public void CloseNode()
        {
            SendAsync(((IPomegranateContract)new CloseNodeContract { SenderId = NodeId }).Serialize());
        }

        /// <summary>
        /// Publishes a contract to both all local subscribers and to the server for any other subscribers
        /// </summary>
        public async void Publish(IPomegranateContract contract, string path)
        {
            var hash = HashUtil.GetHashSet(path);//pre-compute hash
            await m_thisNode.PublishAsync<DefaultSerializer>(contract, hash);
        }

        /// <summary>
        /// Publishes a contract to both all local subscribers and to the server for any other subscribers
        /// </summary>
        public async void Publish<T>(IPomegranateContract contract, string path) where T : ISerializer
        {
            var hash = HashUtil.GetHashSet(path);//pre-compute hash
            await m_thisNode.PublishAsync<T>(contract, hash);
        }

        /// <summary>
        /// Publishes a contract to both all local subscribers and to the server for any other subscribers
        /// </summary>
        async Task INode.PublishAsync<T>(IPomegranateContract contract, PomegranateNamespace hash)
        {
            var transport = new TransportContract(hash, contract) { SenderId = NodeId };

            foreach (var subscription in Subscriptions)
            {
                if (subscription.Value is IPostableSubscription sub) { sub.Post(transport); }//passthrough without serialization
            }

            await SendAsync(((IPomegranateContract)transport).Serialize<T>());
        }

        /// <summary>
        /// Creates a subscription to accept contracts
        /// </summary>
        /// <typeparam name="T">The type this subscription will accept</typeparam>
        /// <param name="callback">The callback to send the inbound contract to</param>
        /// <param name="path">The namespace path to listen on</param>
        /// <param name="typeInheritance">Determines if child types are allowed to be posted to this subscription</param>
        /// <param name="namespaceInheritance">Determines if we are also listening to parent namespaces</param>
        /// <returns>The disposable handle</returns>
        public virtual IPomegranateHandle? Subscribe<T>(Action<Guid, T> callback, string path, bool typeInheritance = false, bool namespaceInheritance = false) where T : IPomegranateContract
        {
            var id = Guid.NewGuid();
            var hash = HashUtil.GetHashSet(path);
            var subscription = new ClientSubscription<T>(callback, NodeId, id, hash, typeof(T), typeInheritance, namespaceInheritance);

            if (!Subscriptions.TryAdd(id, subscription)) { return null; }

            var name = typeof(T).AssemblyQualifiedName;

            if (string.IsNullOrEmpty(name)) { return null; }

            var subCntrct = new SubscribeContract(id, hash, name, typeInheritance, namespaceInheritance) { SenderId = NodeId };
            SendAsync(((IPomegranateContract)subCntrct).Serialize());

            return new SubscriptionHandle(id, this);
        }

        /// <summary>
        /// Creates an observable that wraps a subscription
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="typeInheritance"></param>
        /// <param name="namespaceInheritance"></param>
        /// <param name="autoDispose"></param>
        /// <returns>The IObservable implementation</returns>
        public PomegranateObservable<T> GetObservable<T>(string path, bool typeInheritance = false, bool namespaceInheritance = false, bool autoDispose = false) where T : IPomegranateContract
        {
            return new PomegranateObservable<T>(this, path, typeInheritance, namespaceInheritance, autoDispose);
        }

        /// <summary>
        /// Unsubscribes a subscription from this node
        /// </summary>
        /// <param name="subscriptionId">The ID of the subscription to remove from the node</param>
        /// <returns>Returns true if the operation was successful</returns>
        bool INode.Unsubscribe(Guid subscriptionId)
        {
            if (!Subscriptions.TryRemove(subscriptionId, out _))
                return false;

            var subCntrct = new UnsubscribeContract(subscriptionId) { SenderId = NodeId };
            SendAsync(((IPomegranateContract)subCntrct).Serialize());

            return true;
        }


        public abstract Task SendAsync(byte[] buffer);

        public void Receive(byte[] buffer)
        {
            var contract = IPomegranateContract.Deserialize(buffer);
            if (contract is TransportContract transportContract) 
            {
                foreach (var subscription in Subscriptions)
                {
                    if (subscription.Value is IPostableSubscription sub) { sub.Post(transportContract); }
                }
            }
        }
    }
}
