using DurableTask.Core;
using DurableTask.Core.Common;
using DurableTask.Core.Exceptions;
using DurableTask.Core.History;

using Newtonsoft.Json;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DurableTask.Local {
    internal class LocalOrchestrationSvc : IDisposable, IOrchestrationService, IOrchestrationServiceClient {
        Dictionary<string, byte[]> _SessionState;
        readonly List<TaskMessage> _TimerMessages;

        readonly int MaxConcurrentWorkItems = 20;

        // dictionary<instanceId, dictionary<executionId, orchestrationState>>
        ////Dictionary<string, Dictionary<string, OrchestrationState>> instanceStore;

        readonly PeekLockSessionQueue _OrchestratorQueue;
        readonly PeekLockQueue _WorkerQueue;

        readonly CancellationTokenSource _CancellationTokenSource;

        readonly Dictionary<string, Dictionary<string, OrchestrationState>> _InstanceStore;

        //Dictionary<string, Tuple<List<TaskMessage>, byte[]>> sessionLock;

        readonly object _ThisLock;
        readonly object _TimerLock;

        readonly ConcurrentDictionary<string, TaskCompletionSource<OrchestrationState>> _OrchestrationWaiters;

        /// <summary>
        ///     Creates a new instance of the LocalOrchestrationService with default settings
        /// </summary>
        public LocalOrchestrationSvc() {
            this._ThisLock = new object();
            this._TimerLock = new object();
            this._OrchestratorQueue = new PeekLockSessionQueue();
            this._WorkerQueue = new PeekLockQueue();

            this._SessionState = new Dictionary<string, byte[]>();

            this._TimerMessages = new List<TaskMessage>();
            this._InstanceStore = new Dictionary<string, Dictionary<string, OrchestrationState>>();
            this._OrchestrationWaiters = new ConcurrentDictionary<string, TaskCompletionSource<OrchestrationState>>();
            this._CancellationTokenSource = new CancellationTokenSource();
        }

        async Task TimerMessageSchedulerAsync() {
            while (!this._CancellationTokenSource.Token.IsCancellationRequested) {
                var utcNow = DateTime.UtcNow;
                var utcNext = utcNow.AddHours(1);
                lock (this._TimerLock) {
                    foreach (TaskMessage tm in this._TimerMessages.ToList()) {
                        var te = tm.Event as TimerFiredEvent;

                        if (te == null) {
                            // TODO : unobserved task exception (AFFANDAR)
                            throw new InvalidOperationException("Invalid timer message");
                        }
                        if (te.FireAt < utcNext) {
                            utcNext = te.FireAt;
                        }
                        if (te.FireAt <= utcNow) {
                            this._OrchestratorQueue.SendMessage(tm);
                            this._TimerMessages.Remove(tm);
                        }
                    }
                }
                {
                    var tsNext = utcNext.Subtract(utcNow);
                    if (tsNext.TotalSeconds < 0) {
                        tsNext = TimeSpan.FromMinutes(1);
                    }
                    await Task.Delay(tsNext);
                }
            }
        }

        /******************************/
        // management methods
        /******************************/
        /// <inheritdoc />
        public Task CreateAsync() {
            return CreateAsync(true);
        }

        /// <inheritdoc />
        public Task CreateAsync(bool recreateInstanceStore) {
            return Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public Task CreateIfNotExistsAsync() {
            return Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public Task DeleteAsync() {
            return DeleteAsync(true);
        }

        /// <inheritdoc />
        public Task DeleteAsync(bool deleteInstanceStore) {
            return Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public Task StartAsync() {
            Task.Run(() => TimerMessageSchedulerAsync());
            return Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public Task StopAsync(bool isForced) {
            this._CancellationTokenSource.Cancel();
            return Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public Task StopAsync() {
            return StopAsync(false);
        }

        /// <summary>
        /// Determines whether is a transient or not.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        ///   <c>true</c> if is transient exception; otherwise, <c>false</c>.
        /// </returns>
        public bool IsTransientException(Exception exception) {
            return false;
        }

        /******************************/
        // client methods
        /******************************/
        /// <inheritdoc />
        public Task CreateTaskOrchestrationAsync(TaskMessage creationMessage) {
            return CreateTaskOrchestrationAsync(creationMessage, null);
        }

        /// <inheritdoc />
        public Task CreateTaskOrchestrationAsync(TaskMessage creationMessage, OrchestrationStatus[] dedupeStatuses) {
            var ee = creationMessage.Event as ExecutionStartedEvent;

            if (ee == null) {
                throw new InvalidOperationException("Invalid creation task message");
            }

            lock (this._ThisLock) {
                if (!this._InstanceStore.TryGetValue(creationMessage.OrchestrationInstance.InstanceId, out Dictionary<string, OrchestrationState> ed)) {
                    ed = new Dictionary<string, OrchestrationState>();
                    this._InstanceStore[creationMessage.OrchestrationInstance.InstanceId] = ed;
                }

                OrchestrationState latestState = ed.Values.OrderBy(state => state.CreatedTime).FirstOrDefault(state => state.OrchestrationStatus != OrchestrationStatus.ContinuedAsNew);

                if ((latestState != null) 
                    && (dedupeStatuses == null || dedupeStatuses.Contains(latestState.OrchestrationStatus))) {
                    // An orchestration with same instance id is already running
                    throw new OrchestrationAlreadyExistsException($"An orchestration with id '{creationMessage.OrchestrationInstance.InstanceId}' already exists. It is in state {latestState.OrchestrationStatus}");
                }

                var newState = new OrchestrationState {
                    OrchestrationInstance = new OrchestrationInstance {
                        InstanceId = creationMessage.OrchestrationInstance.InstanceId,
                        ExecutionId = creationMessage.OrchestrationInstance.ExecutionId,
                    },
                    CreatedTime = DateTime.UtcNow,
                    LastUpdatedTime = DateTime.UtcNow,
                    OrchestrationStatus = OrchestrationStatus.Pending,
                    Version = ee.Version,
                    Name = ee.Name,
                    Input = ee.Input,
                };

                ed.Add(creationMessage.OrchestrationInstance.ExecutionId, newState);

                this._OrchestratorQueue.SendMessage(creationMessage);
            }

            return Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public Task SendTaskOrchestrationMessageAsync(TaskMessage message) {
            return SendTaskOrchestrationMessageBatchAsync(message);
        }

        /// <inheritdoc />
        public Task SendTaskOrchestrationMessageBatchAsync(params TaskMessage[] messages) {
            foreach (TaskMessage message in messages) {
                this._OrchestratorQueue.SendMessage(message);
            }

            return Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public async Task<OrchestrationState> WaitForOrchestrationAsync(
            string instanceId,
            string executionId,
            TimeSpan timeout,
            CancellationToken cancellationToken) {
            if (string.IsNullOrWhiteSpace(executionId)) {
                executionId = string.Empty;
            }

            string key = instanceId + "_" + executionId;

            if (!this._OrchestrationWaiters.TryGetValue(key, out TaskCompletionSource<OrchestrationState> tcs)) {
                tcs = new TaskCompletionSource<OrchestrationState>();

                if (!this._OrchestrationWaiters.TryAdd(key, tcs)) {
                    this._OrchestrationWaiters.TryGetValue(key, out tcs);
                }

                if (tcs == null) {
                    throw new InvalidOperationException("Unable to get tcs from orchestrationWaiters");
                }
            }

            // might have finished already
            lock (this._ThisLock) {
                if (this._InstanceStore.ContainsKey(instanceId)) {
                    Dictionary<string, OrchestrationState> stateMap = this._InstanceStore[instanceId];

                    if (stateMap != null && stateMap.Count > 0) {
                        OrchestrationState state = null;
                        if (string.IsNullOrWhiteSpace(executionId)) {
                            IOrderedEnumerable<OrchestrationState> sortedStateMap = stateMap.Values.OrderByDescending(os => os.CreatedTime);
                            state = sortedStateMap.First();
                        } else {
                            if (stateMap.ContainsKey(executionId)) {
                                state = this._InstanceStore[instanceId][executionId];
                            }
                        }

                        if (state != null
                            && state.OrchestrationStatus != OrchestrationStatus.Running
                            && state.OrchestrationStatus != OrchestrationStatus.Pending) {
                            // if only master id was specified then continueAsNew is a not a terminal state
                            if (!(string.IsNullOrWhiteSpace(executionId) && state.OrchestrationStatus == OrchestrationStatus.ContinuedAsNew)) {
                                tcs.TrySetResult(state);
                            }
                        }
                    }
                }
            }

            CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                this._CancellationTokenSource.Token);
            Task timeOutTask = Task.Delay(timeout, cts.Token);
            Task ret = await Task.WhenAny(tcs.Task, timeOutTask);

            if (ret == timeOutTask) {
                throw new TimeoutException("timed out or canceled while waiting for orchestration to complete");
            }

            cts.Cancel();

            return await tcs.Task;
        }

        /// <inheritdoc />
        public async Task<OrchestrationState> GetOrchestrationStateAsync(string instanceId, string executionId) {
            OrchestrationState response;

            lock (this._ThisLock) {
                if (!(this._InstanceStore.TryGetValue(instanceId, out Dictionary<string, OrchestrationState> state) &&
                    state.TryGetValue(executionId, out response))) {
                    response = null;
                }
            }

            return await Task.FromResult(response);
        }

        /// <inheritdoc />
        public async Task<IList<OrchestrationState>> GetOrchestrationStateAsync(string instanceId, bool allExecutions) {
            IList<OrchestrationState> response;

            lock (this._ThisLock) {
                if (this._InstanceStore.TryGetValue(instanceId, out Dictionary<string, OrchestrationState> state)) {
                    response = state.Values.ToList();
                } else {
                    response = new List<OrchestrationState>();
                }
            }

            return await Task.FromResult(response);
        }

        /// <inheritdoc />
        public Task<string> GetOrchestrationHistoryAsync(string instanceId, string executionId) {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task PurgeOrchestrationHistoryAsync(DateTime thresholdDateTimeUtc, OrchestrationStateTimeRangeFilterType timeRangeFilterType) {
            throw new NotSupportedException();
        }

        /******************************/
        // Task orchestration methods
        /******************************/
        /// <inheritdoc />
        public int MaxConcurrentTaskOrchestrationWorkItems => this.MaxConcurrentWorkItems;

        /// <inheritdoc />
        public async Task<TaskOrchestrationWorkItem> LockNextTaskOrchestrationWorkItemAsync(
            TimeSpan receiveTimeout,
            CancellationToken cancellationToken) {
            TaskSession taskSession = await this._OrchestratorQueue.AcceptSessionAsync(receiveTimeout,
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this._CancellationTokenSource.Token).Token);

            if (taskSession == null) {
                return null;
            }

            var wi = new TaskOrchestrationWorkItem {
                NewMessages = taskSession.Messages.ToList(),
                InstanceId = taskSession.Id,
                LockedUntilUtc = DateTime.UtcNow.AddMinutes(5),
                OrchestrationRuntimeState =
                    DeserializeOrchestrationRuntimeState(taskSession.SessionState) ??
                    new OrchestrationRuntimeState(),
            };

            return wi;
        }

        /// <inheritdoc />
        public Task CompleteTaskOrchestrationWorkItemAsync(
            TaskOrchestrationWorkItem workItem,
            OrchestrationRuntimeState newOrchestrationRuntimeState,
            IList<TaskMessage> outboundMessages,
            IList<TaskMessage> orchestratorMessages,
            IList<TaskMessage> workItemTimerMessages,
            TaskMessage continuedAsNewMessage,
            OrchestrationState state) {
            lock (this._ThisLock) {
                byte[] newSessionState;

                if (newOrchestrationRuntimeState == null ||
                newOrchestrationRuntimeState.ExecutionStartedEvent == null ||
                newOrchestrationRuntimeState.OrchestrationStatus != OrchestrationStatus.Running) {
                    newSessionState = null;
                } else {
                    newSessionState = SerializeOrchestrationRuntimeState(newOrchestrationRuntimeState);
                }

                this._OrchestratorQueue.CompleteSession(
                    workItem.InstanceId,
                    newSessionState,
                    orchestratorMessages,
                    continuedAsNewMessage
                    );

                if (outboundMessages != null) {
                    foreach (TaskMessage tm in outboundMessages) {
                        // TODO : make async (AFFANDAR)
                        this._WorkerQueue.SendMessageAsync(tm);
                    }
                }

                if (workItemTimerMessages != null) {
                    lock (this._TimerLock) {
                        foreach (TaskMessage tm in workItemTimerMessages) {
                            var te = tm.Event as TimerFiredEvent;

                            if (te == null) {
                                // TODO : unobserved task exception (AFFANDAR)
                                throw new InvalidOperationException("Invalid timer message");
                            } else { 
                                this._TimerMessages.Add(tm);
                            }
                        }
                    }
                }

                if (workItem.OrchestrationRuntimeState != newOrchestrationRuntimeState) {
                    var oldState = Utils.BuildOrchestrationState(workItem.OrchestrationRuntimeState);
                    CommitState(workItem.OrchestrationRuntimeState, oldState).GetAwaiter().GetResult();
                }

                if (state != null) {
                    CommitState(newOrchestrationRuntimeState, state).GetAwaiter().GetResult();
                }
            }

            return Task.FromResult(0);
        }

        Task CommitState(OrchestrationRuntimeState runtimeState, OrchestrationState state) {
            if (!this._InstanceStore.TryGetValue(runtimeState.OrchestrationInstance.InstanceId, out Dictionary<string, OrchestrationState> mapState)) {
                mapState = new Dictionary<string, OrchestrationState>();
                this._InstanceStore[runtimeState.OrchestrationInstance.InstanceId] = mapState;
            }

            mapState[runtimeState.OrchestrationInstance.ExecutionId] = state;

            // signal any waiters waiting on instanceid_executionid or just the latest instanceid_

            if (state.OrchestrationStatus == OrchestrationStatus.Running
                || state.OrchestrationStatus == OrchestrationStatus.Pending) {
                return Task.FromResult(0);
            }

            string key = runtimeState.OrchestrationInstance.InstanceId + "_" +
                runtimeState.OrchestrationInstance.ExecutionId;

            string key1 = runtimeState.OrchestrationInstance.InstanceId + "_";

            var tasks = new List<Task>();

            if (this._OrchestrationWaiters.TryGetValue(key, out TaskCompletionSource<OrchestrationState> tcs)) {
                tasks.Add(Task.Run(() => tcs.TrySetResult(state)));
            }

            // for instance id level waiters, we will not consider ContinueAsNew as a terminal state because
            // the high level orchestration is still ongoing
            if (state.OrchestrationStatus != OrchestrationStatus.ContinuedAsNew
                && this._OrchestrationWaiters.TryGetValue(key1, out TaskCompletionSource<OrchestrationState> tcs1)) {
                tasks.Add(Task.Run(() => tcs1.TrySetResult(state)));
            }

            if (tasks.Count > 0) {
                Task.WaitAll(tasks.ToArray());
            }

            return Task.FromResult(0);
        }

        /// <inheritdoc />
        public Task AbandonTaskOrchestrationWorkItemAsync(TaskOrchestrationWorkItem workItem) {
            this._OrchestratorQueue.AbandonSession(workItem.InstanceId);
            return Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public Task ReleaseTaskOrchestrationWorkItemAsync(TaskOrchestrationWorkItem workItem) {
            return Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public int TaskActivityDispatcherCount => 1;

        /// <summary>
        ///  Should we carry over unexecuted raised events to the next iteration of an orchestration on ContinueAsNew
        /// </summary>
        public BehaviorOnContinueAsNew EventBehaviourForContinueAsNew => BehaviorOnContinueAsNew.Carryover;

        /// <inheritdoc />
        public int MaxConcurrentTaskActivityWorkItems => this.MaxConcurrentWorkItems;

        /// <inheritdoc />
        public async Task ForceTerminateTaskOrchestrationAsync(string instanceId, string message) {
            var taskMessage = new TaskMessage {
                OrchestrationInstance = new OrchestrationInstance { InstanceId = instanceId },
                Event = new ExecutionTerminatedEvent(-1, message)
            };

            await SendTaskOrchestrationMessageAsync(taskMessage);
        }

        /// <inheritdoc />
        public Task RenewTaskOrchestrationWorkItemLockAsync(TaskOrchestrationWorkItem workItem) {
            workItem.LockedUntilUtc = workItem.LockedUntilUtc.AddMinutes(5);
            return Task.FromResult(0);
        }

        /// <inheritdoc />
        public bool IsMaxMessageCountExceeded(int currentMessageCount, OrchestrationRuntimeState runtimeState) {
            return false;
        }

        /// <inheritdoc />
        public int GetDelayInSecondsAfterOnProcessException(Exception exception) {
            return 0;
        }

        /// <inheritdoc />
        public int GetDelayInSecondsAfterOnFetchException(Exception exception) {
            return 0;
        }

        /// <inheritdoc />
        public int TaskOrchestrationDispatcherCount => 1;

        /******************************/
        // Task activity methods
        /******************************/
        /// <inheritdoc />
        public async Task<TaskActivityWorkItem> LockNextTaskActivityWorkItem(TimeSpan receiveTimeout, CancellationToken cancellationToken) {
            TaskMessage taskMessage = await this._WorkerQueue.ReceiveMessageAsync(receiveTimeout,
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this._CancellationTokenSource.Token).Token);

            if (taskMessage == null) {
                return null;
            }

            return new TaskActivityWorkItem {
                // for the in memory provider we will just use the TaskMessage object ref itself as the id
                Id = "N/A",
                LockedUntilUtc = DateTime.UtcNow.AddMinutes(5),
                TaskMessage = taskMessage,
            };
        }

        /// <inheritdoc />
        public Task AbandonTaskActivityWorkItemAsync(TaskActivityWorkItem workItem) {
            this._WorkerQueue.AbandonMessageAsync(workItem.TaskMessage);
            return Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public Task CompleteTaskActivityWorkItemAsync(TaskActivityWorkItem workItem, TaskMessage responseMessage) {
            lock (this._ThisLock) {
                this._WorkerQueue.CompleteMessageAsync(workItem.TaskMessage);
                this._OrchestratorQueue.SendMessage(responseMessage);
            }

            return Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public Task<TaskActivityWorkItem> RenewTaskActivityWorkItemLockAsync(TaskActivityWorkItem workItem) {
            // TODO : add expiration if we want to unit test it (AFFANDAR)
            workItem.LockedUntilUtc = workItem.LockedUntilUtc.AddMinutes(5);
            return Task.FromResult(workItem);
        }

        byte[] SerializeOrchestrationRuntimeState(OrchestrationRuntimeState runtimeState) {
            if (runtimeState == null) {
                return null;
            }

            string serializeState = JsonConvert.SerializeObject(runtimeState.Events,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            return Encoding.UTF8.GetBytes(serializeState);
        }

        OrchestrationRuntimeState DeserializeOrchestrationRuntimeState(byte[] stateBytes) {
            if (stateBytes == null || stateBytes.Length == 0) {
                return null;
            }

            string serializedState = Encoding.UTF8.GetString(stateBytes);
            var events = JsonConvert.DeserializeObject<IList<HistoryEvent>>(serializedState, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            return new OrchestrationRuntimeState(events);
        }

        /// <inheritdoc />
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (disposing) {
                this._CancellationTokenSource.Cancel();
                this._CancellationTokenSource.Dispose();
            }
        }
    }
}