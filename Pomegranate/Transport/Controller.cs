using Pomegranate.Contracts;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Pomegranate.Transport
{
    internal static class Controller
    {
        private static readonly ConcurrentDictionary<Guid, RegisteredNode> _nodes = new();

        internal static Guid ProcessBuffer(byte[] buffer, IClientProxy? clientProxy = null)
        {
            var contract = IPomegranateContract.Deserialize(buffer);
            if (contract is null) { throw new NullReferenceException(nameof(contract)); }
            return ProcessContract(contract, clientProxy);
        }

        private static Guid ProcessContract(IPomegranateContract contract, IClientProxy? clientProxy)
        {
            if(contract is not PrimaryContract primaryContract) { throw new InvalidOperationException(@"All inbound contracts must be PrimaryContracts"); }

            //check for primary contracts
            switch (primaryContract)
            {
                case SubscribeContract sub:
                    Subscribe(sub, clientProxy);
                    break;
                case UnsubscribeContract usub:
                    Unsubscribe(usub);
                    break;
                case CloseNodeContract cnc:
                    CloseNode(cnc);
                    break;
                case TransportContract transport:
                    Publish(transport);
                    break;
                default:
                    Debug.Assert(false);//not good
                    break;
            }

            return primaryContract.SenderId;
        }

        internal static void Drop(Guid id)
        {
            _nodes.TryRemove(id, out _);
        }

        /// <summary>
        /// Publishes a contract to all listening nodes
        /// </summary>
        /// <param name="contract">The contract to publish wrapped in a transport controller</param>
        private static void Publish(TransportContract contract)
        {
            foreach(var node in _nodes)
            {
                node.Value.Post(contract);
            }
        }

        /// <summary>
        /// Adds a subscription to the parent node. If the node is not yet registered it will be after this call.
        /// </summary>
        /// <param name="info">The subscribe contract</param>
        /// <param name="clientProxy"></param>
        private static void Subscribe(SubscribeContract info, IClientProxy? clientProxy)
        {
            if (clientProxy is null) { throw new ArgumentNullException(nameof(clientProxy)); }
            var node = _nodes.GetOrAdd(info.SenderId, new RegisteredNode(info.SenderId, clientProxy));
            node.Subscribe(info);
        }

        /// <summary>
        /// Adds a subscription to the parent node, if the node is not yet registered it will be after this.
        /// This Subscribe method is only used internally by the HostNode class. This allows the host node
        /// To receive inbound contracts directly and not have to go through the loopback device.
        /// </summary>
        /// <param name="info">The subscription object</param>
        internal static void Subscribe(Subscription info)
        {
            var node = _nodes.GetOrAdd(info.NodeId, new RegisteredNode(info.NodeId));
            node.Subscribe(info);
        }

        /// <summary>
        /// Removes the specified subscription from the requesting node.
        /// </summary>
        /// <param name="info">The unsubscribe contract</param>
        /// <returns>Returns true if the operation completed successfully.</returns>
        private static void Unsubscribe(UnsubscribeContract info)
        {
            _nodes.TryGetValue(info.SenderId, out var node);
            node?.Unsubscribe(info);
        }

        /// <summary>
        /// Removes a node from the server.
        /// </summary>
        /// <param name="info"></param>
        /// <returns>Returns true is the operation was successful.</returns>
        private static void CloseNode(CloseNodeContract info)
        {
            _nodes.TryRemove(info.SenderId, out _);
        }
    }
}
