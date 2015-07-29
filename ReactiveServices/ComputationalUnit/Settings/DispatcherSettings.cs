using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using NLog;

namespace ReactiveServices.ComputationalUnit.Settings
{
    public class DispatcherSettings
    {
        public DispatcherSettings()
        {
            JobConfigurations = new List<JobConfiguration>();
            MaximumNumberOfProcessingJobs = 1;
            IntervalForCheckingUnfinishedJobs = TimeSpan.FromSeconds(5);
            DispatcherLifeSpan = new DispatcherLifeSpan
            {
                Mode = DispatcherLifeSpanMode.Perpetual
            };
        }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public DispatcherId DispatcherId { get; set; }
        public DispatcherLifeSpan DispatcherLifeSpan { get; set; }
        public List<JobConfiguration> JobConfigurations { get; set; }
        public TimeSpan IntervalForCheckingUnfinishedJobs { get; set; }
        public bool RequireAdministratorPriviledges { get; set; }

        /// <summary>
        /// Maximum time the workrequest can take to execute all its works
        /// </summary>
        public TimeSpan MaximumExecutionTime
        {
            get
            {
                if (JobConfigurations.Count == 0)
                    return TimeSpan.Zero;

                var longestRunningJob = JobConfigurations.OrderByDescending(
                    r => r.RequestTimeout.TotalSeconds * r.RequestMaxAttempts
                ).First();

                var result = (longestRunningJob.RequestTimeout.TotalSeconds * longestRunningJob.RequestMaxAttempts);

                result = result + 15; // Give some extra time to finish execution //TODO: Remove this extra time

                return TimeSpan.FromSeconds(result);
            }
        }
        public int MaximumNumberOfProcessingJobs { get; set; }

        public DispatcherSettings Clone()
        {
            return new DispatcherSettings
            {
                DispatcherId = DispatcherId,
                DispatcherLifeSpan = DispatcherLifeSpan,
                IntervalForCheckingUnfinishedJobs = IntervalForCheckingUnfinishedJobs,
                MaximumNumberOfProcessingJobs = MaximumNumberOfProcessingJobs,
                JobConfigurations = JobConfigurations.Select(c => c.Clone()).ToList(),
                RequireAdministratorPriviledges = RequireAdministratorPriviledges
            };
        }

        public void WriteTo(XmlElement dispatcherSettingsElement)
        {
            //DispatcherId
            Debug.Assert(dispatcherSettingsElement.OwnerDocument != null, "dispatcherSettingsElement.OwnerDocument != null");
            var dispatcherIdElement = dispatcherSettingsElement.OwnerDocument.CreateAttribute("DispatcherId");
            dispatcherIdElement.Value = DispatcherId.Value;
            dispatcherSettingsElement.Attributes.Append(dispatcherIdElement);

            //RequireAdministratorPriviledges
            Debug.Assert(dispatcherSettingsElement.OwnerDocument != null, "dispatcherSettingsElement.OwnerDocument != null");
            var requireAdministratorPriviledgesElement = dispatcherSettingsElement.OwnerDocument.CreateAttribute("RequireAdministratorPriviledges");
            requireAdministratorPriviledgesElement.Value = RequireAdministratorPriviledges.ToString();
            dispatcherSettingsElement.Attributes.Append(requireAdministratorPriviledgesElement);

            //IntervalForCheckingUnfinishedJobs
            var intervalForCheckingUnfinishedJobsElement = dispatcherSettingsElement.OwnerDocument.CreateAttribute("IntervalForCheckingUnfinishedJobs");
            intervalForCheckingUnfinishedJobsElement.Value = IntervalForCheckingUnfinishedJobs.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
            dispatcherSettingsElement.Attributes.Append(intervalForCheckingUnfinishedJobsElement);

            //MaximumNumberOfProcessingJobs
            var maximumNumberOfProcessingJobsElement = dispatcherSettingsElement.OwnerDocument.CreateAttribute("MaximumNumberOfProcessingJobs");
            maximumNumberOfProcessingJobsElement.Value = MaximumNumberOfProcessingJobs.ToString(CultureInfo.InvariantCulture);
            dispatcherSettingsElement.Attributes.Append(maximumNumberOfProcessingJobsElement);

            //LifeSpan
            var lifeSpanElement = dispatcherSettingsElement.OwnerDocument.CreateElement("LifeSpan");
            var lifeSpanModeElement = dispatcherSettingsElement.OwnerDocument.CreateAttribute("Mode");
            lifeSpanModeElement.Value = DispatcherLifeSpan.Mode.ToString();
            lifeSpanElement.Attributes.Append(lifeSpanModeElement);
            var lifeSpanTimeoutElement = dispatcherSettingsElement.OwnerDocument.CreateAttribute("Timeout");
            lifeSpanTimeoutElement.Value = DispatcherLifeSpan.Timeout.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
            lifeSpanElement.Attributes.Append(lifeSpanTimeoutElement);
            dispatcherSettingsElement.AppendChild(lifeSpanElement);

            //JobConfigurations
            Debug.Assert(dispatcherSettingsElement.OwnerDocument != null, "dispatcherSettingsElement.OwnerDocument != null");
            var jobConfigurationsElement = dispatcherSettingsElement.OwnerDocument.CreateElement("JobConfigurations");
            foreach (var jobConfiguration in JobConfigurations)
            {
                var jobConfigurationElement = dispatcherSettingsElement.OwnerDocument.CreateElement("JobConfiguration");
                jobConfiguration.WriteTo(jobConfigurationElement);
                jobConfigurationsElement.AppendChild(jobConfigurationElement);
            }
            dispatcherSettingsElement.AppendChild(jobConfigurationsElement);
        }

