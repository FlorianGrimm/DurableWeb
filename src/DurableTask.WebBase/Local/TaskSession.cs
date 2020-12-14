using DurableTask.Core;

using System.Collections.Generic;

namespace DurableTask.Local {
    internal class TaskSession {
        public string Id;
        public byte[] SessionState;
        public List<TaskMessage> Messages;
        public HashSet<TaskMessage> LockTable;

        public TaskSession() {
            this.SessionState = null;
            this.Messages = new List<TaskMessage>();
            this.LockTable = new HashSet<TaskMessage>();
        }
    }
}
