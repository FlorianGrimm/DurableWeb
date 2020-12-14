using DurableTask.Core;
using DurableTask.Core.Serializing;
using DurableTask.Core.Tracking;

using FASTER.core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DurableTask.FileFaster {
    public class FileFasterInstanceStore : IOrchestrationServiceInstanceStore {
        private readonly DataConverter _DataConverter;
        private readonly string _OrchestrationStateFileName;
        private readonly string _WorkItemFileName;
        private readonly IDevice _OrchestrationStateDevice;
        private readonly IDevice _WorkItemDevice;

        public int MaxHistoryEntryLength => throw new NotImplementedException();

        public FileFasterInstanceStore(FileFasterInstanceStoreSettings settings) {
            this._DataConverter = settings.DataConverter;
            this._OrchestrationStateFileName = settings.OrchestrationStateFileName;
            this._WorkItemFileName = settings.WorkItemFileName;
            this._OrchestrationStateDevice = FASTER.core.Devices.CreateLogDevice(this._OrchestrationStateFileName, preallocateFile: false, deleteOnClose: false, capacity: FASTER.core.Devices.CAPACITY_UNSPECIFIED, recoverDevice: false);
            this._WorkItemDevice = FASTER.core.Devices.CreateLogDevice(this._WorkItemFileName, preallocateFile: false, deleteOnClose: false, capacity: FASTER.core.Devices.CAPACITY_UNSPECIFIED, recoverDevice: false);
            new FasterKV<string, SpanByte>(
                size:1L<<20,
                  logSettings: new LogSettings { LogDevice = log, MemorySizeBits = 15, PageSizeBits = 12 },
                )
        }

        public Task InitializeStoreAsync(bool recreate) {
            throw new NotImplementedException();
        }
        public async Task DeleteStoreAsync() {
            if (System.IO.File.Exists(this._OrchestrationStateFileName)) { 
                System.IO.File.Delete(this._OrchestrationStateFileName);
            }
            if (System.IO.File.Exists(this._WorkItemFileName)) {
                System.IO.File.Delete(this._WorkItemFileName);
            }
            await Task.CompletedTask;
        }

        public Task<IEnumerable<OrchestrationStateInstanceEntity>> GetEntitiesAsync(string instanceId, string executionId) {
            throw new NotImplementedException();
        }
        public Task<object> WriteEntitiesAsync(IEnumerable<InstanceEntityBase> entities) {
            throw new NotImplementedException();
        }
        public Task<object> DeleteEntitiesAsync(IEnumerable<InstanceEntityBase> entities) {
            throw new NotImplementedException();
        }
        public Task<IEnumerable<OrchestrationJumpStartInstanceEntity>> GetJumpStartEntitiesAsync(int top) {
            throw new NotImplementedException();
        }
        public Task<object> WriteJumpStartEntitiesAsync(IEnumerable<OrchestrationJumpStartInstanceEntity> entities) {
            throw new NotImplementedException();
        }
        public Task<object> DeleteJumpStartEntitiesAsync(IEnumerable<OrchestrationJumpStartInstanceEntity> entities) {
            throw new NotImplementedException();
        }



        public Task<IEnumerable<OrchestrationWorkItemInstanceEntity>> GetOrchestrationHistoryEventsAsync(string instanceId, string executionId) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OrchestrationStateInstanceEntity>> GetOrchestrationStateAsync(string instanceId, bool allInstances) {
            throw new NotImplementedException();
        }

        public Task<OrchestrationStateInstanceEntity> GetOrchestrationStateAsync(string instanceId, string executionId) {
            throw new NotImplementedException();
        }

        public Task<int> PurgeOrchestrationHistoryEventsAsync(DateTime thresholdDateTimeUtc, OrchestrationStateTimeRangeFilterType timeRangeFilterType) {
            throw new NotImplementedException();
        }



    }
    class X : FASTER.core.IFasterEqualityComparer<X>, FASTER.core.IV { 
    }
}
