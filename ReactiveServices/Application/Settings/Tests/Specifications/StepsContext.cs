using System;
using System.Linq;
using System.IO;
using ReactiveServices.ComputationalUnit.Dispatching.Tests;
using ReactiveServices.ComputationalUnit.Settings;
using ReactiveServices.Configuration;

namespace ReactiveServices.Application.Settings.Tests.Specifications
{
    public sealed class StepsContext : IDisposable
    {
        private readonly BootstrapSettings BootstrapSettings;
        private readonly DispatcherLifeSpan DispatcherLifeSpan;

        private readonly MemoryStream BootstrapperSettingsStream;

        public StepsContext()
        {
            DependencyResolver.Reset();
            DependencyResolver.Initialize();
            DispatcherLifeSpan = new DispatcherLifeSpan
            {
                Mode = DispatcherLifeSpanMode.UntilTimedOut,
                Timeout = TimeSpan.FromSeconds(10)
            };
            BootstrapSettings = new BootstrapSettings();
            BootstrapperSettingsStream = new MemoryStream();
        }

        public void Dispose()
        {

        }

        private RuntimeType JobTypeOfCode(string jobTypeCode)
        {
            switch (jobTypeCode)
            {
                case "A": return RuntimeType.From(typeof(SampleJob));
                case "B": return RuntimeType.From(typeof(AnotherSampleWorker));
                default: return null;
            }
        }

        private RuntimeType WorkerTypeOfCode(string jobTypeCode)
        {
            switch (jobTypeCode)
            {
                case "A": return RuntimeType.From(typeof(SampleWorker));
                case "B": return RuntimeType.From(typeof(AnotherSampleWorker));
                default: return null;
            }
        }

        private JobAndWorkerType JobAndWorkerTypeOfCode(string jobTypeCode)
        {
            var jobType = JobTypeOfCode(jobTypeCode);
            var workerType = WorkerTypeOfCode(jobTypeCode);
            var jobAndWorkerType = new JobAndWorkerType
            {
                JobType = jobType,
                WorkerType = workerType
            };
            return jobAndWorkerType;
        }


        internal void AddDispatcherConfigurationFor(string dispatcherId)
        {
            BootstrapSettings.AddDispatcherConfiguration(dispatcherId);
        }

        internal void SetMaximumParallelJobsForDispatcher(string dispatcherId, int numberOfParallelJobs)
        {
            BootstrapSettings.SetMaximumParallelJobsForDispatcher(dispatcherId, numberOfParallelJobs);
        }

        internal void ConfigureDispatcherToHandleJob(string dispatcherId, string jobTypeCode)
        {
            BootstrapSettings.ConfigureDispatcherToHandleJob(dispatcherId, JobAndWorkerTypeOfCode(jobTypeCode), DispatcherLifeSpan);
        }

        internal void ConfigureBootstrapToExecuteJob(int countOfInstances, string jobTypeCode)
        {
            var jobType = JobTypeOfCode(jobTypeCode);
            BootstrapSettings.ConfigureBootstrapJob(countOfInstances, jobType);
        }

        internal void SaveBootstrapSettings()
        {
            BootstrapSettings.SaveTo(BootstrapperSettingsStream);
        }

        internal void LoadBootstrapSettings()
        {
            BootstrapSettings.Clear();
            BootstrapSettings.LoadFrom(BootstrapperSettingsStream);
        }

        internal int CountOfRequestedInstancesOfDispatchersOfId(string dispatcherId)
        {
            return BootstrapSettings.DispatcherSettings.Count(s => s.DispatcherId.Value == dispatcherId);
        }

        internal int NumberOfJobsThatCanBeProcessedInParallel(string dispatcherId)
        {
            return BootstrapSettings.DispatcherSettings.Single(s => s.DispatcherId.Value == dispatcherId)
                .MaximumNumberOfProcessingJobs;
        }

        internal bool CanProcessJobsOfType(string dispatcherId, string jobTypeCode)
        {
            var jobType = JobTypeOfCode(jobTypeCode);
            return BootstrapSettings.DispatcherSettings.Any(
                s => s.DispatcherId.Value == dispatcherId && s.JobConfigurations.Any(
                    c => c.JobAndWorkerType.JobType == jobType));
        }

        internal int NumberOfJobsToBeRequestedAtBootstrap(string jobTypeCode)
        {
            var jobType = JobTypeOfCode(jobTypeCode);
            return BootstrapSettings.BootstrapJobs.Count(s => s.JobType.Type == jobType.Type);
        }
    }
}
