using System;

namespace Pomegranate
{
    internal sealed class ObserverHandle<T> : IPomegranateHandle
    {
        private readonly T m_observer;
        private bool m_disposed = false;
        private readonly Action<T> m_disposalAction;

        public ObserverHandle(Action<T> disposalAction, T observer)
        {
            m_observer = observer;
            m_disposalAction = disposalAction;
        }

        public void Dispose()
        {
            if (m_disposed) { return; }

            m_disposed = true;
            m_disposalAction.Invoke(m_observer);
        }
    }

}
