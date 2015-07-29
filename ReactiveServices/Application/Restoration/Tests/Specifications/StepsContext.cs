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

namespace ReactiveServices.Application.Restoration.Tests.Specifications
{
    public sealed class StepsContext : IDisposable
    {
        private readonly Supervisor Supervisor;
        private readonly DispatcherLifeSpan DispatcherLifeSpan;
        private readonly BootstrapSettings BootstrapSettings;
        private readonly ISubscriptionBus SubscriptionBus;
        private readonly ISendingBus SendingBus;
        private readonly List<DispatcherId> WorkDispatchersOnline;
        private readonly List<DispatcherId> WorkDispatchersRestarted;


        public StepsContext()
        {
            DependencyResolver.Reset();
            DependencyResolver.Initialize();

            DispatcherLifeSpan = new DispatcherLifeSpan
            {
                Mode = DispatcherLifeSpanMode.UntilTimedOut,
                Timeout = TimeSpan.FromSeconds(15)
            };
            BootstrapSettings = new BootstrapSettings();

            WorkDispatchersOnline = new List<DispatcherId>();
            WorkDispatchersRestarted = new List<DispatcherId>();

            SubscriptionBus = DependencyResolver.Get<ISubscriptionBus>();
            SendingBus = DependencyResolver.Get<ISendingBus>();
            Supervisor = DependencyResolver.Get<Supervisor>();

            ClearAmqpResources();
        }

        public void Dispose()
        {
            ClearAmqpResources();

            SubscriptionBus.Dispose();
            SendingBus.Dispose();
            Supervisor.Dispose();
        }

        private void ClearAmqpResources()
        {
            SubscriptionBus.DeleteSubscriptionQueue(typeof(SampleJob), DispatcherRepository.PendingJobsSubscriptionId);
            SubscriptionBus.DeleteSubscriptionQueue(typeof(SampleListenerJob), DispatcherRepository.PendingJobsSubscriptionId);
            SubscriptionBus.DeleteSubscriptionQueue(typeof(DispatcherOnline), DispatcherRepository.OnlineDispatchersSubscriptionId);
            SubscriptionBus.DeleteSubscriptionQueue(typeof(DispatcherOffline), DispatcherRepository.OfflineDispatchersSubscriptionId);
            SubscriptionBus.DeleteSubscriptionQueue(typeof(DispatcherRestarted), DispatcherRepository.RestartedDispatchersSubscriptionId);
            SubscriptionBus.DeleteSubscriptionQueue(typeof(LifeSignal), DispatcherId.FromString("RestoringTest1").LifeSignalSubscriptionId);
            SubscriptionBus.DeleteSubscriptionQueue(typeof(LifeSignal), DispatcherId.FromString("RestoringTest2").LifeSignalSubscriptionId);
        }

        internal void AddDispatcherConfigurationFor(string dispatcherId)
        {
            BootstrapSettings.AddDispatcherConfiguration(dispatcherId);
        }

        public void ConfigureSupervisorFor(string dispatcherId)
        {
            SubscribeToDispatcherOnlineEvent();
            SubscribeToDisparcherRestartedEvent();

            Supervisor.IsComputationalUnitRestorationEnabled = true;
            Supervisor.Initialize(BootstrapSettings);
        }

        private void SubscribeToDisparcherRestartedEvent()
        {
            if (!SubscriptionBus.IsListenningTo(typeof(DispatcherRestarted), DispatcherRepository.RestartedDispatchersSubscriptionId))
            {
                SubscriptionBus.SubscribeTo<DispatcherRestarted>(
                    DispatcherRepository.RestartedDispatchersSubscriptionId,
                    OnWorkDispatcherRestartedReceived
                );
            }
        }

        private void SubscribeToDispatcherOnlineEvent()
        {
            if (!SubscriptionBus.IsListenningTo(typeof(DispatcherOnline), DispatcherRepository.OnlineDispatchersSubscriptionId))
            {
                SubscriptionBus.SubscribeTo<DispatcherOnline>(
                    DispatcherRepository.OnlineDispatchersSubscriptionId,
                    OnWorkDispatcherOnlineReceived
                );
            }
        }

        private void OnWorkDispatcherOnlineReceived(object message)
        {
            var dispatcherId = ((DispatcherOnline)message).DispatcherId;
            if (WorkDispatchersOnline.Any(d => d == dispatcherId))
                return;

            WorkDispatchersOnline.Add(dispatcherId);
        }

        private void OnWorkDispatcherRestartedReceived(object message)
        {
            var dispatcherId = ((DispatcherRestarted)message).DispatcherId;
            if (!WorkDispatchersRestarted.Contains(dispatcherId))
                WorkDispatchersRestarted.Add(dispatcherId);
        }

        internal void SetMaximumParallelJobsForDispatcher(string dispatcherId, int numberOfParallelJobs)
        {
            BootstrapSettings.SetMaximumParallelJobsForDispatcher(dispatcherId, numberOfParallelJobs);
        }

        internal void ConfigureDispatcherToHandleJob(string dispatcherId, string jobTypeCode)
        {
            BootstrapSettings.ConfigureDispatcherToHandleJob(dispatcherId, JobAndWorkerTypeOfCode(jobTypeCode), DispatcherLifeSpan);
        }

        private RuntimeType JobTypeOfCode(string jobTypeCode)
        {
            switch (jobTypeCode)
            {
                case "Worker": return RuntimeType.From(typeof(SampleJob));
                case "Listener": return RuntimeType.From(typeof(SampleListenerJob));
                default: return null;
            }
        }

        private RuntimeType WorkerTypeOfCode(string jobTypeCode)
        {
            switch (jobTypeCode)
            {
                case "Worker": return RuntimeType.From(typeof(SampleWorker));
                case "Listener": return RuntimeType.From(typeof(SampleListener));
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

        internal void ConfigureBootstrapToExecuteJob(int numberOfRequests, string jobTypeCode)
        {
            var jobType = JobTypeOfCode(jobTypeCode);
            BootstrapSettings.ConfigureBootstrapJob(numberOfRequests, jobType);
        }

        internal void TerminateDispatcher(string dispatcherId)
        {
            Thread.Sleep(3000);

            var poisonPill = new PoisonPill
            {
                EffectOnCurrentWork = PoisonPillEffect.Kill,
                DispatcherId = DispatcherId.FromString(dispatcherId)
            };

            SendingBus.Send(poisonPill, poisonPill.DispatcherId.PoisonPillSubscriptionId, StorageType.NonPersistent);

            Thread.Sleep(10000);
        }

        internal void ExecuteSupervisor()
        {
            Supervisor.Bootstrap();
        }

        internal bool IsDispatcherOnline(string dispatcherId)
        {
            Thread.Sleep(TimeSpan.FromSeconds(3));

            return WorkDispatchersOnline.Any(d => d.Value == dispatcherId);
        }

        internal bool HasDispatcherRestarted(string dispatcherId)
        {
            Thread.Sleep(Supervisor.MaxTimeDispatcherCanBeSilent.Add(TimeSpan.FromSeconds(3)));

            return WorkDispatchersRestarted.Any(d => d.Value == dispatcherId);
        }
    }
}
