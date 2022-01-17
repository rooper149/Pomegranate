using System;

namespace Pomegranate.Contracts
{
    /// <summary>
    /// Contract used to tell the server a node has requested one of this subscriptions be removed
    /// </summary>
    public sealed class UnsubscribeContract : PrimaryContract
    {
        /// <summary>
        /// The ID of the subscription to remove
        /// </summary>
        public Guid SubscriptionId { get; init; }

        public UnsubscribeContract() { }

        public UnsubscribeContract(Guid subscriptionId)
        {
            SubscriptionId = subscriptionId;
        }
    }
}
