using Pomegranate.Contracts;
using System;

namespace Pomegranate
{
    /// <summary>
    /// Base subscription type used by the server and by ClientSubscription.
    /// This is used to define what contracts are allowed to be posted to a node,
    /// and on the client side where those contracts are sent via the callback
    /// </summary>
    public class Subscription
    {
        private readonly bool m_typeInheritance;
        private readonly bool m_pathInheritance;
        private readonly PomegranateNamespace m_hash;

        public readonly Guid Id;
        public readonly Guid NodeId;
        public readonly Type ContractType;

        public Subscription(Guid nodeId, Guid id, PomegranateNamespace hash, Type contractType, bool typeInheritance, bool namespaceInheritance)
        {
            Id = id;
            m_hash = hash;
            NodeId = nodeId;
            ContractType = contractType;
            m_typeInheritance = typeInheritance;
            m_pathInheritance = namespaceInheritance;
        }

        /// <summary>
        /// Validates if this subscription will accept the inbound contract
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        internal bool Validate(TransportContract contract)
        {
            if (!ValidateNamespace(contract.Hash) || !ContractType.IsInstanceOfType(contract.Contract) &&
                ContractType != contract.Contract?.GetType()) { return false; }

            if (!m_typeInheritance) { return contract.Contract.GetType() == ContractType; }
            return true;
        }

        /// <summary>
        /// Checks to see if this subscriber is within the inbound contracts's destination namespace
        /// </summary>
        private bool ValidateNamespace(PomegranateNamespace hash)
        {
            //if our namespace is at a deeper scope then the inbound message, then we obviously don't want it
            //if the namespace identifiers are not the same length and we are not inheriting, then we again don't want it
            var ilen = hash.Length;
            if (ilen < m_hash.Length || ((ilen != m_hash.Length) && !m_pathInheritance)) { return false; }
            return m_hash.Contains(hash);
        }
    }
}
