using NLog;
using ReactiveServices.ComputationalUnit.Settings;
using ReactiveServices.ComputationalUnit.Work;
using ReactiveServices.MessageBus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ReactiveServices.ComputationalUnit.Dispatching;

namespace ReactiveServices.Application
{
    sealed class ComputationalUnitSupervisor : IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private ISubscriptionBus SubscriptionBus { get; set; }
        private IPublishingBus PublishingBus { get; set; }
        private IReceivingBus ReceivingBus { get; set; }
        private List<DispatcherId> AttachedDispatchers { get; set; }
        private List<DispatcherId> OfflineDispatchers { get; set; }
        private readonly Bootstrapper Bootstrapper;
        private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public readonly TimeSpan MaxTimeDispatcherCanBeSilent = TimeSpan.FromSeconds(10);
        public readonly TimeSpan IntervalForCheckingIfDispatcherIsOnline = TimeSpan.FromSeconds(1);

        public bool IsComputationalUnitRestorationEnabled { get; set; }

        internal BootstrapSettings Settings { get; private set; }

        public ComputationalUnitSupervisor(Bootstrapper bootstrapper, ISubscriptionBus subscriptionBus, IPublishingBus publishingBus,
            IReceivingBus receivingBus)
        {
            Debug.Assert(IntervalForCheckingIfDispatcherIsOnline < MaxTimeDispatcherCanBeSilent, "IntervalForCheckingIfDispatcherIsOnline < MaxTimeDispatcherCanBeSilent");

            AttachedDispatchers = new List<DispatcherId>();
            OfflineDispatchers = new List<DispatcherId>();
            Bootstrapper = bootstrapper;
            SubscriptionBus = subscriptionBus;
            PublishingBus = publishingBus;
            ReceivingBus = receivingBus;

            IsComputationalUnitRestorationEnabled = true;
        }

        public void Initialize(BootstrapSettings settings)
        {
            Settings = settings;

            AttachedDispatchers.Clear();
            AttachedDispatchers.AddRange(Settings.DispatcherSettings.Select(d => d.DispatcherId));

            SubscribeToDetachDispatcherRequests();
            SubscribeToDispatcherLifeSignals();
        }

        private void SubscribeToDetachDispatcherRequests()
        {
            foreach (var dispatcherSetting in Settings.DispatcherSettings)
            {
                SubscriptionBus.RemoveSubscription(typeof(DetachDispatcher), dispatcherSetting.DispatcherId.DetachDispatcherSubscriptionId);

                SubscriptionBus.SubscribeTo<DetachDispatcher>(
                    dispatcherSetting.DispatcherId.DetachDispatcherTopicId,
                    dispatcherSetting.DispatcherId.DetachDispatcherSubscriptionId,
                    OnDetachDispatcher
                );
            }
        }

        private void SubscribeToDispatcherLifeSignals()
        {
            foreach (var dispatcherSetting in Settings.DispatcherSettings)
            {
                SubscribeToDispatcherLifeSignal(dispatcherSetting.DispatcherId);
            }
        }

        private void SubscribeToDispatcherLifeSignal(DispatcherId dispatcherId)
        {
            ReceivingBus.RemoveSubscription(typeof(LifeSignal), dispatcherId.LifeSignalSubscriptionId);

            ReceivingBus.Receive<LifeSignal>(dispatcherId.LifeSignalSubscriptionId, message =>
            {
                lock (LatestLifeSignals)
                {
                    var lifeSignal = (LifeSignal)message;
                    if (!lifeSignal.IsExpired())
                    {
                        LatestLifeSignals[dispatcherId] = lifeSignal;
                        Log.Info("Received life signal from dispatcher '{0}'", dispatcherId);
                    }
                }
            });
        }

        /// <summary>
        /// Launch all computational units and then starts the supervisor
        /// </summary>
        public void Bootstrap()
        {
            Bootstrapper.Execute(Settings);
            Start();
        }

        /// <summary>
        /// Starts the supervisor, assuming the computational units are already running
        /// </summary>
        public void Start()
        {
            CheckIfDispatchersAreOnline();
            CkeckOfflineDispatchersToRestart();
            Log.Info("ComputationalUnitSupervisor started!");
        }

        /// <summary>
        /// Stops the supervisor
        /// </summary>
        public void Stop()
        {
            CancellationTokenSource.Cancel();
            Log.Info("ComputationalUnitSupervisor stoped!");
        }

        private void CheckIfDispatchersAreOnline()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        Log.Info("ComputationalUnitSupervisor looping!");

                        Thread.Sleep(IntervalForCheckingIfDispatcherIsOnline);

                        List<DispatcherId> dispatchersToCheck;

                        lock (AttachedDispatchers)
                            dispatchersToCheck = new List<DispatcherId>(AttachedDispatchers);

                        Log.Info("ComputationalUnitSupervisor will check {0} dispatchers!", dispatchersToCheck.Count);

