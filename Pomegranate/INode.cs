using Pomegranate.Contracts;
using Pomegranate.Serialization;
using System;
using System.Threading.Tasks;

namespace Pomegranate
{
    public interface INode
    {
        /// <summary>
        /// The ID of the node
        /// </summary>
        Guid NodeId { get; }

        /// <summary>
        /// Closes the node and cleans up any resources that were created
        /// </summary>
        void CloseNode();

        /// <summary>
        /// Publishes a contract to both all local subscribers and to the server for any other subscribers
        /// </summary>
        /// <param name="contract"></param>
        /// <param name="path"></param>
        /// <returns>Task completed</returns>
        void Publish(IPomegranateContract contract, string path);

        /// <summary>
        /// Publishes a contract to both all local subscribers and to the server for any other subscribers
        /// </summary>
        /// <param name="contract"></param>
        /// <param name="path"></param>
        /// <returns>Task completed</returns>
        void Publish<T>(IPomegranateContract contract, string path) where T : ISerializer;

        /// <summary>
        /// Publishes a contract to both all local subscribers and to the server for any other subscribers
        /// </summary>
        /// <param name="contract"></param>
        /// <param name="hash"></param>
        internal Task PublishAsync<T>(IPomegranateContract contract, PomegranateNamespace hash) where T : ISerializer;

        /// <summary>
        /// Creates a subscription to accept contracts
        /// </summary>
        /// <typeparam name="T">The type this subscription will accept</typeparam>
        /// <param name="callback">The callback to send the inbound contract to</param>
        /// <param name="path">The namespace path to listen on</param>
        /// <param name="typeInheritance">Determines if child types are allowed to be posted to this subscription</param>
        /// <param name="namespaceInheritance">Determines if we are also listening to parent namespaces</param>
        /// <returns>Returns the ID of the subscription.</returns>
        IPomegranateHandle? Subscribe<T>(Action<Guid, T> callback, string path, bool typeInheritance = false, bool namespaceInheritance = false) where T : IPomegranateContract;

        /// <summary>
        /// Creates an observable that wraps a subscription
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="typeInheritance"></param>
        /// <param name="namespaceInheritance"></param>
        /// <returns>The IObservable implementation</returns>
        PomegranateObservable<T> GetObservable<T>(string path, bool typeInheritance = false, bool namespaceInheritance = false) where T : IPomegranateContract;

        /// <summary>
        /// Unsubscribes a subscription from this node
        /// </summary>
        /// <param name="subscriptionId">The ID of the subscription to remove from the node</param>
        /// <returns>Returns true if the operation was successful</returns>
        internal bool Unsubscribe(Guid subscriptionId);

        /// <summary>
        /// Sends the serialized contract through the transport implementation
        /// </summary>
        /// <param name="buffer">The serialized contract buffer</param>
        /// <returns>Task completed</returns>
        public Task SendAsync(byte[] buffer);

        /// <summary>
        /// Receives the serialized contract through the transport implementation
        /// </summary>
        /// <param name="buffer">The serialized contract buffer</param>
        /// <returns>Task completed</returns>
        public void Receive(byte[] buffer);
    }
}
