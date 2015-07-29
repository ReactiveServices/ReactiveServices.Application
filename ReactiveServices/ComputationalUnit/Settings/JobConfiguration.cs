using PostSharp.Patterns.Diagnostics;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml;

namespace ReactiveServices.ComputationalUnit.Settings
{
    [Log(AttributeExclude = true)]
    [LogException(AttributeExclude = true)]
    public class JobConfiguration
    {
        public JobConfiguration()
        {
            JobAndWorkerType = new JobAndWorkerType();
            RequestMaxAttempts = 1;
            RequestTimeout = TimeSpan.FromMinutes(1);
        }

        public JobAndWorkerType JobAndWorkerType { get; private set; }
        public int RequestMaxAttempts { get; set; }
        public TimeSpan RequestTimeout { get; set; }

        public JobConfiguration Clone()
        {
            return new JobConfiguration
            {
                RequestMaxAttempts = RequestMaxAttempts,
                RequestTimeout = RequestTimeout,
                JobAndWorkerType = JobAndWorkerType.Clone()
            };
        }

        public void WriteTo(XmlElement jobConfigurationElement)
        {
            //RequestMaxAttempts
            Debug.Assert(jobConfigurationElement.OwnerDocument != null, "jobConfigurationElement.OwnerDocument != null");
            var requestMaxAttemptsElement = jobConfigurationElement.OwnerDocument.CreateAttribute("RequestMaxAttempts");
            requestMaxAttemptsElement.Value = RequestMaxAttempts.ToString(CultureInfo.InvariantCulture);
            jobConfigurationElement.Attributes.Append(requestMaxAttemptsElement);

            //RequestTimeout
            var requestTimeoutElement = jobConfigurationElement.OwnerDocument.CreateAttribute("RequestTimeout");
            requestTimeoutElement.Value = RequestTimeout.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
            jobConfigurationElement.Attributes.Append(requestTimeoutElement);

            //JobAndWorkerType
            var jobAndWorkerTypeElement = jobConfigurationElement.OwnerDocument.CreateElement("JobAndWorkerType");
            JobAndWorkerType.WriteTo(jobAndWorkerTypeElement);
            jobConfigurationElement.AppendChild(jobAndWorkerTypeElement);
        }

        public void ReadFrom(XmlElement jobConfigurationElement)
        {
            //RequestMaxAttempts
            if (jobConfigurationElement.HasAttribute("RequestMaxAttempts"))
            {
                Debug.Assert(jobConfigurationElement != null, "jobConfigurationElement != null");
                var requestMaxAttemptsElement = jobConfigurationElement.GetAttribute("RequestMaxAttempts");
                RequestMaxAttempts = Int32.Parse(requestMaxAttemptsElement);
            }

            //RequestTimeout
            if (jobConfigurationElement.HasAttribute("RequestTimeout"))
            {
                var requestTimeoutElement = jobConfigurationElement.GetAttribute("RequestTimeout");
                RequestTimeout = TimeSpan.FromMilliseconds(Int32.Parse(requestTimeoutElement));
            }

            //JobAndWorkerType
            var jobAndWorkerTypeElement = (XmlElement)jobConfigurationElement.GetElementsByTagName("JobAndWorkerType")[0];
            JobAndWorkerType = new JobAndWorkerType();
            JobAndWorkerType.ReadFrom(jobAndWorkerTypeElement);
        }
    }
}
