using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReactiveServices.ComputationalUnit.Work;
using System;
using System.Runtime.Serialization;

namespace ReactiveServices.ComputationalUnit.Dispatching.Tests
{
    public class DummyJob : Job
    {
        public DummyJob()
        {
            WorkDurationInMilliseconds = 2000;
            WaitingTimeForWorkExecutionInMilliseconds = WorkDurationInMilliseconds + 1000;
            ExpectedExecutionStatus = JobStatus.Succeeded;
        }

        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        public JobStatus ExpectedExecutionStatus { get; set; }
        [DataMember]
        public int WorkDurationInMilliseconds { get; set; }
        [DataMember]
        public int WaitingTimeForWorkExecutionInMilliseconds { get; set; }

        [DataMember]
        public int? ValueChangedDuringRequestExecution { get; set; }

        public override string ToString()
        {
            return String.Format("[Id: {0}, ExpectedExecutionStatus: {1}, RequestTimeout: {2}, WorkDurationInMilliseconds: {3}, WaitingTimeForWorkExecutionInMilliseconds: {4}]",
                                 MessageId, ExpectedExecutionStatus, RequestTimeout, WorkDurationInMilliseconds, WaitingTimeForWorkExecutionInMilliseconds);
        }
    }
}
