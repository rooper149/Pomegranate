using Pomegranate.Contracts;
using System;
using System.Diagnostics;

namespace Pomegranate.Client
{
    /// <summary>
    /// This is a subscription only used for clients as it holds a callback which is fired if the post conditions are valid
    /// </summary>
    /// <typeparam name="T">The Contract type to subscribe to</typeparam>
    internal sealed class ClientSubscription<T> : Subscription, IPostableSubscription where T : IPomegranateContract
    {
        private readonly Action<Guid, T> m_callback;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="callback">The callback to be fired when a contract is received that matches this subscription's conditions</param>
        /// <param name="nodeId">The ID of the node that owns this subscription</param>
        /// <param name="id">The ID of this subscription</param>
        /// <param name="hash">The namespace we are subscribing to</param>
        /// <param name="contractType">The type of contract we are listening for</param>
        /// <param name="typeInheritance">Determines if children types will be allowed to be posted</param>
        /// <param name="namespaceInheritance">Determines if we want to listen to the parent namespaces as well</param>
        internal ClientSubscription(Action<Guid, T> callback, Guid nodeId, Guid id, PomegranateNamespace hash, Type contractType, bool typeInheritance, bool namespaceInheritance) :
            base(nodeId, id, hash, contractType, typeInheritance, namespaceInheritance)
        {
            m_callback = callback;
        }

        /// <summary>
        /// Posts a message to this subscription (i.e. calls the callback) if the inbound contract meets our requirements
        /// </summary>
        /// <param name="contract">The inbound contract wrapped in a Transport Contract</param>
        void IPostableSubscription.Post(TransportContract contract)
        {
            if (!Validate(contract)) { return; }

            if (contract.Contract is T tmp) { m_callback(contract.SenderId, tmp); }
            else { Debug.Assert(false); }
        }
    }
}
