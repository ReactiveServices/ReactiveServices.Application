using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;
using ReactiveServices.ComputationalUnit.Settings;
using ReactiveServices.ComputationalUnit.Work;
using ReactiveServices.Configuration;
using ReactiveServices.MessageBus;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PostSharp.Patterns.Diagnostics;

namespace ReactiveServices.ComputationalUnit.Dispatching
{
    internal sealed class DispatcherRepository : IDisposable
    {
        public DispatcherSettings Settings { get; private set; }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // TODO: Replace by ReactiveServices.Extentions.LogExtensions.Entries()
        private MemoryTarget _memoryLog;
        private MemoryTarget MemoryLog
        {
            get
            {
                if (_memoryLog != null)
                    return _memoryLog;

                if (LogManager.Configuration == null)
                    throw new ArgumentException("NLog configuration could not be loaded!");

                var target = LogManager.Configuration.FindTargetByName("memory");
                if (target is AsyncTargetWrapper)
                    _memoryLog = (target as AsyncTargetWrapper).WrappedTarget as MemoryTarget;
                else
                    _memoryLog = target as MemoryTarget;

                if (_memoryLog == null)
                    throw new ArgumentException("Memory target named 'memory' not found on NLog configuration!");

                return _memoryLog;
            }
        }

        private ISubscriptionBus SubscriptionBus { get; set; }
        private IPublishingBus PublishingBus { get; set; }
        private ISendingBus SendingBus { get; set; }
        private IReceivingBus ReceivingBus { get; set; }
        private IRequestBus RequestBus { get; set; }
        private IResponseBus ResponseBus { get; set; }

        public static readonly TopicId PendingJobsTopic = JobStatus.Pending.TopicId();

        public static readonly SubscriptionId PendingJobsSubscriptionId = SubscriptionId.FromString("PendingJobs");
        public static readonly SubscriptionId SucceededJobsSubscriptionId = SubscriptionId.FromString("SucceededJobs");
        public static readonly SubscriptionId ProcessingJobsSubscriptionId = SubscriptionId.FromString("ProcessingJobs");
        public static readonly SubscriptionId FailedJobsSubscriptionId = SubscriptionId.FromString("FailedJobs");
        public static readonly SubscriptionId OnlineDispatchersSubscriptionId = SubscriptionId.FromString("OnlineDispatchers");
        public static readonly SubscriptionId OfflineDispatchersSubscriptionId = SubscriptionId.FromString("OfflineDispatchers");
        public static readonly SubscriptionId RestartedDispatchersSubscriptionId = SubscriptionId.FromString("RestartedDispatchers");
        public static readonly SubscriptionId DetachedDispatchersSubscriptionId = SubscriptionId.FromString("DetachedDispatchers");

        internal bool IsDisposed { get; private set; }

        [Log]
        [LogException]
        public DispatcherRepository(DispatcherSettings settings)
        {
            Settings = settings;
            SubscriptionBus = DependencyResolver.Get<ISubscriptionBus>();
            PublishingBus = DependencyResolver.Get<IPublishingBus>();
            ReceivingBus = DependencyResolver.Get<IReceivingBus>();
            RequestBus = DependencyResolver.Get<IRequestBus>();
            SendingBus = DependencyResolver.Get<ISendingBus>();
            ResponseBus = DependencyResolver.Get<IResponseBus>();
        }

        internal void SubscribeToPoisonPill(Action<PoisonPill> takePoisonPill)
        {
            Log.Info("Subscribing to poison pill at {0}", Settings.DispatcherId);
            // Se inscreve para receber trabalhos pendentes
            ReceivingBus.Receive<PoisonPill>(
                Settings.DispatcherId.PoisonPillSubscriptionId,
                poisonPill => takePoisonPill((PoisonPill)poisonPill),
                SubscriptionMode.Exclusive
            );
        }

        internal void RemoveSubscriptionToPendingJobs()
        {
            SubscriptionBus.RemoveSubscriptions(PendingJobsSubscriptionId);
        }

