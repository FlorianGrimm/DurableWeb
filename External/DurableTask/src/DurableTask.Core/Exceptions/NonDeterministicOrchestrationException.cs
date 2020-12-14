﻿//  ----------------------------------------------------------------------------------
//  Copyright Microsoft Corporation
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  http://www.apache.org/licenses/LICENSE-2.0
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  ----------------------------------------------------------------------------------

namespace DurableTask.Core.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents non deterministic created during orchestration execution
    /// </summary>
    [Serializable]
    public class NonDeterministicOrchestrationException : OrchestrationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NonDeterministicOrchestrationException"/> class.
        /// </summary>
        public NonDeterministicOrchestrationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NonDeterministicOrchestrationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NonDeterministicOrchestrationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes an new instance of the OrchestrationException class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public NonDeterministicOrchestrationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes an new instance of the NonDeterministicOrchestrationException class with a specified event id and error message
        /// </summary>
        /// <param name="eventId">EventId of the error.</param>
        /// <param name="eventDetails">The message that describes the error.</param>
        public NonDeterministicOrchestrationException(int eventId, string eventDetails)
            : base("Non-Deterministic workflow detected: " + eventDetails)
        {
            this.EventId = eventId;
        }

        /// <summary>
        /// Initializes a new instance of the NonDeterministicOrchestrationException class with serialized data.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
        protected NonDeterministicOrchestrationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}