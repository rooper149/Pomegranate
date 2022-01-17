using Pomegranate.Contracts;
using Pomegranate.Transport;

public class MessageContract : IPomegranateContract
{
    public string? Message { get; init; }

    public MessageContract() { }
    public MessageContract(string message) { Message = message; }
}

public class Program
{
    private static volatile int _count = 0;
    private static readonly TransportNode _node = new();
    private static readonly MessageContract _testContract = new(@"Hello, World!");

    public static void Main(string[] args)
    {     
        //using var handle = _node.Subscribe<MessageContract>(ReceivedMessage, nameof(MessageContract.Message));
        using var handle = _node.GetObservable<MessageContract>(nameof(MessageContract.Message));
        handle.Subscribe(x => ReceivedMessage(x));

        var timer = new System.Timers.Timer(5000);
        timer.Elapsed += Timer_Elapsed;
        timer.Start();

        for(int i = 0; i < 3; i++)
            Loop();
      
        Console.ReadLine();
    }

    private static async void Loop()
    {
        await Task.Run(() =>
        {
            for (; ; )
            {
                _node.Publish(_testContract, nameof(MessageContract.Message));
            }
        });
    }

    private static void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        Console.WriteLine($@"Contracts per second: {_count/5.0}");
        _count = 0;
    }

    private static async void ReceivedMessage(MessageContract contract)
    {
        await Task.Run(() => _count++);
    }
}
