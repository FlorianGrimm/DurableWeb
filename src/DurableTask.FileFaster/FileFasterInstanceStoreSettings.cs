using DurableTask.Core.Serializing;

using System;

namespace DurableTask.FileFaster {
    public class FileFasterInstanceStoreSettings {
        private DataConverter _DataConverter;
        internal const string OrchestrationTable = "_OrchestrationState";
        internal const string WorkitemTable = "_WorkItem";

        private string _OrchestrationStateFileName;
        private string _WorkItemFileName;
        private string _HubName;
        private string _BaseFolder;

        public FileFasterInstanceStoreSettings() {
        }

        public DataConverter DataConverter { 
            get { return this._DataConverter ??= new JsonDataConverter(); }
            set { this._DataConverter = value; } 
        }

        /// <summary>
        /// Gets or sets the hub name for the databse instance store.
        /// </summary>
        public string HubName { get => this._HubName; set { this._HubName = value; this.Initialize(); } }

        /// <summary>
        /// Gets or sets the schema name to which the tables will be added.
        /// </summary>
        public string BaseFolder { get => this._BaseFolder; set { this._BaseFolder = value; this.Initialize();  } }

        /// <summary>
        /// The schema and name of the Orchestration State table.
        /// </summary>
        public string OrchestrationStateFileName => this._OrchestrationStateFileName;
        //=> $"[{BaseFolder}].[{HubName}{OrchestrationTable}]";

        /// <summary>
        /// The schema nad name of the Work Item table.
        /// </summary>
        public string WorkItemFileName => this._WorkItemFileName;
        //    => $"[{BaseFolder}].[{HubName}{WorkitemTable}]";

        
        private void Initialize() {
            this._WorkItemFileName = System.IO.Path.Combine(this._BaseFolder ?? ".", $"{this._HubName}{WorkitemTable}.tab");
            this._OrchestrationStateFileName = System.IO.Path.Combine(this._BaseFolder ?? ".", $"{this._HubName}{OrchestrationTable}.tab");
        }
    }
}
