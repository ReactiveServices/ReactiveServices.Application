using NLog;
using ReactiveServices.ComputationalUnit.Dispatching;
using ReactiveServices.ComputationalUnit.Settings;
using ReactiveServices.MessageBus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using ReactiveServices.ComputationalUnit;
using ReactiveServices.Extensions;

namespace ReactiveServices.Application
{
    public class DispatcherLauncher : IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private DispatcherLauncherId DispatcherLauncherId { get; set; }
        private SubscriptionId LaunchConfirmationSubscriptionId { get; set; }
        private ISubscriptionBus SubscriptionBus { get; set; }
        private List<LaunchRecord> LaunchRecords { get; set; }
        public const string LaunchConfirmationSubscriptionIdPrefix = "LaunchConfirmationSubscriptionFor_";

        public DispatcherLauncher(ISubscriptionBus subscriptionBus)
        {
            DispatcherLauncherId = DispatcherLauncherId.New();
            SubscriptionBus = subscriptionBus;
            LaunchRecords = new List<LaunchRecord>();

            LaunchConfirmationSubscriptionId = SubscriptionId.FromString(
                LaunchConfirmationSubscriptionIdPrefix + DispatcherLauncherId
            );
            SubscribeToLaunchConfirmations();
        }

        public void Launch(DispatcherSettings settings)
        {
            RegisterLaunchRequestFor(settings.DispatcherId);
            LaunchComputationalUnit(settings);
        }

        private string _computationalUnitExeName;
        private string ComputationalUnitExeName
        {
            get
            {
                if (_computationalUnitExeName == null)
                    _computationalUnitExeName = Assembly.GetAssembly(typeof(Program)).GetName().Name + ".exe";

                return _computationalUnitExeName;
            }
        }

        protected virtual void LaunchComputationalUnit(DispatcherSettings settings)
        {
            var dispatcherSettingsFileName = Path.GetTempFileName();
            using (var stream = new FileStream(dispatcherSettingsFileName, FileMode.Create, FileAccess.Write))
            {
                settings.SaveTo(stream);
            }

            var showConsoleWindow = Configuration.ConfigurationFiles.Settings.ReactiveServices.ShowConsoleWindow;

            new Process().Start(
                ComputationalUnitExeName, 
                ComputationalUnitArgumentsFor(dispatcherSettingsFileName), 
                showConsoleWindow,
                settings.RequireAdministratorPriviledges
                );
        }

        private static string ComputationalUnitArgumentsFor(string dispatcherSettingsFileName)
        {
            return String.Format("\"{0}\"", dispatcherSettingsFileName);
        }

        private void SubscribeToLaunchConfirmations()
        {
            SubscriptionBus.SubscribeTo<LaunchConfirmation>(
                LaunchConfirmationSubscriptionId,
                m =>
                {
                    var dispatcherId = ((LaunchConfirmation)m).DispatcherId;
                    RegisterLaunchConfirmationFor(dispatcherId);
                },
                SubscriptionMode.Exclusive
            );
        }

        private void RegisterLaunchRequestFor(DispatcherId dispatcherId)
        {
            lock (LaunchRecords)
            {
                LaunchRecords.Add(new LaunchRecord
                {
                    DispatcherId = dispatcherId,
                    RequestTime = DateTime.Now
                });
            }
        }

        private void RegisterLaunchConfirmationFor(DispatcherId dispatcherId)
        {
            LaunchRecord[] launchRecords;

            lock (LaunchRecords)
            {
                launchRecords = LaunchRecords.ToArray();
            }

            var launchRecord = launchRecords.FirstOrDefault(r => r.DispatcherId == dispatcherId && !r.IsConfirmed);
            if (launchRecord != null)
                launchRecord.ConfirmationTime = DateTime.Now;
        }

        public void WaitForLaunchConfirmations(TimeSpan launchTimeout)
        {
            try
            {
                var sw = new Stopwatch();
                sw.Start();
                while (!HasReceivedLaunchConfirmationFromAllDispatchers())
                {
                    if (sw.Elapsed >= launchTimeout)
                        throw new TimeoutException();

                    Thread.Sleep(10);
                }
                sw.Stop();
            }
            catch (TimeoutException)
            {
                throw new TimeoutException(String.Format("Could not launch all configured work dispatchers within {0} milliseconds!", launchTimeout.TotalMilliseconds));
            }
        }

        public bool HasReceivedLaunchConfirmationFromAllDispatchers()
        {
            LaunchRecord[] launchRecords;

            lock (LaunchRecords)
            {
                launchRecords = LaunchRecords.ToArray();
            }

            return launchRecords.All(launchRecord => launchRecord.IsConfirmed);
        }

        public bool HasReceivedLaunchConfirmationFrom(DispatcherId dispatcherId)
        {
            LaunchRecord[] launchRecords;

            lock (LaunchRecords)
            {
                launchRecords = LaunchRecords.ToArray();
            }

            var launchRecord = launchRecords.SingleOrDefault(r => r.DispatcherId == dispatcherId);
            return launchRecord != null && launchRecord.IsConfirmed;
        }

        public void Dispose()
        {
            SubscriptionBus.Dispose();
        }
    }

    public class InProcessDispatcherLauncher : DispatcherLauncher
    {
        public InProcessDispatcherLauncher(ISubscriptionBus subscriptionBus) 
            : base(subscriptionBus)
        {
        }

        protected override void LaunchComputationalUnit(DispatcherSettings settings)
        {
            var dispatcher = new Dispatcher();
            dispatcher.Initialize(settings);

            /*
             * Atention: 
             * These dispatcher continue to live after the InProcessDispatcherLauncher is disposed
             * They will be disposed only after they receive a poison pill
             */
        }
    }
}
