using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ReactiveServices.ComputationalUnit.Dispatching;
using ReactiveServices.ComputationalUnit.Dispatching.Tests;
using ReactiveServices.ComputationalUnit.Settings;
using ReactiveServices.ComputationalUnit.Work;
using ReactiveServices.Configuration;
using ReactiveServices.MessageBus;

namespace ReactiveServices.Application.Launching.Tests.Specifications
{
    public sealed class StepsContext : IDisposable
    {
        private readonly string DispatcherId = "Single";
        private readonly BootstrapSettings BootstrapSettings;
        private readonly DispatcherLifeSpan DispatcherLifeSpan;
        private readonly ISubscriptionBus SubscriptionBus;
        private readonly DispatcherLauncher Launcher;
        private readonly Bootstrapper Bootstrapper;
        private readonly List<Type> JobsExecuted;

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
            SubscriptionBus = DependencyResolver.Get<ISubscriptionBus>();
            JobsExecuted = new List<Type>();
            Launcher = DependencyResolver.Get<DispatcherLauncher>();
            Bootstrapper = DependencyResolver.Get<Bootstrapper>();

            ClearAmqpResources();
        }

        public void Dispose()
        {
            ClearAmqpResources();
            Bootstrapper.Dispose();
            Launcher.Dispose();
            SubscriptionBus.Dispose();
        }

        private void ClearAmqpResources()
        {
            SubscriptionBus.DeleteSubscriptionQueue(typeof(SampleJob), DispatcherRepository.PendingJobsSubscriptionId);
            SubscriptionBus.DeleteSubscriptionQueue(typeof(AnotherSampleJob), DispatcherRepository.PendingJobsSubscriptionId);
            SubscriptionBus.DeleteSubscriptionQueue(typeof(SampleJob), DispatcherRepository.SucceededJobsSubscriptionId);
            SubscriptionBus.DeleteSubscriptionQueue(typeof(DispatcherOnline), DispatcherRepository.OnlineDispatchersSubscriptionId);
            SubscriptionBus.DeleteSubscriptionQueue(typeof(DispatcherOffline), DispatcherRepository.OfflineDispatchersSubscriptionId);
            SubscriptionBus.DeleteSubscriptionQueue(typeof(LifeSignal), ComputationalUnit.Settings.DispatcherId.FromString("Single").LifeSignalSubscriptionId);
            SubscriptionBus.DeleteSubscriptionQueue(typeof(LifeSignal), ComputationalUnit.Settings.DispatcherId.FromString("A").LifeSignalSubscriptionId);
            SubscriptionBus.DeleteSubscriptionQueue(typeof(LifeSignal), ComputationalUnit.Settings.DispatcherId.FromString("B").LifeSignalSubscriptionId);
        }

        private RuntimeType JobTypeOfCode(string jobTypeCode)
        {
            switch (jobTypeCode)
            {
                case "Single":
                case "A": return RuntimeType.From(typeof(SampleJob));
                case "B": return RuntimeType.From(typeof(AnotherSampleJob));
                default: return null;
            }
        }

