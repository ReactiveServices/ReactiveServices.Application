using System.Diagnostics;
using System.Xml;
using PostSharp.Patterns.Diagnostics;

namespace ReactiveServices.ComputationalUnit.Settings
{
    [Log(AttributeExclude = true)]
    [LogException(AttributeExclude = true)]
    public class JobAndWorkerType
    {
        public RuntimeType JobType { get; set; }
        public RuntimeType WorkerType { get; set; }

        public JobAndWorkerType Clone()
        {
            return new JobAndWorkerType
            {
                JobType = JobType,
                WorkerType = WorkerType
            };
        }

        public void WriteTo(XmlElement jobAndWorkerTypeElement)
        {
            //JobType
            Debug.Assert(jobAndWorkerTypeElement.OwnerDocument != null, "jobAndWorkerTypeElement.OwnerDocument != null");
            var jobTypeElement = jobAndWorkerTypeElement.OwnerDocument.CreateElement("JobType");
            JobType.WriteTo(jobTypeElement);
            jobAndWorkerTypeElement.AppendChild(jobTypeElement);

            //WorkerType
            Debug.Assert(jobAndWorkerTypeElement.OwnerDocument != null, "jobAndWorkerTypeElement.OwnerDocument != null");
            var workerTypeElement = jobAndWorkerTypeElement.OwnerDocument.CreateElement("WorkerType");
            WorkerType.WriteTo(workerTypeElement);
            jobAndWorkerTypeElement.AppendChild(workerTypeElement);
        }

        public void ReadFrom(XmlElement jobAndWorkerTypeElement)
        {
            //JobType
            Debug.Assert(jobAndWorkerTypeElement != null, "jobAndWorkerTypeElement != null");
            var jobTypeElement = (XmlElement)jobAndWorkerTypeElement.GetElementsByTagName("JobType")[0];
            JobType = new RuntimeType();
            JobType.ReadFrom(jobTypeElement);

            //WorkerType
            var workerTypeElement = (XmlElement)jobAndWorkerTypeElement.GetElementsByTagName("WorkerType")[0];
            WorkerType = new RuntimeType();
            WorkerType.ReadFrom(workerTypeElement);
        }
    }
}