        private void Republish(Job job, JobStatus newRequestStatus)
        {
            var oldRequestStatus = job.ExecutionStatus;

            job.ExecutionStatus = newRequestStatus;

            Log.DispatcherActivity(Settings.DispatcherId, DispatcherActivity.JobWasRepublished, job, oldRequestStatus, newRequestStatus);

            PublishingBus.Publish(newRequestStatus.TopicId(), job);
        }

        internal void RepublishAsPending(Job job)
        {
            Republish(job, JobStatus.Pending);
        }

        internal void RepublishAsFailed(Job job)
        {
            Republish(job, JobStatus.Failed);
        }

        internal void PublishLaunchConfirmation()
        {
            PublishingBus.Publish(new LaunchConfirmation
            {
                DispatcherId = Settings.DispatcherId
            }, StorageType.NonPersistent);
        }

        internal void RegisterCompletionOf(Job job)
        {
            Log.DispatcherActivity(Settings.DispatcherId, DispatcherActivity.JobWasCompleted, job, job.ExecutionStatus, job.ExecutionStatus);
        }

        internal IEnumerable<DispatcherActivityLogEntry> GetDispatcherEvents()
        {
            return MemoryLog.GetDispatcherEvents(Settings.DispatcherId);
        }

        internal void SubscribeToPendingJobs(Action<Job> receiveJob)
        {
            foreach (var configuration in Settings.JobConfigurations)
            {
                SubscribeToPendingJob(configuration, receiveJob);
            }
        }

        private void SubscribeToPendingJob(JobConfiguration configuration, Action<Job> receiveJob)
        {
            if ((configuration.JobAndWorkerType.JobType.Type != typeof(Job)) &&
                (!configuration.JobAndWorkerType.JobType.Type.IsSubclassOf(typeof(Job))))
                throw new ArgumentException("MessageType must be a subtype of Job");

            // Will block when receive a work request and receive another only after the first is processed.
            // If it fails, the work request will return to the pending queue to be retried

            SubscriptionBus.SubscribeTo(
                configuration.JobAndWorkerType.JobType.Type,
                JobStatus.Pending.TopicId(),
                PendingJobsSubscriptionId,
                message => receiveJob((Job)message));
        }

        private readonly CancellationTokenSource SendLifeSignalCancellationTokenSource = new CancellationTokenSource();

        internal void StartSendingLifeSignal()
        {
            Log.Info("Start sending life signals at dispatcher {0}", Settings.DispatcherId);
            Task.Run(() =>
            {
                var lifeSignalInterval = TimeSpan.FromSeconds(1);
                while (!SendLifeSignalCancellationTokenSource.IsCancellationRequested)
                {
                    Thread.Sleep(lifeSignalInterval);
                    SendingBus.Send(
                        new LifeSignal { SourceId = Settings.DispatcherId },
                        Settings.DispatcherId.LifeSignalSubscriptionId,
                        StorageType.NonPersistent, 
                        expiration: lifeSignalInterval);
                }

            }, SendLifeSignalCancellationTokenSource.Token);
        }

        internal void StopSendingLifeSignals()
        {
            PublishDetachDispatcherRequest();
            SendLifeSignalCancellationTokenSource.Cancel();
        }

        internal void PublishDetachDispatcherRequest()
        {
            // Request the supervisor to stop monitoring this dispatcher
            var detachDispatcher = new DetachDispatcher { DispatcherId = Settings.DispatcherId };
            PublishingBus.Publish(Settings.DispatcherId.DetachDispatcherTopicId, detachDispatcher, StorageType.NonPersistent);
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            RemoveSubscriptionToPendingJobs();
            StopSendingLifeSignals();

            SubscriptionBus.Dispose();
            PublishingBus.Dispose();
            ReceivingBus.Dispose();
            RequestBus.Dispose();
            SendingBus.Dispose();
            ResponseBus.Dispose();

            IsDisposed = true;
        }
    }
}
