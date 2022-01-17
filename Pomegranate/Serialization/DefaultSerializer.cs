using Pomegranate.Contracts;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Pomegranate.Serialization
{
    /// <summary>
    /// This is the default serializer that uses the built in DataContractSerializer
    /// </summary>
    internal sealed class DefaultSerializer : ISerializer
    {
        private static readonly DataContractSerializer _dataContractSerializer = new(typeof(IPomegranateContract), SerializationUtil.GetKnownTypes());

        public IPomegranateContract? Deserialize(byte[] buffer)
        {
            var reader = XmlDictionaryReader.CreateBinaryReader(buffer, XmlDictionaryReaderQuotas.Max);
            return (IPomegranateContract?)_dataContractSerializer.ReadObject(reader);
        }

        public byte[] Serialize(IPomegranateContract contract, MemoryStream buffer)
        {
            using var writer = XmlDictionaryWriter.CreateBinaryWriter(buffer);
            _dataContractSerializer.WriteObject(writer, contract);
            writer.Flush();
            return buffer.ToArray();
        }
    }
}
