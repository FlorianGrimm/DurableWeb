﻿using DurableTask.Core;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DurableTask.Local {
    internal class PeekLockSessionQueue {
        readonly List<TaskSession> sessionQueue;
        readonly List<TaskSession> lockedSessionQueue;

        readonly object thisLock = new object();

        public PeekLockSessionQueue() {
            this.sessionQueue = new List<TaskSession>();
            this.lockedSessionQueue = new List<TaskSession>();
        }

        public void DropSession(string id) {
            lock (this.thisLock) {
                TaskSession taskSession = this.lockedSessionQueue.Find((ts) => string.Equals(ts.Id, id, StringComparison.InvariantCultureIgnoreCase));

                if (taskSession == null) {
                    return;
                }

                if (this.sessionQueue.Contains(taskSession)) {
                    this.sessionQueue.Remove(taskSession);
                } else if (this.lockedSessionQueue.Contains(taskSession)) {
                    this.lockedSessionQueue.Remove(taskSession);
                }
            }
        }

        public void SendMessage(TaskMessage message) {
            lock (this.thisLock) {
                foreach (TaskSession ts in this.sessionQueue) {
                    if (ts.Id == message.OrchestrationInstance.InstanceId) {
                        ts.Messages.Add(message);
                        return;
                    }
                }

                foreach (TaskSession ts in this.lockedSessionQueue) {
                    if (ts.Id == message.OrchestrationInstance.InstanceId) {
                        ts.Messages.Add(message);
                        return;
                    }
                }

                // create a new session
                this.sessionQueue.Add(new TaskSession {
                    Id = message.OrchestrationInstance.InstanceId,
                    SessionState = null,
                    Messages = new List<TaskMessage> { message }
                });
            }
        }

        public void CompleteSession(
            string id,
            byte[] newState,
            IList<TaskMessage> newMessages,
            TaskMessage continuedAsNewMessage) {
            lock (this.thisLock) {
                TaskSession taskSession = this.lockedSessionQueue.Find((ts) => string.Equals(ts.Id, id, StringComparison.InvariantCultureIgnoreCase));

                if (taskSession == null) {
                    // TODO : throw proper lock lost exception (AFFANDAR)
                    throw new InvalidOperationException("Lock lost");
                }

                this.lockedSessionQueue.Remove(taskSession);

                // make the required updates to the session
                foreach (TaskMessage tm in taskSession.LockTable) {
                    taskSession.Messages.Remove(tm);
                }

                taskSession.LockTable.Clear();

                taskSession.SessionState = newState;

                if (newState != null) {
                    this.sessionQueue.Add(taskSession);
                }

                foreach (TaskMessage m in newMessages) {
                    SendMessage(m);
                }

                if (continuedAsNewMessage != null) {
                    SendMessage(continuedAsNewMessage);
                }
            }
        }

        public void AbandonSession(string id) {
            lock (this.thisLock) {
                TaskSession taskSession = this.lockedSessionQueue.Find((ts) => string.Equals(ts.Id, id, StringComparison.InvariantCultureIgnoreCase));

                if (taskSession == null) {
                    // TODO : throw proper lock lost exception (AFFANDAR)
                    throw new InvalidOperationException("Lock lost");
                }

                this.lockedSessionQueue.Remove(taskSession);

                // TODO : note that this we are adding to the tail of the queue rather than head, which is what sbus would actually do (AFFANDAR)
                //      doesn't really matter though in terms of semantics
                this.sessionQueue.Add(taskSession);

                // unlock all messages
                taskSession.LockTable.Clear();
            }
        }

        public async Task<TaskSession> AcceptSessionAsync(TimeSpan receiveTimeout, CancellationToken cancellationToken) {
            Stopwatch timer = Stopwatch.StartNew();
            while (timer.Elapsed < receiveTimeout && !cancellationToken.IsCancellationRequested) {
                lock (this.thisLock) {
                    foreach (TaskSession ts in this.sessionQueue) {
                        if (ts.Messages.Count > 0) {
                            this.lockedSessionQueue.Add(ts);
                            this.sessionQueue.Remove(ts);

                            // all messages are now locked
                            foreach (TaskMessage tm in ts.Messages) {
                                ts.LockTable.Add(tm);
                            }

                            return ts;
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
    }
}