                        foreach (var dispatcherId in dispatchersToCheck)
                            CheckIfDispatcherIsOnline(dispatcherId);

                    }
                    catch (ThreadAbortException)
                    {
                    }
                }
            }, CancellationTokenSource.Token);
        }

        private void OnDetachDispatcher(object detachDispatcher)
        {
            var dispatcherId = ((DetachDispatcher)detachDispatcher).DispatcherId;

            Log.Debug("Received DetachDispatcher request of id {1} for DispatcherId {0}", dispatcherId, ((DetachDispatcher)detachDispatcher).MessageId);

            lock (AttachedDispatchers)
            {
                AttachedDispatchers.Remove(dispatcherId);
                Log.Info("DetachDispatcher message received! DispatcherId: {0}", dispatcherId);
            }

            NotifyDispatcherHasBeenDetached(dispatcherId);
        }

        private void NotifyDispatcherHasBeenDetached(DispatcherId dispatcherId)
        {
            var dispatcherDetached = new DispatcherDetached
            {
                DispatcherId = dispatcherId
            };
            Log.Debug("Publishing DispatcherDetached event of id {1} for DispatcherId {0}", dispatcherId, dispatcherDetached.MessageId);
            PublishingBus.Publish(dispatcherDetached, StorageType.NonPersistent);
        }

        private void CkeckOfflineDispatchersToRestart()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        if (!IsComputationalUnitRestorationEnabled)
                        {
                            Thread.Sleep(1000);
                            continue;
                        }

                        Thread.Sleep(MaxTimeDispatcherCanBeSilent);

                        Queue<DispatcherId> dispatchersToRestart;
                        lock (OfflineDispatchers)
                        {
                            dispatchersToRestart = new Queue<DispatcherId>(OfflineDispatchers);
                            OfflineDispatchers.Clear();
                        }
                        StartDispatchers(dispatchersToRestart);
                    }
                    catch (ThreadAbortException)
                    {
                    }
                }
            }, CancellationTokenSource.Token);
        }

        private void StartDispatchers(Queue<DispatcherId> dispatchersToRestart)
        {
            while (dispatchersToRestart.Count > 0)
            {
                StartDispatcher(dispatchersToRestart.Dequeue());
            }
        }

        private void StartDispatcher(DispatcherId dispatcherId)
        {
            Task.Run(() =>
            {
                var dispatcherSettings = Settings.DispatcherSettings.Single(d => d.DispatcherId == dispatcherId);

                var bootstrapSettings = Settings.Clone();
                bootstrapSettings.BootstrapJobs.Clear();
                bootstrapSettings.DispatcherSettings.Clear();
                bootstrapSettings.DispatcherSettings.Add(dispatcherSettings);

                Bootstrapper.Execute(bootstrapSettings);

                NotifyDispatcherWasRestarted(dispatcherId);
            }, CancellationTokenSource.Token);
        }

        internal void RemoveSubscriptionsToDetachDispatcherRequests()
        {
            Debug.Assert(Settings != null, "Settings != null");
            foreach (var dispatcherSetting in Settings.DispatcherSettings)
            {
                SubscriptionBus.RemoveSubscription(typeof(DetachDispatcher), dispatcherSetting.DispatcherId.DetachDispatcherSubscriptionId);
                SubscriptionBus.DeleteSubscriptionQueue(typeof(DetachDispatcher), dispatcherSetting.DispatcherId.DetachDispatcherSubscriptionId);
            }
        }

        private readonly Dictionary<DispatcherId, LifeSignal> LatestLifeSignals = new Dictionary<DispatcherId, LifeSignal>();

        private void CheckIfDispatcherIsOnline(DispatcherId dispatcherId)
        {
            Log.Info("Checking dispatcher {0}!", dispatcherId);

            var dispatcherOnline = LastLifeSignalIsTooOldToConsiderTheDispatcherIsStillOnline(dispatcherId);
            if (dispatcherOnline)
            {
                RemoveDispatcherFromRestartQueue(dispatcherId);
            }
            else
            {
                AddDispatcherToRestartQueue(dispatcherId);
            }
        }

        private bool LastLifeSignalIsTooOldToConsiderTheDispatcherIsStillOnline(DispatcherId dispatcherId)
        {
            lock (LatestLifeSignals)
            {
                LifeSignal latestLifeSignal;
                if (LatestLifeSignals.TryGetValue(dispatcherId, out latestLifeSignal))
                {
                    return (DateTime.Now - latestLifeSignal.CreationDate) < MaxTimeDispatcherCanBeSilent;
                }
                return false;
            }
        }

        private void AddDispatcherToRestartQueue(DispatcherId dispatcherId)
        {
            lock (OfflineDispatchers)
            {
                if (!OfflineDispatchers.Contains(dispatcherId))
                {
                    OfflineDispatchers.Add(dispatcherId);
                    NotifyDispatcherIsOffline(dispatcherId);
                }
            }
        }

        private void RemoveDispatcherFromRestartQueue(DispatcherId dispatcherId)
        {
            lock (OfflineDispatchers)
            {
                if (OfflineDispatchers.Contains(dispatcherId))
                    OfflineDispatchers.Remove(dispatcherId);

                NotifyDispatcherIsOnline(dispatcherId);
            }
        }

        private void NotifyDispatcherIsOnline(DispatcherId dispatcherId)
        {
            Log.Info("Dispatcher {0} is online!", dispatcherId);
            var dispacherOnlineEvent = new DispatcherOnline { DispatcherId = dispatcherId };
            PublishingBus.Publish(dispacherOnlineEvent, StorageType.NonPersistent);
        }

        private void NotifyDispatcherIsOffline(DispatcherId dispatcherId)
        {
            Log.Info("Dispatcher {0} is offline!", dispatcherId);
            var dispacherOfflineEvent = new DispatcherOffline { DispatcherId = dispatcherId };
            PublishingBus.Publish(dispacherOfflineEvent, StorageType.NonPersistent);
        }

        private void NotifyDispatcherWasRestarted(DispatcherId dispatcherId)
        {
            Log.Info("Dispatcher {0} was restarted!", dispatcherId);
            var dispacherRestartedEvent = new DispatcherRestarted { DispatcherId = dispatcherId };
            PublishingBus.Publish(dispacherRestartedEvent, StorageType.NonPersistent);
        }

        public void Dispose()
        {
            Stop();
            RemoveSubscriptionsToDetachDispatcherRequests();
            Bootstrapper.Dispose();
        }
    }
}
