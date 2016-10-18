using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;

namespace Magic.Net.Data
{

    internal abstract class ObjectStream : IEnumerator, IDisposable
    {
        protected object _currentItem;
        protected bool _disposed;

        public ObjectStream(Uri remoteAddress)
        {
            RemoteAddress = remoteAddress;
        }

        #region Implementation of IEnumerable

        /// <inheritdoc />
        public abstract IEnumerator GetEnumerator();

        #endregion
        public Uri RemoteAddress { get; private set; }

        public abstract void Push([NotNull]object item);

        #region Implementation of IEnumerator

        public abstract bool MoveNext();

        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator.Current
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException(RemoteAddress.ToString(), typeof(ObjectStream).FullName + " is allready disposed.");
                return _currentItem;
            }
        }

        #endregion

        #region Implementation of IDisposable

        protected virtual void Dispose(bool diposeManagedResources)
        {
            if (_disposed) return;
            _disposed = true;
            if (diposeManagedResources)
            {
                 
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }

    internal sealed class RemoteObjectStream<T> : ObjectStream, IEnumerator<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();
        private readonly ManualResetEventSlim _waitForNewItemsEvent = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _canReadEvent = new ManualResetEventSlim(true);

        public RemoteObjectStream(Uri remoteAddress)
            :base(remoteAddress)
        {

        }

        public override IEnumerator GetEnumerator()
        {
            return this;
        }

        public override void Push([NotNull]object item)
        {
            if (item == null) throw new ArgumentNullException("item");
            if (!(item is T))
            {
                throw new InvalidCastException(item.GetType().FullName + " can not cast into type " + typeof(T).FullName);
            }
            _canReadEvent.Reset();
            _queue.Enqueue((T)item);
            _canReadEvent.Set();
            _waitForNewItemsEvent.Set();
        }

        #region Implementation of IEnumerator

        public override bool MoveNext()
        {
            _canReadEvent.Wait();
            if (_queue.Count > 0)
            {
                _currentItem = _queue.Dequeue();
                return true;
            }
            _waitForNewItemsEvent.Wait();
            if (_queue.Count > 0)
            {
                _currentItem = _queue.Dequeue();
                return true;
            }
            return false;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public T Current
        {
            get
            {
                return (T)((IEnumerator)this).Current;
            }
        }

        #endregion

        #region Implementation of IDisposable

        #region Overrides of ObjectStream

        protected override void Dispose(bool diposeManagedResources)
        {
            _waitForNewItemsEvent.Dispose();
            _canReadEvent.Dispose();
            base.Dispose(diposeManagedResources);
        }

        #endregion

        #endregion
    }
}