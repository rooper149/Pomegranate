using Pomegranate.Samples.Shared;
using Pomegranate.Transport;
using Pomegranate.Transport.WebSocket;

public class Program
{
    static readonly TransportNode _node = new();

    public static void Main(string[] args)
    {
        var server = new Server(System.Net.IPAddress.Loopback, 5000);
        server.Run();

        _node.Subscribe<MessageContract>(ReceiveMessage, @"/message");
        Console.ReadLine();
    }

    private static void ReceiveMessage(Guid sender, MessageContract contract)
    {
        Console.WriteLine($@"FROM CLIENT: {contract.Message}");
        _node.Publish(new MessageContract(@"Hello, Client!"), $@"/{sender}/message");//send a message back to the sender
    }
}
