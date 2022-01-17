namespace Pomegranate.Contracts
{
    /// <summary>
    /// Used by both the client and server to determine whether or 
    /// not the wrapped contract is to be posted to a particular subscription.
    /// </summary>
    public sealed class TransportContract : PrimaryContract
    {
        /// <summary>
        /// The namespace
        /// </summary>
        public PomegranateNamespace Hash { get; init; }

        /// <summary>
        /// The contract being sent over Pomegranate
        /// </summary>
        public IPomegranateContract? Contract { get; init; }

        /// <summary>
        /// Default .ctor
        /// </summary>
        public TransportContract() { }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="hash">The namespace path</param>
        /// <param name="contract">The contract being sent over FC6</param>
        public TransportContract(PomegranateNamespace hash, IPomegranateContract contract)
        {
            Hash = hash;
            Contract = contract;
        }
    }
}
