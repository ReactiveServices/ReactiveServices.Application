using FluentAssertions;
using PostSharp.Patterns.Diagnostics;
using ReactiveServices.ComputationalUnit.Dispatching.Tests;
using ReactiveServices.ComputationalUnit.Settings;
using ReactiveServices.ComputationalUnit.Work;
using ReactiveServices.Configuration;
using ReactiveServices.Extensions;
using ReactiveServices.MessageBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace ReactiveServices.ComputationalUnit.Dispatching.LoadBalancing.Tests.Specifications
{
    sealed class StepsExecutor : IDisposable
    {
        DispatcherSettings Settings { get; set; }
        DispatcherSettings AnotherSettings { get; set; }
        Dispatcher WorkDispatcher { get; set; }
        Dispatcher AnotherWorkDispatcher { get; set; }
        DummyJob DummyJob { get; set; }
        AnotherDummyJob AnotherDummyJob { get; set; }
        ISubscriptionBus SubscriptionBus { get; set; }
        IPublishingBus PublishingBus { get; set; }
        ISendingBus SendingBus { get; set; }
        List<Dispatcher> ListOfDispatchers { get; set; }
        List<DispatcherSettings> ListOfSettingsForMultipleDispatchers { get; set; }
        private int NumberOfDummyJobs { get; set; }
        internal Exception LastException { get; private set; }

        static StepsExecutor()
        {
            AppDomain.CurrentDomain.LogExceptions();
        }

        internal StepsExecutor()
        {
            InitializeDependencies();

            ListOfDispatchers = new List<Dispatcher>();
            ListOfSettingsForMultipleDispatchers = new List<DispatcherSettings>();

            SubscriptionBus = DependencyResolver.Get<ISubscriptionBus>();
            PublishingBus = DependencyResolver.Get<IPublishingBus>();
            SendingBus = DependencyResolver.Get<ISendingBus>();
        }

        private static void InitializeDependencies()
        {
            //Reset the static data before running other scenarios
            DependencyResolver.Reset();
            DependencyResolver.Initialize();
        }

        [Log(AttributeExclude = true)]
        [LogException(AttributeExclude = true)]
        public void Dispose()
        {
            SubscriptionBus.Dispose();
            PublishingBus.Dispose();
            SendingBus.Dispose();

            if (WorkDispatcher != null)
                WorkDispatcher.Dispose();

            if (AnotherWorkDispatcher != null)
                AnotherWorkDispatcher.Dispose();

            FinalizeListOfDispatchers();

            ListOfDispatchers.Clear();
            ListOfSettingsForMultipleDispatchers.Clear();

            DeleteQueuesAndExchanges();
        }

        private static void FinalizeWorkDispatcher(Dispatcher dispatcher)
        {
            if (dispatcher != null)
            {
                dispatcher.Dispose();
            }
        }

        private void FinalizeListOfDispatchers()
        {
            foreach (var dispatcher in ListOfDispatchers)
            {
                FinalizeWorkDispatcher(dispatcher);
            }
        }

        private static void DeleteQueuesAndExchanges()
        {
            using (var bus = DependencyResolver.Get<ISubscriptionBus>())
            {
                bus.DeleteSubscriptionQueue(typeof(DummyJob), DispatcherRepository.PendingJobsSubscriptionId);
                bus.DeleteSubscriptionQueue(typeof(DummyJob), DispatcherRepository.ProcessingJobsSubscriptionId);
                bus.DeleteSubscriptionQueue(typeof(DummyJob), DispatcherRepository.SucceededJobsSubscriptionId);
                bus.DeleteSubscriptionQueue(typeof(DummyJob), DispatcherRepository.FailedJobsSubscriptionId);
                bus.DeleteSubscriptionQueue(typeof(AnotherDummyJob), DispatcherRepository.PendingJobsSubscriptionId);
                bus.DeleteSubscriptionQueue(typeof(AnotherDummyJob), DispatcherRepository.ProcessingJobsSubscriptionId); 
                bus.DeleteSubscriptionQueue(typeof(AnotherDummyJob), DispatcherRepository.SucceededJobsSubscriptionId);
                bus.DeleteSubscriptionQueue(typeof(AnotherDummyJob), DispatcherRepository.FailedJobsSubscriptionId);
                bus.DeleteSubscriptionQueue(typeof(MatrixParsingRequest), DispatcherRepository.PendingJobsSubscriptionId);
                bus.DeleteSubscriptionQueue(typeof(MatrixParsingRequest), DispatcherRepository.ProcessingJobsSubscriptionId); 
                bus.DeleteSubscriptionQueue(typeof(MatrixParsingRequest), DispatcherRepository.SucceededJobsSubscriptionId);
                bus.DeleteSubscriptionQueue(typeof(MatrixParsingRequest), DispatcherRepository.FailedJobsSubscriptionId);
                bus.DeleteSubscriptionQueue(typeof(MatrixReversingRequest), DispatcherRepository.PendingJobsSubscriptionId);
                bus.DeleteSubscriptionQueue(typeof(MatrixReversingRequest), DispatcherRepository.ProcessingJobsSubscriptionId);
                bus.DeleteSubscriptionQueue(typeof(MatrixReversingRequest), DispatcherRepository.SucceededJobsSubscriptionId);
                bus.DeleteSubscriptionQueue(typeof(MatrixReversingRequest), DispatcherRepository.FailedJobsSubscriptionId);
            }
        }

        internal void ConfigureConnectionsForWorkDispatcher()
        {
            Settings = new DispatcherSettings
            {
                DispatcherId = DispatcherId.FromString(String.Format("WorkDispatcher_{0}", Guid.NewGuid())),
                IntervalForCheckingUnfinishedJobs = TimeSpan.FromMilliseconds(800),
                MaximumNumberOfProcessingJobs = 10
            };

            DeleteQueuesAndExchanges();
        }

        internal void ConfigureConnectionsForAnotherWorkDispatcher()
        {
            AnotherSettings = new DispatcherSettings
            {
                DispatcherId = DispatcherId.FromString(String.Format("AnotherWorkDispatcher_{0}", Guid.NewGuid())),
                IntervalForCheckingUnfinishedJobs = TimeSpan.FromMilliseconds(800),
                MaximumNumberOfProcessingJobs = 10
            };

            DeleteQueuesAndExchanges();
        }

        internal void ConfigureConnectionsForMultipleDispatchers(int numberOfDispatchers)
        {
            ConfigureBaseSettingsForMultipleDispatchers(numberOfDispatchers);

            DeleteQueuesAndExchanges();
        }

        internal void ConfigureBaseSettingsForMultipleDispatchers(int numberOfDispatchers)
        {
            for (var i = 0; i < numberOfDispatchers; i++)
            {
                var settings = new DispatcherSettings
                {
                    //DispatcherId = DispatcherId.FromString("Dispatcher" + i.ToString("00")),
                    DispatcherId = DispatcherId.FromString(String.Format("WorkDispatcher_{0}_{1}", i, Guid.NewGuid())),
                    IntervalForCheckingUnfinishedJobs = TimeSpan.FromMilliseconds(800),
                    MaximumNumberOfProcessingJobs = 1
                };

                ListOfSettingsForMultipleDispatchers.Add(settings);
            }
        }

        internal void ConfigureIntervalForCheckingUnfinishedJobsOnAnotherSettingsTo(int someMilliseconds)
        {
            AnotherSettings.IntervalForCheckingUnfinishedJobs = TimeSpan.FromMilliseconds(someMilliseconds);
        }

        internal void InstantiateDummyJob()
        {
            DummyJob = new DummyJob();
        }

        internal void InstantiateWorkDispatcher()
        {
            WorkDispatcher = new Dispatcher();
        }

        internal void InstantiateAnotherWorkDispatcher()
        {
            AnotherWorkDispatcher = new Dispatcher();
        }

        internal void InstantiateAnotherDummyJob()
        {
            AnotherDummyJob = new AnotherDummyJob();
        }

        internal void ConfigureSubscriptionForDummyJobOnWorkDispatcher()
        {
            var configuration = new JobConfiguration
            {
                JobAndWorkerType = { JobType = RuntimeType.From(typeof(DummyJob)), WorkerType = RuntimeType.From(typeof(DummyWorker)) },
                RequestMaxAttempts = 2,
                RequestTimeout = TimeSpan.FromSeconds(10)
            };

            Settings.JobConfigurations.Add(configuration);
        }

        internal void ConfigureSubscriptionForDummyJobOnAnotherWorkDispatcher()
        {
            var configuration = new JobConfiguration
            {
                JobAndWorkerType = { JobType = RuntimeType.From(typeof(DummyJob)), WorkerType = RuntimeType.From(typeof(DummyWorker)) },
                RequestMaxAttempts = 2,
                RequestTimeout = TimeSpan.FromSeconds(10)
            };

            AnotherSettings.JobConfigurations.Add(configuration);
        }

        internal void ConfigureSubscriptionForAnotherDummyJobOnAnotherWorkDispatcher()
        {
            var configuration = new JobConfiguration
            {
                JobAndWorkerType = { JobType = RuntimeType.From(typeof(AnotherDummyJob)), WorkerType = RuntimeType.From(typeof(DummyWorker)) },
                RequestMaxAttempts = 2,
                RequestTimeout = TimeSpan.FromSeconds(10)
            };

            AnotherSettings.JobConfigurations.Add(configuration);
        }

        internal void ConfigureSubscriptionForDummyJobForMultipleDispatchers()
        {
            foreach (var settings in ListOfSettingsForMultipleDispatchers)
            {
                var configuration = new JobConfiguration
                {
                    JobAndWorkerType =
                    {
                        JobType = RuntimeType.From(typeof(DummyJob)),
                        WorkerType = RuntimeType.From(typeof(DummyWorker))
                    },
                    RequestMaxAttempts = 2,
                    RequestTimeout = TimeSpan.FromSeconds(5 * ListOfSettingsForMultipleDispatchers.Count)
                };

                settings.JobConfigurations.Add(configuration);
            }
        }

        internal void InitializeWorkDispatcher()
        {
            try
            {
                Settings.Validate();
                WorkDispatcher.Initialize(Settings);
            }
            catch (Exception e)
            {
                LastException = e;
            }
        }

        internal void InitializeAnotherWorkDispatcher()
        {
            AnotherSettings.Validate();
            AnotherWorkDispatcher.Initialize(AnotherSettings);
        }

        internal void InitializeMultipleDispatchers()
        {
            foreach (var settings in ListOfSettingsForMultipleDispatchers)
            {
                var dispatcher = new Dispatcher();
                ListOfDispatchers.Add(dispatcher);
                settings.Validate();
                dispatcher.Initialize(settings);
            }
        }

        internal void ConfigureRequestTimeoutsOnDummyJobAs(int? requestTimeoutInSeconds, int? requestMaxAttempts)
        {
            Settings.JobConfigurations[0].RequestTimeout = requestTimeoutInSeconds.HasValue ? TimeSpan.FromSeconds(requestTimeoutInSeconds.Value) : Settings.JobConfigurations[0].RequestTimeout;
            Settings.JobConfigurations[0].RequestMaxAttempts = requestMaxAttempts.HasValue ? requestMaxAttempts.Value : Settings.JobConfigurations[0].RequestMaxAttempts;
        }

        internal void ConfigureRequestTimeoutsForDummyJobOnAnotherSettingsAs(int? requestTimeoutInSeconds, int? requestMaxAttempts)
        {
            AnotherSettings.JobConfigurations[0].RequestTimeout = requestTimeoutInSeconds.HasValue ? TimeSpan.FromSeconds(requestTimeoutInSeconds.Value) : AnotherSettings.JobConfigurations[0].RequestTimeout;
            AnotherSettings.JobConfigurations[0].RequestMaxAttempts = requestMaxAttempts.HasValue ? requestMaxAttempts.Value : AnotherSettings.JobConfigurations[0].RequestMaxAttempts;
        }

        internal void ConfigureDummyJobDurationTo(int someSeconds)
        {
            DummyJob.WorkDurationInMilliseconds = someSeconds * 1000;
        }

        internal void ConfigureWaitingTimeForWorkExecutionTo(int someSeconds)
        {
            DummyJob.WaitingTimeForWorkExecutionInMilliseconds = someSeconds * 1000;
        }

        internal void ConfigureMaximumNumberOfProcessingJobsOnSettingsTo(int numberOfProcessingRequests)
        {
            Settings.MaximumNumberOfProcessingJobs = numberOfProcessingRequests;
        }

        internal void PublishDummyJobAndWaitForVerification()
        {
            PublishDummyJob();
            Thread.Sleep(DummyJob.WaitingTimeForWorkExecutionInMilliseconds);
        }

        internal void PublishDummyJob()
        {
            PublishingBus.Publish(JobStatus.Pending.TopicId(), DummyJob, StorageType.NonPersistent);
        }

        internal void PublishAnotherDummyJobAndWaitForVerification()
        {
            PublishAnotherDummyJob();
            Thread.Sleep(AnotherDummyJob.WaitingTimeForWorkExecutionInMilliseconds);
        }

        private void PublishAnotherDummyJob()
        {
            PublishingBus.Publish(JobStatus.Pending.TopicId(), AnotherDummyJob, StorageType.NonPersistent);
        }

        internal void PublishMultipleDummyJobs(int numberOfDummyJobsToPublish)
        {
            NumberOfDummyJobs = numberOfDummyJobsToPublish;
            const int dummyJobDuration = 500;
            const int waitingTimeForDummyJob = 4000;
            for (var i = 0; i < numberOfDummyJobsToPublish; i++)
            {
                var dummyJob = new DummyJob
                {
                    WorkDurationInMilliseconds = dummyJobDuration,
                    WaitingTimeForWorkExecutionInMilliseconds = waitingTimeForDummyJob
                };

                PublishingBus.Publish(JobStatus.Pending.TopicId(), dummyJob, StorageType.NonPersistent);
            }
            Thread.Sleep(numberOfDummyJobsToPublish * (waitingTimeForDummyJob + 1000));
        }

        //internal void DisposeWorkDispatcher()
        //{
        //    Thread.Sleep(1000);
        //    WorkDispatcher.Dispose();
        //}

        //internal void AssertOnWorkDispatcherThatJobHasDispatchedTheDummyJobForTheDummyWorker()
        //{
        //    var containsActivity = WorkDispatcher.Repository.GetDispatcherEvents().Any(e => e.Activity == DispatcherActivity.JobWasDispatched);
        //    containsActivity.Should().BeTrue();
        //}

        //internal void AssertOnWorkDispatcherThatJobHasHasBeenRejected()
        //{
        //    var containsActivity = WorkDispatcher.Repository.GetDispatcherEvents().Any(e => e.Activity == DispatcherActivity.JobWasRejected);
        //    containsActivity.Should().BeTrue();
        //}

        internal void AssertOnWorkDispatcherThatTheDummyJobPassedThrough(DispatcherActivity activity)
        {
            var dispatcherLogs = WorkDispatcher.Repository.GetDispatcherEvents();
            var containsActivity = dispatcherLogs.Any(e => e.Activity == activity);
            containsActivity.Should().BeTrue();
        }

        internal void AssertOnWorkDispatcherThatANumberOfDummyJobsWereCompleted(int numberOfCompletedJobs)
        {
            var activities = WorkDispatcher.Repository.GetDispatcherEvents().Where(e => e.Activity == DispatcherActivity.JobWasCompleted && e.NewStatus == JobStatus.Succeeded);
            activities.Count().Should().Be(numberOfCompletedJobs);
        }

        internal void AssertOnAnotherWorkDispatcherThatTheDummyJobHaveBeenRepublishedAs(JobStatus newStatus)
        {
            var dispatcherLogs = AnotherWorkDispatcher.Repository.GetDispatcherEvents();
            var containsActivity = dispatcherLogs.Any(e => e.Activity == DispatcherActivity.JobWasRepublished && e.NewStatus == newStatus);
            containsActivity.Should().BeTrue();
        }

        internal void AssertOnAnotherWorkDispatcherThatTheDummyJobPassedThrough(DispatcherActivity thisActivity)
        {
            var dispatcherLogs = AnotherWorkDispatcher.Repository.GetDispatcherEvents();
            var containsActivity = dispatcherLogs.Any(e => e.Activity == thisActivity);
            containsActivity.Should().BeTrue();
        }

        internal void AssertOnAnotherWorkDispatcherThatTheDummyJobHasBeenCompletedAsSucceeded()
        {
            var dispatcherLogs = AnotherWorkDispatcher.Repository.GetDispatcherEvents();
            var containsActivity = dispatcherLogs.Any(
                e => e.Activity == DispatcherActivity.JobWasCompleted &&
                     e.NewStatus == JobStatus.Succeeded
            );
            containsActivity.Should().BeTrue();
        }

        internal void AssertOnAnyWorkDispatcherThatTheDummyJobPassedThrough(DispatcherActivity thisActivity)
        {
            var dispatcherLogs = WorkDispatcher.Repository.GetDispatcherEvents();
            var containsActivity = dispatcherLogs.Any(e => e.Activity == thisActivity);
            var anotherDispatcherLogs = AnotherWorkDispatcher.Repository.GetDispatcherEvents();
            var containsAnotherActivity = anotherDispatcherLogs.Any(e => e.Activity == thisActivity);
            (containsActivity && containsAnotherActivity).Should().BeTrue();
        }

        internal void AssertOnASingleWorkDispatcherThatTheDummyJobPassedThrough(DispatcherActivity thisActivity)
        {
            var dispatcherLogs = WorkDispatcher.Repository.GetDispatcherEvents();
            var containsActivity = dispatcherLogs.Any(e => e.Activity == thisActivity);
            var containsAnotherActivity = AnotherWorkDispatcher.Repository.GetDispatcherEvents().Any(e => e.Activity == thisActivity);
            (containsActivity | containsAnotherActivity).Should().BeTrue();
        }

        internal void AssertThatAtLeastTwoOfTheMultipleDispatchersHaveCompletedAJob()
        {
            var completedJobsByDispatcher = (
                from Dispatcher d in ListOfDispatchers
                group d by d
                into g
                select new
                {
                    Dispatcher = g.Key,
                    CompletedJobs = g.Key.Repository.GetDispatcherEvents().Count(
                        e => e.Activity == DispatcherActivity.JobWasCompleted)
                }).ToArray();

            //All jobs must have been completed
            completedJobsByDispatcher.Sum(cjbc => cjbc.CompletedJobs).Should().Be(ListOfDispatchers.Count);

            // At least two dispatchers must have participated
            completedJobsByDispatcher.Count(cjbc => cjbc.CompletedJobs > 0).Should().BeGreaterThan(1);
        }

        public void AssertOnWorkDispatcherThatOneJobHasCompletedBeforeTheSecondHasStarted()
        {
            var dispatcherLogs = WorkDispatcher.Repository.GetDispatcherEvents().ToArray();
            dispatcherLogs.Count(a => a.Activity == DispatcherActivity.JobWasCompleted).Should().Be(2);
            dispatcherLogs.Count(a => a.Activity == DispatcherActivity.JobWasDispatched).Should().Be(2);
            var firstCompletedActivity = dispatcherLogs.First(a => a.Activity == DispatcherActivity.JobWasCompleted);
            var secondDispatchedActivity = dispatcherLogs.Last(a => a.Activity == DispatcherActivity.JobWasDispatched);
            secondDispatchedActivity.TimeStamp.Should().BeAfter(firstCompletedActivity.TimeStamp);
        }
    }
}