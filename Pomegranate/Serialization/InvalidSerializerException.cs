using System;

namespace Pomegranate.Serialization
{
    public sealed class InvalidSerializerException<T> : Exception
    {
        public InvalidSerializerException() : base($@"{typeof(T)} is not a known serializer.") { }
    }
}
