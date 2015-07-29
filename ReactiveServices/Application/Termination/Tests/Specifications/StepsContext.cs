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

namespace ReactiveServices.Application.Termination.Tests.Specifications
{
    public sealed class StepsContext : IDisposable
    {
        internal readonly string DispatcherId = "Single";
        internal readonly BootstrapSettings BootstrapSettings;
        internal readonly DispatcherLifeSpan DispatcherLifeSpan;
        internal readonly ISendingBus SendingBus;
        internal readonly IReceivingBus ReceivingBus;
        internal readonly Bootstrapper Bootstrapper;
        internal readonly Dictionary<string, LifeSignal> LatestLifeSignals;
        internal readonly TimeSpan MaxTimeDispatcherCanBeSilent;

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
            SendingBus = DependencyResolver.Get<ISendingBus>();
            ReceivingBus = DependencyResolver.Get<IReceivingBus>();
            Bootstrapper = DependencyResolver.Get<Bootstrapper>();
            LatestLifeSignals = new Dictionary<string, LifeSignal>();
            MaxTimeDispatcherCanBeSilent = TimeSpan.FromSeconds(5);
            ClearAmqpResources();
        }

        public void Dispose()
        {
            ClearAmqpResources();
            Bootstrapper.Dispose();
            SendingBus.Dispose();
            ReceivingBus.Dispose();
        }

        private void ClearAmqpResources()
        {
            ReceivingBus.DeleteSubscriptionQueue(typeof(SampleJob), DispatcherRepository.PendingJobsSubscriptionId);
            ReceivingBus.DeleteSubscriptionQueue(typeof(DispatcherOnline), DispatcherRepository.OnlineDispatchersSubscriptionId);
            ReceivingBus.DeleteSubscriptionQueue(typeof(DispatcherOffline), DispatcherRepository.OfflineDispatchersSubscriptionId);
            ReceivingBus.DeleteSubscriptionQueue(typeof(LifeSignal), ComputationalUnit.Settings.DispatcherId.FromString("Single").LifeSignalSubscriptionId);
            ReceivingBus.DeleteSubscriptionQueue(typeof(LifeSignal), ComputationalUnit.Settings.DispatcherId.FromString("A").LifeSignalSubscriptionId);
            ReceivingBus.DeleteSubscriptionQueue(typeof(LifeSignal), ComputationalUnit.Settings.DispatcherId.FromString("B").LifeSignalSubscriptionId);
        }

        internal RuntimeType JobTypeOfCode(string jobTypeCode)
        {
            switch (jobTypeCode)
            {
                case "Single":
                case "A": return RuntimeType.From(typeof(SampleJob));
                case "B": return RuntimeType.From(typeof(AnotherSampleJob));
                default: return null;
            }
        }

        internal RuntimeType WorkerTypeOfCode(string jobTypeCode)
        {
            switch (jobTypeCode)
            {
                case "Single":
                case "A": return RuntimeType.From(typeof(SampleWorker));
                case "B": return RuntimeType.From(typeof(AnotherSampleWorker));
                default: return null;
            }
        }

        internal JobAndWorkerType JobAndWorkerTypeOfCode(string jobTypeCode)
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

        internal string CreateBootstrapSettingsFile()
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

        internal void ExecuteBootstrapScript()
        {
            var filePath = CreateBootstrapSettingsFile();
            Bootstrapper.Execute(filePath);

            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        internal void TerminateApplication()
        {
            ReceivingBus.Receive<LifeSignal>(ComputationalUnit.Settings.DispatcherId.FromString(DispatcherId).LifeSignalSubscriptionId, message =>
            {
                lock (LatestLifeSignals)
                {
                    LatestLifeSignals[DispatcherId] = (LifeSignal)message;
                }
            });

            var settings = BootstrapSettings.DispatcherSettings.Single(s => s.DispatcherId.Value.StartsWith(DispatcherId));

            lock (LatestLifeSignals)
            {
                LifeSignal latestLifeSignal;
                if (LatestLifeSignals.TryGetValue(settings.DispatcherId.Value, out latestLifeSignal))
                {
                    if ((DateTime.Now - latestLifeSignal.CreationDate) >= MaxTimeDispatcherCanBeSilent)
                        throw new Exception("Dispatcher is already stopped.");
                }
                LatestLifeSignals[settings.DispatcherId.Value] = new LifeSignal
                {
                    SourceId = ComputationalUnit.Settings.DispatcherId.FromString("FirstChanceForLifeSignalNotReceived")
                };
            }

            var poisonPill = new PoisonPill
            {
                EffectOnCurrentWork = PoisonPillEffect.Cancel,
                DispatcherId = settings.DispatcherId
            };
            SendingBus.Send(poisonPill, settings.DispatcherId.PoisonPillSubscriptionId, StorageType.NonPersistent);
        }

        internal void WaitForApplicationTermination()
        {
            Thread.Sleep(DispatcherLifeSpan.Timeout);
        }

        internal bool AllDispatchersHaveTerminated()
        {
            var settings = BootstrapSettings.DispatcherSettings.Single(s => s.DispatcherId.Value.StartsWith(DispatcherId));
            var isStillRunning = true;
            lock (LatestLifeSignals)
            {
                LifeSignal latestLifeSignal;
                if (LatestLifeSignals.TryGetValue(settings.DispatcherId.Value, out latestLifeSignal))
                {
                    if ((DateTime.Now - latestLifeSignal.CreationDate) >= MaxTimeDispatcherCanBeSilent)
                        isStillRunning = false;
                }
                LatestLifeSignals[settings.DispatcherId.Value] = new LifeSignal
                {
                    SourceId = ComputationalUnit.Settings.DispatcherId.FromString("FirstChanceForLifeSignalNotReceived")
                };
            }

            return !isStillRunning;
        }
    }
}
