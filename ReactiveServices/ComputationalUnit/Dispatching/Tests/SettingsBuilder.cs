using System;
using ReactiveServices.ComputationalUnit.Settings;

namespace ReactiveServices.ComputationalUnit.Dispatching.Tests
{
    public static class SettingsBuilder
    {
        public static DispatcherSettings Build()
        {
            var settings = new DispatcherSettings();

            var configuration = new JobConfiguration
            {
                JobAndWorkerType =
                {
                    JobType = RuntimeType.From(typeof (DummyJob)),
                    WorkerType = RuntimeType.From(typeof (DummyWorker))
                },
                RequestMaxAttempts = 3,
                RequestTimeout = TimeSpan.FromSeconds(10)
            };

            settings.JobConfigurations.Add(configuration);

            settings.IntervalForCheckingUnfinishedJobs = TimeSpan.FromMilliseconds(800);

            return settings;
        }
    }
}
