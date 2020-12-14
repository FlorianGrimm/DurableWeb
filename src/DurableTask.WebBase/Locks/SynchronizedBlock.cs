using System;

namespace DurableTask.Local {
    /// <summary>
    /// Provides a mechanism that synchronizes access to objects. It is implemented as a thin wrapper
    /// on <see cref="System.Threading.Monitor"/>. During testing, the implementation is automatically replaced
    /// with a controlled mocked version. It can be used as a replacement of the lock keyword to allow
    /// systematic testing.
    /// </summary>
    public class SynchronizedBlock : IDisposable
    {
        /// <summary>
        /// The object used for synchronization.
        /// </summary>
        protected readonly object _SyncObject;

        /// <summary>
        /// True if the lock was taken, else false.
        /// </summary>
        internal bool _IsLockTaken;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizedBlock"/> class.
        /// </summary>
        /// <param name="syncObject">The sync object to serialize access to.</param>
        protected SynchronizedBlock(object syncObject)
        {
            this._SyncObject = syncObject;
        }

        /// <summary>
        /// Creates a new <see cref="SynchronizedBlock"/> for synchronizing access
        /// to the specified object and enters the lock.
        /// </summary>
        /// <returns>The synchronized block.</returns>
        public static SynchronizedBlock Lock(object syncObject) 
            => new SynchronizedBlock(syncObject).EnterLock();

        /// <summary>
        /// Enters the lock.
        /// </summary>
        /// <returns>The synchronized block.</returns>
        protected virtual SynchronizedBlock EnterLock()
        {
            System.Threading.Monitor.Enter(this._SyncObject, ref this._IsLockTaken);
            return this;
        }

        /// <summary>
        /// Notifies a thread in the waiting queue of a change in the locked object's state.
        /// </summary>
        public virtual void Pulse() => System.Threading.Monitor.Pulse(this._SyncObject);

        /// <summary>
        /// Notifies all waiting threads of a change in the object's state.
        /// </summary>
        public virtual void PulseAll() => System.Threading.Monitor.PulseAll(this._SyncObject);

        /// <summary>
        /// Releases the lock on an object and blocks the current thread until it reacquires
        /// the lock.
        /// </summary>
        /// <returns>True if the call returned because the caller reacquired the lock for the specified
        /// object. This method does not return if the lock is not reacquired.</returns>
        public virtual bool Wait() => System.Threading.Monitor.Wait(this._SyncObject);

        /// <summary>
        /// Releases the lock on an object and blocks the current thread until it reacquires
        /// the lock. If the specified time-out interval elapses, the thread enters the ready
        /// queue.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait before the thread enters the ready queue.</param>
        /// <returns>True if the lock was reacquired before the specified time elapsed; false if the
        /// lock was reacquired after the specified time elapsed. The method does not return
        /// until the lock is reacquired.</returns>
        public virtual bool Wait(int millisecondsTimeout) => System.Threading.Monitor.Wait(this._SyncObject, millisecondsTimeout);

        /// <summary>
        /// Releases the lock on an object and blocks the current thread until it reacquires
        /// the lock. If the specified time-out interval elapses, the thread enters the ready
        /// queue.
        /// </summary>
        /// <param name="timeout">A System.TimeSpan representing the amount of time to wait before the thread enters
        /// the ready queue.</param>
        /// <returns>True if the lock was reacquired before the specified time elapsed; false if the
        /// lock was reacquired after the specified time elapsed. The method does not return
        /// until the lock is reacquired.</returns>
        public virtual bool Wait(TimeSpan timeout) => System.Threading.Monitor.Wait(this._SyncObject, timeout);

        /// <summary>
        /// Releases resources used by the synchronized block.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && this._IsLockTaken)
            {
                System.Threading.Monitor.Exit(this._SyncObject);
            }
        }

        /// <summary>
        /// Releases resources used by the synchronized block.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
