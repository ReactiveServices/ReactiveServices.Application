using ReactiveServices.ComputationalUnit.Dispatching;
using ReactiveServices.ComputationalUnit.Settings;
using ReactiveServices.ComputationalUnit.Work;
using ReactiveServices.MessageBus;
using System;
using System.IO;
using NLog;
using ReactiveServices.Extensions;

namespace ReactiveServices.Application
{
    public sealed class Bootstrapper : IDisposable
    {
        BootstrapSettings BootstrapSettings { get; set; }
        DispatcherLauncher Launcher { get; set; }
        ISubscriptionBus SubscriptionBus { get; set; }
        IPublishingBus PublishingBus { get; set; }
        ThreadExecutor ThreadExecutor { get; set; }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public Bootstrapper(DispatcherLauncher launcher, ISubscriptionBus subscriptionBus, IPublishingBus publishingBus)
        {
            Launcher = launcher;
            SubscriptionBus = subscriptionBus;
            PublishingBus = publishingBus;
            ThreadExecutor = new ThreadExecutor();
        }

        public void Execute(string bootstrapFileName)
        {
            var bootstrapSettings = new BootstrapSettings();
            using (var stream = new FileStream(bootstrapFileName, FileMode.Open, FileAccess.Read))
            {
                bootstrapSettings.LoadFrom(stream);
            }
            Execute(bootstrapSettings);
        }

        public void Execute(BootstrapSettings bootstrapSettings)
        {
            Log.WriteRuntimeInfo();
            ApplySettings(bootstrapSettings);
            PrepareSubscriptionsForPendingJobs();
            LaunchComputationalUnits();
            PublishJobs();
        }

        private void PrepareSubscriptionsForPendingJobs()
        {
            foreach (var bootstrapJob in BootstrapSettings.BootstrapJobs)
            {
                SubscriptionBus.PrepareSubscriptionTo(
                    bootstrapJob.JobType.Type,
                    JobStatus.Pending.TopicId(), DispatcherRepository.PendingJobsSubscriptionId);
            }
        }

        private void PublishJobs()
        {
            foreach (var bootstrapJob in BootstrapSettings.BootstrapJobs)
            {
                var job = (Job)Activator.CreateInstance(bootstrapJob.JobType.Type);
                job.Parameters = bootstrapJob.JobParameters;
                PublishingBus.Publish(JobStatus.Pending.TopicId(), job);
            }
        }

        private void ApplySettings(BootstrapSettings bootstrapSettings)
        {
            // Clone settings to avoid side effect in case the reference is changed afterwards
            BootstrapSettings = bootstrapSettings.Clone();
        }

        private void LaunchComputationalUnits()
        {
            foreach (var dispatcherSettings in BootstrapSettings.DispatcherSettings)
            {
                Launcher.Launch(dispatcherSettings);
            }
            Launcher.WaitForLaunchConfirmations(BootstrapSettings.LaunchTimeout);
        }

        public bool HasReceivedLaunchConfirmationFromAllDispatchers()
        {
            return Launcher.HasReceivedLaunchConfirmationFromAllDispatchers();
        }

        public void Dispose()
        {
            ThreadExecutor.Dispose();
            SubscriptionBus.Dispose();
            PublishingBus.Dispose();
            Launcher.Dispose();
        }
    }
}
