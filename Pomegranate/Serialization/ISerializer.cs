using Pomegranate.Contracts;
using System.IO;

namespace Pomegranate.Serialization
{
    /// <summary>
    /// Pomegranate calls Activator.CreateInstance on all ISerializers to create
    /// instances. All you have to do is define the ISerializer in an assembly
    /// that is visible to Pomegranate, be sure to have a default ctor.
    /// </summary>
    public interface ISerializer
    {
        public IPomegranateContract? Deserialize(byte[] buffer);
        public byte[] Serialize(IPomegranateContract contract, MemoryStream buffer);
    }
}