        public void ReadFrom(XmlElement dispatcherSettingsElement)
        {
            //DispatcherId
            DispatcherId = DispatcherId.FromString(dispatcherSettingsElement.GetAttribute("DispatcherId"));

            //RequireAdministratorPriviledges
            if (dispatcherSettingsElement.HasAttribute("RequireAdministratorPriviledges"))
                RequireAdministratorPriviledges = Boolean.Parse(dispatcherSettingsElement.GetAttribute("RequireAdministratorPriviledges"));
            
            //IntervalForCheckingUnfinishedJobs
            if (dispatcherSettingsElement.HasAttribute("IntervalForCheckingUnfinishedJobs"))
                IntervalForCheckingUnfinishedJobs = TimeSpan.FromMilliseconds(Int32.Parse(dispatcherSettingsElement.GetAttribute("IntervalForCheckingUnfinishedJobs")));

            //MaximumNumberOfProcessingJobs
            if (dispatcherSettingsElement.HasAttribute("MaximumNumberOfProcessingJobs"))
                MaximumNumberOfProcessingJobs = Int32.Parse(dispatcherSettingsElement.GetAttribute("MaximumNumberOfProcessingJobs"));

            //LifeSpan
            var lifeSpanElementExists = dispatcherSettingsElement.GetElementsByTagName("LifeSpan").Count == 1;
            if (lifeSpanElementExists)
            {

                var lifeSpanElement = (XmlElement)dispatcherSettingsElement.GetElementsByTagName("LifeSpan")[0];
                DispatcherLifeSpan = new DispatcherLifeSpan
                {
                    Mode =
                        (DispatcherLifeSpanMode)
                            Enum.Parse(typeof(DispatcherLifeSpanMode), lifeSpanElement.GetAttribute("Mode")),
                    Timeout = TimeSpan.FromMilliseconds(Int32.Parse(lifeSpanElement.GetAttribute("Timeout")))
                };
            }

            //JobConfigurations
            var jobConfigurationsElements = dispatcherSettingsElement.GetElementsByTagName("JobConfigurations")[0].ChildNodes;
            foreach (XmlElement jobConfigurationElement in jobConfigurationsElements)
            {
                var jobConfiguration = new JobConfiguration();
                jobConfiguration.ReadFrom(jobConfigurationElement);
                JobConfigurations.Add(jobConfiguration);
            }
        }

        public void SaveTo(Stream stream)
        {
            var xml = new XmlDocument();
            xml.AppendChild(xml.CreateElement("DispatcherSettings"));
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

        public void Validate()
        {
            string exception = null;

            if (DispatcherId == null)
            {
                exception = "DispatcherId cannot be null";
            }
            else if (JobConfigurations.Count == 0)
            {
                exception = "JobConfigurations cannot be empty";
            }
            else if (MaximumNumberOfProcessingJobs == 0)
            {
                exception = "MaximumNumberOfProcessingJobs cannot be 0";
            }
            else if (IntervalForCheckingUnfinishedJobs.TotalMilliseconds < 100 || IntervalForCheckingUnfinishedJobs.TotalSeconds > 20)
            {
                exception = "IntervalForCheckingUnfinishedJobs must be between 100 and 20000 milliseconds";
            }

            foreach (var configuration in JobConfigurations)
            {
                if (configuration.JobAndWorkerType.JobType == null || configuration.JobAndWorkerType.WorkerType == null)
                {
                    exception = "JobType and WorkerType cannot be null";
                    break;
                }

                var jobType = configuration.JobAndWorkerType.JobType.Type;

                if (jobType == null)
                {
                    exception = "JobType.Type and WorkerType cannot be null";
                    break;
                }

                if (configuration.RequestMaxAttempts <= 0)
                {
                    exception = "RequestMaxAttempts must be greater than 0";
                    break;
                }

                if (configuration.RequestTimeout.TotalSeconds <= 0 && configuration.RequestTimeout != Timeout.InfiniteTimeSpan)
                {
                    exception = "RequestTimeout must be greater than 0";
                    break;
                }
            }

            if (exception != null)
            {
                Log.Error("Invalid dispatcher settings: {0}", exception);
                throw new InvalidOperationException(exception);
            }
        }
    }
}