using Pomegranate.Samples.Shared;
using Pomegranate.Transport.WebSocket;

public class Program
{
    private static readonly Client _client = new(System.Net.IPAddress.Loopback, 5000);

    public static void Main(string[] args)
    {
        _client.Open();
        Console.WriteLine($@"Client ID: {_client.NodeId}");
        _client.Subscribe<MessageContract>(ReceiveMessage, $@"/{_client.NodeId}/message");//listen to messages from the server

        Task.Run(async () => 
        {
            for(; ; )
            {
                await Task.Delay(1000);
                _client.Publish(new MessageContract(@"Hello, Server!"), @"/message");
            }
        });

        Console.ReadLine();
    }

    private static void ReceiveMessage(Guid sender, MessageContract contract)
    {
        Console.WriteLine($@"FROM SERVER: {contract.Message}");
    }
}