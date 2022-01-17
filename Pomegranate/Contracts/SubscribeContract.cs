using System;

namespace Pomegranate.Contracts
{
    /// <summary>
    /// Issues a subscription request to the server
    /// </summary>
    public sealed class SubscribeContract : PrimaryContract
    {
        /// <summary>
        /// The namespace hash
        /// </summary>
        public PomegranateNamespace Hash { get; init; }

        /// <summary>
        /// The subscription's ID
        /// </summary>
        public Guid SubscriptionId { get; init; }

        /// <summary>
        /// Determines whether or not children types weill be able to post to this subscription
        /// </summary>
        public bool TypeInheritance { get; init; }

        /// <summary>
        /// Determines whether of not we want to listen to parent namespaces
        /// </summary>
        public bool NamespaceInheritance { get; init; }

        /// <summary>
        /// The full name of the type that this subscription listens for
        /// </summary>
        public string? ContractAssemblyQualifiedName { get; init; }

        public Type GetContractType()
        {
            if (string.IsNullOrEmpty(ContractAssemblyQualifiedName)) { throw new ArgumentNullException(ContractAssemblyQualifiedName); }
            var type = Type.GetType(ContractAssemblyQualifiedName);
            if (type == null) { throw new Exception($@"Could not resolve {ContractAssemblyQualifiedName} as a known type!"); }
            return type;
        }

        public SubscribeContract() { }//all contracts must have a default .ctor

        public SubscribeContract(Guid subscriptionId, PomegranateNamespace hash, string contractType, bool typeInheritance, bool pathInheritance)
        {
            Hash = hash;
            SubscriptionId = subscriptionId;
            TypeInheritance = typeInheritance;
            NamespaceInheritance = pathInheritance;
            ContractAssemblyQualifiedName = contractType;
        }
    }
}
