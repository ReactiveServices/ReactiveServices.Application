using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ReactiveServices.ComputationalUnit.Dispatching;
using ReactiveServices.ComputationalUnit.Dispatching.Tests;
using ReactiveServices.ComputationalUnit.Settings;
using ReactiveServices.ComputationalUnit.Work;
using ReactiveServices.Configuration;
using ReactiveServices.MessageBus;

namespace ReactiveServices.Application.Monitoring.Tests.Specifications
{
    public sealed class StepsContext : IDisposable
    {
        private readonly Supervisor Supervisor;
        private readonly List<DispatcherId> WorkDispatchersOffline;
        private readonly List<DispatcherId> WorkDispatchersOnline;
        private readonly ISubscriptionBus SubscriptionBus;
        private readonly ISendingBus SendingBus;
        private readonly DispatcherLauncher Launcher;

        public StepsContext()
        {
            DependencyResolver.Reset();
            DependencyResolver.Initialize();

            Supervisor = DependencyResolver.Get<Supervisor>();
            SubscriptionBus = DependencyResolver.Get<ISubscriptionBus>();
            SendingBus = DependencyResolver.Get<ISendingBus>();
            Launcher = DependencyResolver.Get<DispatcherLauncher>();

            WorkDispatchersOffline = new List<DispatcherId>();
            WorkDispatchersOnline = new List<DispatcherId>();

            ClearAmqpResources();
        }

        public void Dispose()
        {
            ClearAmqpResources();
            Supervisor.Dispose();
            SubscriptionBus.Dispose();
            SendingBus.Dispose();
            Launcher.Dispose();
        }

        private void ClearAmqpResources()
        {
            SubscriptionBus.DeleteSubscriptionQueue(typeof(SampleJob), DispatcherRepository.PendingJobsSubscriptionId);
            SubscriptionBus.DeleteSubscriptionQueue(typeof(DispatcherOnline), DispatcherRepository.OnlineDispatchersSubscriptionId);
            SubscriptionBus.DeleteSubscriptionQueue(typeof(DispatcherOffline), DispatcherRepository.OfflineDispatchersSubscriptionId);
            SubscriptionBus.DeleteSubscriptionQueue(typeof(LifeSignal), DispatcherId.FromString("MonitorintTest1").LifeSignalSubscriptionId);
            SubscriptionBus.DeleteSubscriptionQueue(typeof(LifeSignal), DispatcherId.FromString("MonitoringTest2").LifeSignalSubscriptionId);
        }

        public void ConfigureSupervisorFor(string dispatcherId)
        {
            var dispatcherSettings = ConfigureDispatcherFor(dispatcherId);

            var bootstrapSettings = new BootstrapSettings();
            bootstrapSettings.DispatcherSettings.Add(dispatcherSettings);

            SubscribeToDispatcherOffLineEvent();
            SubscribeToDispatcherOnLineEvent();

            Supervisor.Initialize(bootstrapSettings);
        }

        private static DispatcherSettings ConfigureDispatcherFor(string dispatcherId)
        {
            var dispatcherSettings = new DispatcherSettings
            {
                DispatcherId = DispatcherId.FromString(dispatcherId),
                DispatcherLifeSpan = new DispatcherLifeSpan
                {
                    Mode = DispatcherLifeSpanMode.UntilTimedOut,
                    Timeout = TimeSpan.FromSeconds(30)
                },
                IntervalForCheckingUnfinishedJobs = TimeSpan.FromMilliseconds(800),
                MaximumNumberOfProcessingJobs = 1,
            };
            var jobConfiguration = new JobConfiguration
            {
                JobAndWorkerType =
                {
                    JobType = RuntimeType.From(typeof (SampleJob)),
                    WorkerType = RuntimeType.From(typeof (SampleWorker))
                },
                RequestMaxAttempts = 3,
                RequestTimeout = TimeSpan.FromSeconds(10)
            };
            dispatcherSettings.JobConfigurations.Add(jobConfiguration);
            return dispatcherSettings;
        }

        private void SubscribeToDispatcherOffLineEvent()
        {
            if (!SubscriptionBus.IsListenningTo(typeof(DispatcherOffline), DispatcherRepository.OfflineDispatchersSubscriptionId))
            {
                SubscriptionBus.SubscribeTo<DispatcherOffline>(
                    DispatcherRepository.OfflineDispatchersSubscriptionId,
                    OnWorkDispatcherOfflineReceived
                );
            }
        }

        private void OnWorkDispatcherOfflineReceived(object message)
        {
            var dispatcherId = ((DispatcherOffline)message).DispatcherId;
            if (WorkDispatchersOffline.Any(d => d == dispatcherId))
                return;

            WorkDispatchersOnline.Remove(dispatcherId);
            WorkDispatchersOffline.Add(dispatcherId);
        }

        private void SubscribeToDispatcherOnLineEvent()
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

            WorkDispatchersOffline.Remove(dispatcherId);
            WorkDispatchersOnline.Add(dispatcherId);
        }

        internal void StartSupervisorWithoutServiceRestoration()
        {
            Supervisor.IsComputationalUnitRestorationEnabled = false;
            Supervisor.Start();
        }

        internal void InitializeDispatcher(string dispatcherId)
        {
            var dispatcherSettings = ConfigureDispatcherFor(dispatcherId);

            Launcher.Launch(dispatcherSettings);
        }

        internal bool ReceivedDispatcherOfflineEventFor(string dispatcherId)
        {
            return WorkDispatchersOffline.Any(d => d.Value.Equals(dispatcherId));
        }

        internal bool ReceivedDispatcherOnlineEventFor(string dispatcherId)
        {
            return WorkDispatchersOnline.Any(d => d.Value.Equals(dispatcherId));
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
    }
}
