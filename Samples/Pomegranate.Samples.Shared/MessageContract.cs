using Pomegranate.Contracts;

namespace Pomegranate.Samples.Shared
{
    public class MessageContract : IPomegranateContract
    {
        public string? Message { get; init; }

        public MessageContract() { }
        public MessageContract(string message) { Message = message; }
    }
}