        private RuntimeType WorkerTypeOfCode(string jobTypeCode)
        {
            switch (jobTypeCode)
            {
                case "Single":
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

        private void OnWorkCompleted(object completedJob)
        {
            JobsExecuted.Add(completedJob.GetType());
        }

        private string CreateBootstrapSettingsFile()
        {
            var bootstrapSettingsFileName = BootstrapSettings.GetTempBootstrapSettingsFileName();
            using (var stream = new FileStream(bootstrapSettingsFileName, FileMode.Create, FileAccess.Write))
            {
                BootstrapSettings.SaveTo(stream);
            }
            return bootstrapSettingsFileName;
        }

        internal void ConfigureApplicationExecution()
        {

            BootstrapSettings.AddDispatcherConfiguration(DispatcherId);
            BootstrapSettings.ConfigureDispatcherToHandleJob(DispatcherId,
                JobAndWorkerTypeOfCode("A"), DispatcherLifeSpan);
        }

        internal void PrepareBootstrapScript()
        {
            BootstrapSettings.AddDispatcherConfiguration("X");
            BootstrapSettings.AddDispatcherConfiguration("Y");
            BootstrapSettings.SetMaximumParallelJobsForDispatcher("X", 1);
            BootstrapSettings.SetMaximumParallelJobsForDispatcher("Y", 1);
            BootstrapSettings.ConfigureDispatcherToHandleJob("X",
                JobAndWorkerTypeOfCode("A"), DispatcherLifeSpan);
            BootstrapSettings.ConfigureDispatcherToHandleJob("Y",
                JobAndWorkerTypeOfCode("B"), DispatcherLifeSpan);
            BootstrapSettings.ConfigureBootstrapJob(1, JobTypeOfCode("A"));
            BootstrapSettings.ConfigureBootstrapJob(1, JobTypeOfCode("B"));
            SubscriptionBus.SubscribeTo<SampleJob>(
                JobStatus.Succeeded.TopicId(),
                DispatcherRepository.SucceededJobsSubscriptionId,
                OnWorkCompleted);
        }

        internal void EnsureScriptHasOnlyOneJob()
        {
            var firstRequest = BootstrapSettings.BootstrapJobs.First();
            BootstrapSettings.BootstrapJobs.Clear();
            BootstrapSettings.BootstrapJobs.Add(firstRequest);

            var firstDispatcher = BootstrapSettings.DispatcherSettings.First();
            BootstrapSettings.DispatcherSettings.Clear();
            BootstrapSettings.DispatcherSettings.Add(firstDispatcher);
        }

        internal void PrepareBootstrapScriptForTwoJobsOfSameType()
        {
            BootstrapSettings.AddDispatcherConfiguration("X");
            BootstrapSettings.AddDispatcherConfiguration("Y");
            BootstrapSettings.SetMaximumParallelJobsForDispatcher("X", 1);
            BootstrapSettings.SetMaximumParallelJobsForDispatcher("Y", 1);
            BootstrapSettings.ConfigureDispatcherToHandleJob("X",
                JobAndWorkerTypeOfCode("A"), DispatcherLifeSpan);
            BootstrapSettings.ConfigureDispatcherToHandleJob("Y",
                JobAndWorkerTypeOfCode("A"), DispatcherLifeSpan);
            BootstrapSettings.ConfigureBootstrapJob(1, JobTypeOfCode("A"));
            BootstrapSettings.ConfigureBootstrapJob(1, JobTypeOfCode("A"));
            SubscriptionBus.SubscribeTo<SampleJob>(
                JobStatus.Succeeded.TopicId(),
                DispatcherRepository.SucceededJobsSubscriptionId,
                OnWorkCompleted);
        }

        internal void ExecuteApplication()
        {
            var settings = BootstrapSettings.DispatcherSettings.Single(s => s.DispatcherId.Value.StartsWith(DispatcherId));
            Launcher.Launch(settings);

            Thread.Sleep(DispatcherLifeSpan.Timeout);
        }

        internal void ExecuteBootstrapScript()
        {
            var bootstrapSettingsFileName = CreateBootstrapSettingsFile();
            Bootstrapper.Execute(bootstrapSettingsFileName);

            Thread.Sleep(DispatcherLifeSpan.Timeout);
        }

        internal bool HasReceivedLaunchConfirmation()
        {
            return Launcher.HasReceivedLaunchConfirmationFrom(ComputationalUnit.Settings.DispatcherId.FromString(DispatcherId));
        }

        internal bool HasReceivedLaunchConfirmationFromAllDispatchers()
        {
            return Bootstrapper.HasReceivedLaunchConfirmationFromAllDispatchers();
        }

        internal bool AllJobsFromBootstrapScriptHaveStarted()
        {
            var result = JobsExecuted.Count == BootstrapSettings.BootstrapJobs.Count;
            result = result && BootstrapSettings.BootstrapJobs.All(
                job => JobsExecuted.Contains(job.JobType.Type));
            return result;
        }
    }
}
