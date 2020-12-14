using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DurableTask.Local {
    /// <summary>
    /// A non-reentrant mutual exclusion lock that can be acquired asynchronously
    /// in a first-in first-out order.
    /// </summary>
    public class AsyncLock : IDisposable {
        /// <summary>
        /// Queue of tasks awaiting to acquire the lock.
        /// </summary>
        protected readonly Queue<TaskCompletionSource<object>> Awaiters;

        /// <summary>
        /// True if the lock has been acquired, else false.
        /// </summary>
        public bool IsAcquired { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLock"/> class.
        /// </summary>
        protected AsyncLock() {
            this.Awaiters = new Queue<TaskCompletionSource<object>>();
            this.IsAcquired = false;
        }

        /// <summary>
        /// Creates a new mutual exclusion lock.
        /// </summary>
        /// <returns>The asynchronous mutual exclusion lock.</returns>
        public static AsyncLock Create() => new AsyncLock();

        /// <summary>
        /// Tries to acquire the lock asynchronously, and returns a task that completes
        /// when the lock has been acquired. The returned task contains a releaser that
        /// releases the lock when disposed. This is not a reentrant operation.
        /// </summary>
        public virtual async Task<Releaser> AcquireAsync() {
            TaskCompletionSource<object> awaiter;
            lock (this.Awaiters) {
                if (this.IsAcquired) {
                    awaiter = new TaskCompletionSource<object>();
                    this.Awaiters.Enqueue(awaiter);
                } else {
                    this.IsAcquired = true;
                    awaiter = null;
                }
            }

            if (awaiter != null) {
                await awaiter.Task;
            }

            return new Releaser(this);
        }

        /// <summary>
        /// Releases the lock.
        /// </summary>
        protected virtual void Release() {
            TaskCompletionSource<object> awaiter = null;
            lock (this.Awaiters) {
                if (this.Awaiters.Count > 0) {
                    awaiter = this.Awaiters.Dequeue();
                } else {
                    this.IsAcquired = false;
                }
            }

            if (awaiter != null) {
                awaiter.SetResult(null);
            }
        }

        protected virtual void Dispose(bool disposing) {
            TaskCompletionSource<object>[] awaiters;
            lock (this.Awaiters) {
                awaiters = this.Awaiters.ToArray();
                this.Awaiters.Clear();
            }
            System.Exception rethrow = null;
            foreach (var awaiter in awaiters) {
                try {
                    if (disposing) {
                        awaiter.SetResult(null);
                    } else {
                        awaiter.TrySetResult(null);
                    }
                } catch (System.Exception error) {
                    if (disposing) {
                        rethrow = error;
                    }
                }
            }
            if ((disposing) && (rethrow is object)) {
                throw rethrow;
            }
        }

        ~AsyncLock() {
            Dispose(disposing: false);
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the acquired <see cref="AsyncLock"/> when disposed.
        /// </summary>
        public struct Releaser : IDisposable {
            /// <summary>
            /// The acquired lock.
            /// </summary>
            private AsyncLock _AsyncLock;

            /// <summary>
            /// Initializes a new instance of the <see cref="Releaser"/> struct.
            /// </summary>
            internal Releaser(AsyncLock asyncLock) {
                this._AsyncLock = asyncLock;
            }

            /// <summary>
            /// Releases the acquired lock.
            /// </summary>
            public void Dispose() {
                var asyncLock = System.Threading.Interlocked.Exchange(ref this._AsyncLock, null);
                asyncLock?.Release();
            }
        }
    }
}
