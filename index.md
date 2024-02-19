## Welcome to Pomegranate
Pomegranate is an extreemly flexible Pub/Sub framework designed for any type of transport, or even no transport at all. You can use Pomegranate locally, write a transport that uses Unix/Windows sockets for IPC, or use WebSockets for going across the WAN. The method of transport it entirely up to you. 

Pomegranate also allows you to bring your own serialization. By default Pomegranate provides a serializer that uses `DataContractSerializer` and doesn't require any kind of attributes for serialization, you can simply send any object that implements `IPomegranateContract` over Pomegranate, so long as the receiving end shares the same object definition (likely in some kind of shared libary).

### Quick Start

Here are some really basic examples that show how use Pomegranate.

Let's create a new contract type that is shared between all projects use Pomegranate for communication, we could put this in a shared libary
```csharp
public class MessageContract : IPomegranateContract
{
    public string? Message { get; init; }//a simple message

    public MessageContract() { }//default .ctor required
    public MessageContract(string message) { Message = message; }
}
```

Now let's subscribe to a particular ContractType and Namespace

```csharp
public class ExampleNode
{
    private readonly INode m_node;

    public ExampleNode(INode node)
    {
        m_node = node;
        
        //Here we subscribe to all MessageContracts within the /msg namespace.
        //Strings are used to define namespaces, but they are converted into hashes
        //to avoid any type of string comparisons and string manipulations
        var handle = m_node.Subscribe<MessageContract>(ReceiveMessage, @"/msg", typeInheritance: false, namespaceInheritance: false);//Subscribe<T> returns an IDisposable handle that is used to end the subscription when you no longer need it
        
        //and while you have to use a string to define the Namespace, it is probably best to come of with a way to use
        //deterministic strings, entirely removing the need for "magic strings". A simple example here might be to replace
        //@"/msg" with nameof(MessageContract).
        
        //as optional parameters you can specify whether or not want to listen for sub-types, 
        //as well as whether or not we want to listen to parent namespaces. The default is false
        //for both options.
    }

    //Whenever a contract of MessageContract type is received on the @"/msg", this method will be called.
    //Feel free to make subsciptions asynchronous if needed. 
    private void ReceiveMessage(Guid sender, MessageContract contract)
    {
        Console.WriteLine(contract.Message);
    }
}
```

You can also use the Observable/Observer pattern which makes it easy to use with ReactiveUI:

```csharp
IObservable<MessageContract> observable = m_node.GetObservable<MessageContract>(@"/msg", typeInheritance: false, namespaceInheritance: false, autoDispose: false);

//GetObservable returns a PomegranateObservable<T> type which implements IObservable<T> as well as IDisposable which is not normal for Observables.
//It implements IDisposable because it wraps a Pomegranate subscription which uses an IDisposable handle to know when to close the subscription.
//There is an optional autoDispose parameter that defaults to false. If set to true the PomegranateObservable will call dispose when the last subscriber as been removed.
```


There is a "Samples" directory in the repository that contains a working sample of using Pomegranate over WebSockets for a more in-depth example.
