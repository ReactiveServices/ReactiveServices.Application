using System.Globalization;
using ReactiveServices.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;

namespace ReactiveServices.ComputationalUnit.Settings
{
    public class BootstrapSettings
    {
        public TimeSpan LaunchTimeout { get; set; }
        public List<DispatcherSettings> DispatcherSettings { get; private set; }
        public List<BootstrapJob> BootstrapJobs { get; private set; }

        private BootstrapSettings(List<DispatcherSettings> dispatcherSettings, List<BootstrapJob> bootstrapJobs)
        {
            Clear();
            DispatcherSettings = dispatcherSettings;
            BootstrapJobs = bootstrapJobs;
        }

        public BootstrapSettings()
            : this(new List<DispatcherSettings>(), new List<BootstrapJob>())
        {
        }

        public BootstrapSettings Clone()
        {
            var clone = new BootstrapSettings(
                DispatcherSettings.Select(s => s.Clone()).ToList(),
                BootstrapJobs.Select(r => r.Clone()).ToList()
            )
            {
                LaunchTimeout = LaunchTimeout
            };
            return clone;
        }

        public string GetTempBootstrapSettingsFileName()
        {
            var dispatcherIds = DispatcherSettings.Select(
                d => d.DispatcherId
            ).Select(
                d => d.Value
            ).Aggregate( 
                (d1, d2) => d1 + "-" + d2
            );
            var md5 = System.Security.Cryptography.MD5.Create();
            var fileName = dispatcherIds.GetMd5Hash(md5);
            var path = String.Format("{0}ReactiveServices.BootstrapSettings", Path.GetTempPath());
            Thread.Sleep(1);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            
            var launchSettingsFileName = Path.ChangeExtension(Path.Combine(path, fileName),".xml");

            return launchSettingsFileName;
        }

        public void SaveTo(Stream stream)
        {
            var xml = new XmlDocument();
            xml.AppendChild(xml.CreateElement("BootstrapSettings"));
            WriteTo(xml.DocumentElement);
            xml.Save(stream);
        }

        public void LoadFrom(Stream stream)
        {
            var xml = new XmlDocument();
            stream.Position = 0;
            var xmlContent = new StreamReader(stream).ReadToEnd();
            xml.LoadXml(xmlContent);
            ReadFrom(xml.DocumentElement);
        }

