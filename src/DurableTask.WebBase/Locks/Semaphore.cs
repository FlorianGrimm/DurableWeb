using System;
using System.Threading;
using System.Threading.Tasks;

namespace DurableTask.Local {
    /// <summary>
    /// A semaphore that limits the number of tasks that can access a resource. During testing,
    /// the semaphore is automatically replaced with a controlled mocked version.
    /// </summary>
    public class Semaphore : IDisposable {
        /// <summary>
        /// Limits the number of tasks that can access a resource.
        /// </summary>
        private readonly SemaphoreSlim Instance;

        /// <summary>
        /// Number of remaining tasks that can enter the semaphore.
        /// </summary>
        public virtual int CurrentCount => this.Instance.CurrentCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="Semaphore"/> class.
        /// </summary>
        protected Semaphore(SemaphoreSlim semaphore) {
            this.Instance = semaphore;
        }

        /// <summary>
        /// Creates a new semaphore.
        /// </summary>
        /// <returns>The semaphore.</returns>
        public static Semaphore Create(int initialCount, int maxCount) =>
            new Semaphore(new SemaphoreSlim(initialCount, maxCount));

        /// <summary>
        /// Blocks the current task until it can enter the semaphore.
        /// </summary>
        public virtual void Wait() => this.Instance.Wait();

        /// <summary>
        /// Blocks the current task until it can enter the semaphore, using a <see cref="TimeSpan"/>
        /// that specifies the timeout.
        /// </summary>
        /// <param name="timeout">
        /// A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/>
        /// that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents
        /// 0 milliseconds to test the wait handle and return immediately.
        /// </param>
        /// <returns>True if the current task successfully entered the semaphore, else false.</returns>
        public virtual bool Wait(TimeSpan timeout) => this.Instance.Wait(timeout);

        /// <summary>
        /// Blocks the current task until it can enter the semaphore, using a 32-bit signed integer
        /// that specifies the timeout.
        /// </summary>
        /// <param name="millisecondsTimeout">
        /// The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely,
        /// or zero to test the state of the wait handle and return immediately.
        /// </param>
        /// <returns>True if the current task successfully entered the semaphore, else false.</returns>
        public virtual bool Wait(int millisecondsTimeout) => this.Instance.Wait(millisecondsTimeout);

        /// <summary>
        /// Blocks the current task until it can enter the semaphore, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        public virtual void Wait(CancellationToken cancellationToken) => this.Instance.Wait(cancellationToken);

        /// <summary>
        /// Blocks the current task until it can enter the semaphore, using a <see cref="TimeSpan"/>
        /// that specifies the timeout, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="timeout">
        /// A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/>
        /// that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents
        /// 0 milliseconds to test the wait handle and return immediately.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>True if the current task successfully entered the semaphore, else false.</returns>
        public virtual bool Wait(TimeSpan timeout, CancellationToken cancellationToken) =>
            this.Instance.Wait(timeout, cancellationToken);

        /// <summary>
        /// Blocks the current task until it can enter the semaphore, using a 32-bit signed integer
        /// that specifies the timeout, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="millisecondsTimeout">
        /// The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely,
        /// or zero to test the state of the wait handle and return immediately.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>True if the current task successfully entered the semaphore, else false.</returns>
        public virtual bool Wait(int millisecondsTimeout, CancellationToken cancellationToken) =>
            this.Instance.Wait(millisecondsTimeout, cancellationToken);

        /// <summary>
        /// Asynchronously waits to enter the semaphore.
        /// </summary>
        /// <returns>A task that will complete when the semaphore has been entered.</returns>
        public virtual Task WaitAsync() => this.Instance.WaitAsync();

        /// <summary>
        /// Asynchronously waits to enter the semaphore, using a <see cref="TimeSpan"/>
        /// that specifies the timeout.
        /// </summary>
        /// <param name="timeout">
        /// A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/>
        /// that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents
        /// 0 milliseconds to test the wait handle and return immediately.
        /// </param>
        /// <returns>
        /// A task that will complete with a result of true if the current thread successfully entered
        /// the semaphore, otherwise with a result of false.
        /// </returns>
        public virtual Task<bool> WaitAsync(TimeSpan timeout) => this.Instance.WaitAsync(timeout);

        /// <summary>
        /// Asynchronously waits to enter the semaphore, using a 32-bit signed integer
        /// that specifies the timeout.
        /// </summary>
        /// <param name="millisecondsTimeout">
        /// The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely,
        /// or zero to test the state of the wait handle and return immediately.
        /// </param>
        /// <returns>
        /// A task that will complete with a result of true if the current thread successfully entered
        /// the semaphore, otherwise with a result of false.
        /// </returns>
        public virtual Task<bool> WaitAsync(int millisecondsTimeout) =>
            this.Instance.WaitAsync(millisecondsTimeout);

        /// <summary>
        /// Asynchronously waits to enter the semaphore, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that will complete when the semaphore has been entered.</returns>
        public virtual Task WaitAsync(CancellationToken cancellationToken) =>
            this.Instance.WaitAsync(cancellationToken);

        /// <summary>
        /// Asynchronously waits to enter the semaphore, using a <see cref="TimeSpan"/>
        /// that specifies the timeout, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="timeout">
        /// A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/>
        /// that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents
        /// 0 milliseconds to test the wait handle and return immediately.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>
        /// A task that will complete with a result of true if the current thread successfully entered
        /// the semaphore, otherwise with a result of false.
        /// </returns>
        public virtual Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken) =>
            this.Instance.WaitAsync(timeout, cancellationToken);

        /// <summary>
        /// Asynchronously waits to enter the semaphore, using a 32-bit signed integer
        /// that specifies the timeout, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="millisecondsTimeout">
        /// The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely,
        /// or zero to test the state of the wait handle and return immediately.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>
        /// A task that will complete with a result of true if the current thread successfully entered
        /// the semaphore, otherwise with a result of false.
        /// </returns>
        public virtual Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken cancellationToken) =>
            this.Instance.WaitAsync(millisecondsTimeout, cancellationToken);

        /// <summary>
        /// Releases the semaphore.
        /// </summary>
        public virtual void Release() => this.Instance.Release();

        /// <summary>
        /// Releases resources used by the semaphore.
        /// </summary>
        private void Dispose(bool disposing) {
            if (!disposing) {
                return;
            }

            this.Instance?.Dispose();
        }

        /// <summary>
        /// Releases resources used by the semaphore.
        /// </summary>
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
