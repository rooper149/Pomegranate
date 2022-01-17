using System;

namespace Pomegranate.Serialization
{
    public sealed class UnknownSerializerException : Exception
    {
        public UnknownSerializerException(ulong sig) : base($@"Serializer with signature {sig} was not found.") { }
    }
}
