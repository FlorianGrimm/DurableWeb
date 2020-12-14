using DurableTask.Core;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DurableTask.Local {
    public class LocalOrchestrationService : IOrchestrationService {
        public IOrchestrationService OrchestrationService;
        public LocalOrchestrationService(IOrchestrationService orchestrationService) {
            this.OrchestrationService = orchestrationService;
        }

        public int TaskOrchestrationDispatcherCount => this.OrchestrationService.TaskOrchestrationDispatcherCount;

        public int MaxConcurrentTaskOrchestrationWorkItems => this.OrchestrationService.MaxConcurrentTaskOrchestrationWorkItems;

        public BehaviorOnContinueAsNew EventBehaviourForContinueAsNew => this.OrchestrationService.EventBehaviourForContinueAsNew;

        public int TaskActivityDispatcherCount => this.OrchestrationService.TaskActivityDispatcherCount;

        public int MaxConcurrentTaskActivityWorkItems => this.OrchestrationService.MaxConcurrentTaskActivityWorkItems;

        public Task AbandonTaskActivityWorkItemAsync(TaskActivityWorkItem workItem) {
            return this.OrchestrationService.AbandonTaskActivityWorkItemAsync(workItem);
        }

        public Task AbandonTaskOrchestrationWorkItemAsync(TaskOrchestrationWorkItem workItem) {
            return this.OrchestrationService.AbandonTaskOrchestrationWorkItemAsync(workItem);
        }

        public Task CompleteTaskActivityWorkItemAsync(TaskActivityWorkItem workItem, TaskMessage responseMessage) {
            return this.OrchestrationService.CompleteTaskActivityWorkItemAsync(workItem, responseMessage);
        }

        public Task CompleteTaskOrchestrationWorkItemAsync(TaskOrchestrationWorkItem workItem, OrchestrationRuntimeState newOrchestrationRuntimeState, IList<TaskMessage> outboundMessages, IList<TaskMessage> orchestratorMessages, IList<TaskMessage> timerMessages, TaskMessage continuedAsNewMessage, OrchestrationState orchestrationState) {
            return this.OrchestrationService.CompleteTaskOrchestrationWorkItemAsync(workItem, newOrchestrationRuntimeState, outboundMessages, orchestratorMessages, timerMessages, continuedAsNewMessage, orchestrationState);
        }

        public Task CreateAsync() {
            return this.OrchestrationService.CreateAsync();
        }

        public Task CreateAsync(bool recreateInstanceStore) {
            return this.OrchestrationService.CreateAsync(recreateInstanceStore);
        }

        public Task CreateIfNotExistsAsync() {
            return this.OrchestrationService.CreateIfNotExistsAsync();
        }

        public Task DeleteAsync() {
            return this.OrchestrationService.DeleteAsync();
        }

        public Task DeleteAsync(bool deleteInstanceStore) {
            return this.OrchestrationService.DeleteAsync(deleteInstanceStore);
        }

        public int GetDelayInSecondsAfterOnFetchException(Exception exception) {
            return this.OrchestrationService.GetDelayInSecondsAfterOnFetchException(exception);
        }

        public int GetDelayInSecondsAfterOnProcessException(Exception exception) {
            return this.OrchestrationService.GetDelayInSecondsAfterOnProcessException(exception);
        }

        public bool IsMaxMessageCountExceeded(int currentMessageCount, OrchestrationRuntimeState runtimeState) {
            return this.OrchestrationService.IsMaxMessageCountExceeded(currentMessageCount, runtimeState);
        }

        public Task<TaskActivityWorkItem> LockNextTaskActivityWorkItem(TimeSpan receiveTimeout, CancellationToken cancellationToken) {
            return this.OrchestrationService.LockNextTaskActivityWorkItem(receiveTimeout, cancellationToken);
        }

        public Task<TaskOrchestrationWorkItem> LockNextTaskOrchestrationWorkItemAsync(TimeSpan receiveTimeout, CancellationToken cancellationToken) {
            return this.OrchestrationService.LockNextTaskOrchestrationWorkItemAsync(receiveTimeout, cancellationToken);
        }

        public Task ReleaseTaskOrchestrationWorkItemAsync(TaskOrchestrationWorkItem workItem) {
            return this.OrchestrationService.ReleaseTaskOrchestrationWorkItemAsync(workItem);
        }

        public Task<TaskActivityWorkItem> RenewTaskActivityWorkItemLockAsync(TaskActivityWorkItem workItem) {
            return this.OrchestrationService.RenewTaskActivityWorkItemLockAsync(workItem);
        }

        public Task RenewTaskOrchestrationWorkItemLockAsync(TaskOrchestrationWorkItem workItem) {
            return this.OrchestrationService.RenewTaskOrchestrationWorkItemLockAsync(workItem);
        }

        public Task StartAsync() {
            return this.OrchestrationService.StartAsync();
        }

        public Task StopAsync() {
            return this.OrchestrationService.StopAsync();
        }

        public Task StopAsync(bool isForced) {
            return this.OrchestrationService.StopAsync(isForced);
        }
    }
}
