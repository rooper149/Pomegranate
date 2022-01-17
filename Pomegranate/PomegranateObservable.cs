using Pomegranate.Contracts;
using System;
using System.Collections.Concurrent;

namespace Pomegranate
{
    //currently the observable itself is IDisposable since it holds the actual subscription on the node
    //maybe it's better to just have each observable wrap its own subscription?
    public class PomegranateObservable<T> : IObservable<T>, IPomegranateHandle where T : IPomegranateContract
    {
        private bool m_disposed = false;
        private readonly bool m_autoDispose;
        private readonly IPomegranateHandle? m_disposable;
        private readonly ConcurrentDictionary<IObserver<T>, IDisposable> m_observers = new();

        public PomegranateObservable(INode node, string path, bool typeInheritance = false, bool namespaceInheritance = false, bool autoDispose = false)
        {
            m_autoDispose = autoDispose;
            m_disposable = node.Subscribe<T>(Do, path, typeInheritance, namespaceInheritance);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return m_observers.GetOrAdd(observer, new ObserverHandle<IObserver<T>>(RemoveObserver, observer));
        }

        private void Do(Guid sender, T contract)
        {
            foreach(var observer in m_observers) { observer.Key.OnNext(contract); }
        }

        private void RemoveObserver(IObserver<T> observer)
        {
            m_observers.TryRemove(observer, out _); 
            observer.OnCompleted();

            if(m_autoDispose && m_observers.Count == 0) { Dispose(); }
        }

        public void Dispose()
        {
            if (m_disposed) { return; }

            m_disposed = true;
            m_disposable?.Dispose();
            foreach (var observer in m_observers) { observer.Key.OnCompleted(); }//if there are any left, send out the complete notification
            GC.SuppressFinalize(this);
        }
    }
}
