using Pomegranate.Contracts;

namespace Pomegranate
{
    /// <summary>
    /// Used for subscriptions that implement the Post method (ClientSubscriptions, since they post to the callback)
    /// </summary>
    internal interface IPostableSubscription
    {
        internal void Post(TransportContract cntrlr);
    }
}
