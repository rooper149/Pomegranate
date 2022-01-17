using Pomegranate.Serialization;

namespace Pomegranate.Contracts
{
    public interface IPomegranateContract
    {
        public byte[] Serialize()
        {
            return SerializationUtil.Serialize(this);
        }

        public byte[] Serialize<T>() where T : ISerializer
        {
            return SerializationUtil.Serialize<T>(this);
        }

        public static IPomegranateContract? Deserialize(byte[] data)
        {
            return SerializationUtil.Deserialize(data);
        }

        public static T? Deserialize<T>(byte[] data) where T : IPomegranateContract
        {
            return (T?)SerializationUtil.Deserialize(data);
        }
    }
}
