
using DurableTask.Core;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
namespace DurableTask.Local {
    internal class PeekLockQueue {
        readonly List<TaskMessage> messages;
        readonly HashSet<TaskMessage> lockTable;

        readonly object thisLock = new object();

        public PeekLockQueue() {
            this.messages = new List<TaskMessage>();
            this.lockTable = new HashSet<TaskMessage>();
        }

        public async Task<TaskMessage> ReceiveMessageAsync(TimeSpan receiveTimeout, CancellationToken cancellationToken) {
            Stopwatch timer = Stopwatch.StartNew();
            while (timer.Elapsed < receiveTimeout && !cancellationToken.IsCancellationRequested) {
                lock (this.thisLock) {
                    foreach (TaskMessage tm in this.messages) {
                        if (!this.lockTable.Contains(tm)) {
                            this.lockTable.Add(tm);
                            return tm;
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested) {
                throw new TaskCanceledException();
            }

            return null;
        }

        public void SendMessageAsync(TaskMessage message) {
            lock (this.thisLock) {
                this.messages.Add(message);
            }
        }

        public void CompleteMessageAsync(TaskMessage message) {
            lock (this.thisLock) {
                if (!this.lockTable.Contains(message)) {
                    throw new InvalidOperationException("Message Lock Lost");
                }

                this.lockTable.Remove(message);
                this.messages.Remove(message);
            }
        }

        public void AbandonMessageAsync(TaskMessage message) {
            lock (this.thisLock) {
                if (!this.lockTable.Contains(message)) {
                    return;
                }

                this.lockTable.Remove(message);
            }
        }
    }
}