        public void LoadFrom(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open))
            {
                LoadFrom(stream);
            }
        }

        public void WriteTo(XmlElement bootstrapSettingsElement)
        {
            //LaunchTimeout
            Debug.Assert(bootstrapSettingsElement.OwnerDocument != null, "bootstrapSettingsElement.OwnerDocument != null");
            var launchTimeoutElement = bootstrapSettingsElement.OwnerDocument.CreateAttribute("LaunchTimeout");
            launchTimeoutElement.Value = LaunchTimeout.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
            bootstrapSettingsElement.Attributes.Append(launchTimeoutElement);

            //DispatcherSettings
            Debug.Assert(bootstrapSettingsElement.OwnerDocument != null, "bootstrapSettingsElement.OwnerDocument != null");
            var dispatchersElement = bootstrapSettingsElement.OwnerDocument.CreateElement("Dispatchers");
            foreach (var dispacherSettings in DispatcherSettings)
            {
                var dispatcherSettingsElement = bootstrapSettingsElement.OwnerDocument.CreateElement("DispatcherSettings");
                dispacherSettings.WriteTo(dispatcherSettingsElement);
                dispatchersElement.AppendChild(dispatcherSettingsElement);
            }
            bootstrapSettingsElement.AppendChild(dispatchersElement);

            //BootstrapJobs
            Debug.Assert(bootstrapSettingsElement.OwnerDocument != null, "bootstrapSettingsElement.OwnerDocument != null");
            var bootstrapJobTypeElements = bootstrapSettingsElement.OwnerDocument.CreateElement("BootstrapJobs");
            foreach (var bootstrapJob in BootstrapJobs)
            {
                var bootstrapJobElement = bootstrapSettingsElement.OwnerDocument.CreateElement("BootstrapJob");
                bootstrapJob.WriteTo(bootstrapJobElement);
                bootstrapJobTypeElements.AppendChild(bootstrapJobElement);
            }
            bootstrapSettingsElement.AppendChild(bootstrapJobTypeElements);
        }

        public void ReadFrom(XmlElement bootstrapSettingsElement)
        {
            //LaunchTimeout
            if (bootstrapSettingsElement.HasAttribute("LaunchTimeout"))
            {
                int launchTimeout;
                LaunchTimeout = Int32.TryParse(bootstrapSettingsElement.GetAttribute("LaunchTimeout"), out launchTimeout)
                    ? TimeSpan.FromMilliseconds(launchTimeout)
                    : TimeSpan.FromSeconds(30);
            }

            //DispatcherSettings
            var dispatchersElements = bootstrapSettingsElement.GetElementsByTagName("Dispatchers")[0].ChildNodes;
            foreach (XmlElement dispatcherSettingsElement in dispatchersElements)
            {
                var dispatcherSettings = new DispatcherSettings();
                dispatcherSettings.ReadFrom(dispatcherSettingsElement);
                DispatcherSettings.Add(dispatcherSettings);
            }

            //BootstrapJobs
            var bootstrapSettingsElementsExists =
                bootstrapSettingsElement.GetElementsByTagName("BootstrapJobs").Count == 1 &&
                bootstrapSettingsElement.GetElementsByTagName("BootstrapJob").Count > 0;

            if (!bootstrapSettingsElementsExists) 
                return;
            var bootstrapJobsElements =
                bootstrapSettingsElement.GetElementsByTagName("BootstrapJobs")[0].ChildNodes;
            foreach (XmlElement bootstrapJobElement in bootstrapJobsElements)
            {
                var bootstrapJob = new BootstrapJob();
                bootstrapJob.ReadFrom(bootstrapJobElement);
                BootstrapJobs.Add(bootstrapJob);
            }
        }

        public DispatcherSettings AddDispatcherConfiguration(string dispatcherId)
        {
            var dispatcherSettings = new DispatcherSettings
            {
                DispatcherId = DispatcherId.FromString(dispatcherId)
            };
            DispatcherSettings.Add(dispatcherSettings);
            return dispatcherSettings;
        }

        public void SetMaximumParallelJobsForDispatcher(string dispatcherId, int numberOfParallelJobs)
        {
            var dispatcherSettings = DispatcherSettings.Single(c => c.DispatcherId.Value == dispatcherId);
            dispatcherSettings.MaximumNumberOfProcessingJobs = numberOfParallelJobs;
        }

        public void ConfigureDispatcherToHandleJob(string dispatcherId, JobAndWorkerType jobAndWorkerType, DispatcherLifeSpan dispatcherLifeSpan)
        {
            var dispatcherSettings = DispatcherSettings.Single(c => c.DispatcherId.Value == dispatcherId);
            dispatcherSettings.DispatcherLifeSpan = dispatcherLifeSpan;
            if (dispatcherSettings.JobConfigurations.Any(
                c => c.JobAndWorkerType.JobType == jobAndWorkerType.JobType)) return;
            var jobConfiguration = new JobConfiguration
            {
                JobAndWorkerType =
                {
                    JobType = jobAndWorkerType.JobType,
                    WorkerType = jobAndWorkerType.WorkerType
                }
            };
            dispatcherSettings.JobConfigurations.Add(jobConfiguration);
        }

        public void ConfigureBootstrapJob(int countOfInstances, RuntimeType jobType, Dictionary<string, string> jobParameters = null)
        {
            for (var i = 0; i < countOfInstances; i++)
                BootstrapJobs.Add(jobParameters == null
                    ? new BootstrapJob(jobType)
                    : new BootstrapJob(jobType, jobParameters));
        }

        public void Clear()
        {
            LaunchTimeout = TimeSpan.FromSeconds(60);
            DispatcherSettings = new List<DispatcherSettings>();
            BootstrapJobs = new List<BootstrapJob>();
        }
    }
}
