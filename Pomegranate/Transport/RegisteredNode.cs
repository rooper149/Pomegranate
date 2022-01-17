using Pomegranate.Contracts;
using System;
using System.Collections.Concurrent;

namespace Pomegranate.Transport
{
    /// <summary>
    /// The registered node is a server side representation of a client node.
    /// </summary>
    internal sealed class RegisteredNode
    {
        private readonly Guid m_nodeId;
        private readonly IClientProxy? m_clientProxy;
        private readonly ConcurrentDictionary<Guid, Subscription> m_subscriptions = new();

        internal RegisteredNode(Guid id)
        {
            m_nodeId = id;
        }

        internal RegisteredNode(Guid id, IClientProxy clientProxy)
        {
            m_nodeId = id;
            m_clientProxy = clientProxy;
        }

        internal void Subscribe(SubscribeContract contract)
        {
            var sub = new Subscription(contract.SenderId, contract.SubscriptionId, contract.Hash, contract.GetContractType(), contract.TypeInheritance, contract.NamespaceInheritance);
            m_subscriptions.AddOrUpdate(contract.SubscriptionId, sub, (_, _) => sub);
        }

        internal void Subscribe(Subscription subscription)
        {
            m_subscriptions.AddOrUpdate(subscription.Id, subscription, (_, _) => subscription);
        }

        internal bool Unsubscribe(UnsubscribeContract contract)
        {
            return m_subscriptions.TryRemove(contract.SubscriptionId, out _);//discard the removed subscription
        }

        internal async void Post(TransportContract contract)
        {
            if (contract.SenderId == m_nodeId) { return; }//don't need to loop back

            foreach (var (_, value) in m_subscriptions)
            {
                if (!value.Validate(contract)) continue;
                if (value is IPostableSubscription tmp) { tmp.Post(contract); }//only for server side subs
                else if(m_clientProxy is not null) { await m_clientProxy.Send(((IPomegranateContract)contract).Serialize()); }//post to the client
                return;
            }
        }
    }
}
