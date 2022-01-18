using System;

namespace Pomegranate
{
    internal sealed class SubscriptionHandle : IPomegranateHandle
    {
        private readonly INode m_node;
        private bool m_disposed = false;
        private readonly Guid m_subscriptionId;

        public SubscriptionHandle(Guid subscriptionId, INode node)
        {
            m_node = node;
            m_subscriptionId = subscriptionId;
        }

        public void Dispose()
        {
            if (m_disposed) { return; }

            m_disposed = true;
            m_node.Unsubscribe(m_subscriptionId);
        }
    }
}
