using System;

namespace Pomegranate.Contracts
{
    public abstract class PrimaryContract : IPomegranateContract
    {
        public Guid SenderId { get; init; }
    }
}
