using System;

namespace Pomegranate.Transport
{
    public abstract class PomegranateTransport
    {
        protected static void Drop(Guid id)
        {
            Controller.Drop(id);
        }

        protected static Guid Receive(byte[] buffer, IClientProxy clientProxy)
        {
            return Controller.ProcessBuffer(buffer, clientProxy);
        }
    }
}
