using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace ReactiveServices.ComputationalUnit.Settings
{
    public class BootstrapJob
    {
        public BootstrapJob(RuntimeType jobType, Dictionary<string, string> jobParameters)
        {
            if (jobType == null)
                throw new ArgumentNullException("jobType");

            if (jobParameters == null)
                throw new ArgumentNullException("jobParameters");

            JobType = jobType;
            JobParameters = jobParameters;
        }

        public BootstrapJob(RuntimeType jobType)
        {
            JobType = jobType;
            JobParameters = new Dictionary<string, string>();
        }

        internal BootstrapJob()
        {
        }

        public RuntimeType JobType { get; private set; }
        public Dictionary<string, string> JobParameters { get; private set; }

        public void WriteTo(XmlElement element)
        {
            //JobType
            Debug.Assert(element.OwnerDocument != null, "element.OwnerDocument != null");
            var jobTypeElement = element.OwnerDocument.CreateElement("JobType");
            JobType.WriteTo(jobTypeElement);
            element.AppendChild(jobTypeElement);
            //JobParameters
            Debug.Assert(element.OwnerDocument != null, "element.OwnerDocument != null");
            var jobParametersElement = element.OwnerDocument.CreateElement("JobParameters");
            if (JobParameters != null)
            {
                foreach (var jobParameter in JobParameters)
                {
                    var jobParameterElement = element.OwnerDocument.CreateElement("JobParameter");
                    var jobParameterNameElement = element.OwnerDocument.CreateAttribute("Name");
                    jobParameterNameElement.Value = jobParameter.Key;
                    var jobParameterValueElement = element.OwnerDocument.CreateAttribute("Value");
                    jobParameterValueElement.Value = jobParameter.Value;
                    jobParameterElement.Attributes.Append(jobParameterNameElement);
                    jobParameterElement.Attributes.Append(jobParameterValueElement);
                    jobParametersElement.AppendChild(jobParameterElement);
                }
                element.AppendChild(jobParametersElement);
            }
        }

        public void ReadFrom(XmlElement element)
        {
            //JobType
            var jobTypeElement = (XmlElement)element.GetElementsByTagName("JobType")[0];
            JobType = new RuntimeType();
            JobType.ReadFrom(jobTypeElement);
            //JobParameters
            JobParameters = new Dictionary<string, string>();
            var hasJobParametersElements = element.GetElementsByTagName("JobParameters").Count > 0;
            if (hasJobParametersElements)
            {
                var jobParametersElements = element.GetElementsByTagName("JobParameters")[0].ChildNodes;
                foreach (var jobParametersElement in jobParametersElements.OfType<XmlElement>())
                {
                    var jobParameterNameElement = jobParametersElement.GetAttribute("Name");
                    var jobParameterValueElement = jobParametersElement.GetAttribute("Value");

                    if (String.IsNullOrWhiteSpace(jobParameterNameElement))
                        continue;

                    JobParameters.Add(jobParameterNameElement, jobParameterValueElement);
                }
            }
        }

        public BootstrapJob Clone()
        {
            return new BootstrapJob
            {
                JobType = new RuntimeType
                {
                    Type = JobType.Type
                },
                JobParameters = new Dictionary<string, string>(JobParameters)
            };
        }
    }
}
