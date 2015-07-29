using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReactiveServices.MessageBus;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;

namespace ReactiveServices.ComputationalUnit.Work
{
    [DataContract]
    public class Job : Message
    {
        [DataMember]
        public DateTime? RequestStartTime { get; private set; }
        [DataMember]
        public DateTime? RequestCompletionTime { get; private set; }
        [DataMember]
        public TimeSpan RequestTimeout { get; internal set; }
        [DataMember]
        public TimeSpan RequestMaxTime { get; internal set; }
        [DataMember]
        public int RequestMaxAttempts { get; internal set; }
        [DataMember]
        public int ProcessingAttempts { get; internal set; }
        [DataMember]
        public WorkerId WorkerId { get; internal set; }
        [DataMember]
        public Exception ExecutionException { get; internal set; }
        [DataMember]
        public Dictionary<string, string> Parameters { get; set; }
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        public JobFailureAction FailureAction { get; set; }
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        private JobStatus _executionStatus = JobStatus.Pending;
        public JobStatus ExecutionStatus
        {
            get
            {
                return _executionStatus;
            }
            set
            {
                _executionStatus = value;
                switch (_executionStatus)
                {
                    case JobStatus.Pending:
                        RequestStartTime = null;
                        RequestCompletionTime = null;
                        break;
                    case JobStatus.Processing:
                        RequestStartTime = RequestStartTime ?? DateTime.Now;
                        RequestCompletionTime = null;
                        break;
                    case JobStatus.Succeeded:
                    case JobStatus.Failed:
                        RequestCompletionTime = DateTime.Now;
                        break;
                }
            }
        }

        public Type WorkerType { get; internal set; }

        public TimeSpan ProcessingTime
        {
            get
            {
                if (RequestStartTime.HasValue)
                    return DateTime.Now - RequestStartTime.Value;
                return TimeSpan.Zero;
            }
        }

        public bool HasExceededMaxAttempts()
        {
            if (RequestMaxAttempts == Timeout.Infinite)
                return false;
            return ProcessingAttempts >= RequestMaxAttempts;
        }

        public bool HasTimedOut()
        {
            if (RequestTimeout == Timeout.InfiniteTimeSpan)
                return false;

            var now = DateTime.Now;
            if (RequestCompletionTime.HasValue)
            {
                Debug.Assert(HasCompletedWithSuccessOrFailure(), "A work request that contains a RequestCompletionTime should also have a completed execution status!");
                now = RequestCompletionTime.Value;
            }
            var result = now - RequestStartTime > RequestTimeout;
            //Console.WriteLine("Now: {0}, RequestStartTime: {1}, RequestTimeout: {2}, HasTimedOut: {3}", now, RequestStartTime, RequestTimeout, result);
            return result;
        }

        public bool HasCompletedWithSuccessOrFailure()
        {
            return ExecutionStatus == JobStatus.Succeeded || 
                   ExecutionStatus == JobStatus.Failed;
        }

        public override string ToString()
        {
            return String.Format("Job#{0}", MessageId.Value);
        }
    }
}
