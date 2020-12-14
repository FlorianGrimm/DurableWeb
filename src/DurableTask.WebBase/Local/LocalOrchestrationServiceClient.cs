using DurableTask.Core;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DurableTask.Local {
    public class LocalOrchestrationServiceClient : IOrchestrationServiceClient {
        public IOrchestrationServiceClient OrchestrationServiceClient;
        public LocalOrchestrationServiceClient(IOrchestrationServiceClient orchestrationServiceClient) {
            this.OrchestrationServiceClient = orchestrationServiceClient;
        }

        public Task CreateTaskOrchestrationAsync(TaskMessage creationMessage) {
            return this.OrchestrationServiceClient.CreateTaskOrchestrationAsync(creationMessage);
        }

        public Task CreateTaskOrchestrationAsync(TaskMessage creationMessage, OrchestrationStatus[] dedupeStatuses) {
            return this.OrchestrationServiceClient.CreateTaskOrchestrationAsync(creationMessage, dedupeStatuses);
        }

        public Task ForceTerminateTaskOrchestrationAsync(string instanceId, string reason) {
            return this.OrchestrationServiceClient.ForceTerminateTaskOrchestrationAsync(instanceId, reason);
        }

        public Task<string> GetOrchestrationHistoryAsync(string instanceId, string executionId) {
            return this.OrchestrationServiceClient.GetOrchestrationHistoryAsync(instanceId, executionId);
        }

        public Task<IList<OrchestrationState>> GetOrchestrationStateAsync(string instanceId, bool allExecutions) {
            return this.OrchestrationServiceClient.GetOrchestrationStateAsync(instanceId, allExecutions);
        }

        public Task<OrchestrationState> GetOrchestrationStateAsync(string instanceId, string executionId) {
            return this.OrchestrationServiceClient.GetOrchestrationStateAsync(instanceId, executionId);
        }

        public Task PurgeOrchestrationHistoryAsync(DateTime thresholdDateTimeUtc, OrchestrationStateTimeRangeFilterType timeRangeFilterType) {
            return this.OrchestrationServiceClient.PurgeOrchestrationHistoryAsync(thresholdDateTimeUtc, timeRangeFilterType);
        }

        public Task SendTaskOrchestrationMessageAsync(TaskMessage message) {
            return this.OrchestrationServiceClient.SendTaskOrchestrationMessageAsync(message);
        }

        public Task SendTaskOrchestrationMessageBatchAsync(params TaskMessage[] messages) {
            return this.OrchestrationServiceClient.SendTaskOrchestrationMessageBatchAsync(messages);
        }

        public Task<OrchestrationState> WaitForOrchestrationAsync(string instanceId, string executionId, TimeSpan timeout, CancellationToken cancellationToken) {
            return this.OrchestrationServiceClient.WaitForOrchestrationAsync(instanceId, executionId, timeout, cancellationToken);
        }
    }
}
