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
using System.Threading;

namespace ReactiveServices.ComputationalUnit.Dispatching.Work.Tests.Specifications
{
    sealed class StepsExecutor : IDisposable
    {
        DispatcherSettings Settings { get; set; }
        Dispatcher WorkDispatcher { get; set; }
        Dispatcher AnotherWorkDispatcher { get; set; }
        DummyJob DummyJob { get; set; }
        ISubscriptionBus SubscriptionBus { get; set; }
        IPublishingBus PublishingBus { get; set; }
        ISendingBus SendingBus { get; set; }
        List<Job> ProcessingJob { get; set; }
        internal Exception LastException { get; private set; }

        static StepsExecutor()
        {
            AppDomain.CurrentDomain.LogExceptions();
        }

        internal StepsExecutor()
        {
            InitializeDependencies();

            ProcessingJob = new List<Job>();

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
            if (SubscriptionBus != null)
                SubscriptionBus.Dispose();

            if (PublishingBus != null)
                PublishingBus.Dispose();

            if (SendingBus != null)
                SendingBus.Dispose();

            if (WorkDispatcher != null)
                WorkDispatcher.Dispose();

            if (AnotherWorkDispatcher != null)
                AnotherWorkDispatcher.Dispose();

            DeleteQueuesExchanges();
        }

        private static void DeleteQueuesExchanges()
        {
            using (var bus = DependencyResolver.Get<IPublishingBus>())
            {
                bus.DeletePublishingExchange(typeof(LaunchConfirmation));
                bus.DeletePublishingExchange(typeof(DummyJob));
            }

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
                bus.DeleteSubscriptionQueue(typeof(MatrixReversingRequest), DispatcherRepository.ProcessingJobsSubscriptionId); 
                bus.DeleteSubscriptionQueue(typeof(MatrixReversingRequest), DispatcherRepository.PendingJobsSubscriptionId);
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
                MaximumNumberOfProcessingJobs = 1
            };

            DeleteQueuesExchanges();
        }

        internal void InstantiateWorkDispatcher()
        {
            WorkDispatcher = new Dispatcher();
        }

        internal void InstantiateDummyJobAndConfigureSubscriptionOnWorkDispatcher()
        {
            DummyJob = new DummyJob();
            ConfigureSubscriptionForDummyJobOnWorkDispatcher();
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

        internal void ConfigureRequestTimeoutsOnDummyJobAs(int? requestTimeoutInSeconds, int? requestMaxAttempts)
        {
            Settings.JobConfigurations[0].RequestTimeout = requestTimeoutInSeconds.HasValue ? TimeSpan.FromSeconds(requestTimeoutInSeconds.Value) : Settings.JobConfigurations[0].RequestTimeout;
            Settings.JobConfigurations[0].RequestMaxAttempts = requestMaxAttempts ?? Settings.JobConfigurations[0].RequestMaxAttempts;
        }

        internal void ConfigureDummyJobDurationTo(int someSeconds)
        {
            DummyJob.WorkDurationInMilliseconds = someSeconds * 1000;
        }

        internal void ConfigureWaitingTimeForWorkExecutionTo(int someSeconds)
        {
            DummyJob.WaitingTimeForWorkExecutionInMilliseconds = someSeconds * 1000;
        }

        internal void ConfigureIntervalForCheckingUnfinishedJobsOnSettingsTo(int someMilliseconds)
        {
            Settings.IntervalForCheckingUnfinishedJobs = TimeSpan.FromMilliseconds(someMilliseconds);
        }

        internal void SetFailureActionForDummyJobTo(JobFailureAction thisFailureAction)
        {
            DummyJob.FailureAction = thisFailureAction;
        }

        internal void SetExpectedExecutionStatusForDummyJobTo(JobStatus jobStatus)
        {
            DummyJob.ExpectedExecutionStatus = jobStatus;
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

        internal void AssertOnWorkDispatcherThatTheDummyJobPassedThrough(DispatcherActivity activity)
        {
            var dispatcherLogs = WorkDispatcher.Repository.GetDispatcherEvents();
            var containsActivity = dispatcherLogs.Any(e => e.Activity == activity);
            containsActivity.Should().BeTrue();
        }

        internal void AssertOnWorkDispatcherThatTheDummyJobHaveBeenRepublishedAs(JobStatus newStatus)
        {
            var dispatcherLogs = WorkDispatcher.Repository.GetDispatcherEvents();
            var containsActivity = dispatcherLogs.Any(e => e.Activity == DispatcherActivity.JobWasRepublished && e.NewStatus == newStatus);
            containsActivity.Should().BeTrue();
        }

        internal void AssertOnWorkDispatcherThatTheDummyJobHaveNotBeenRepublishedAs(JobStatus newStatus)
        {
            var dispatcherLogs = WorkDispatcher.Repository.GetDispatcherEvents();
            var containsActivity = dispatcherLogs.Any(e => e.Activity == DispatcherActivity.JobWasRepublished && e.NewStatus == newStatus);
            containsActivity.Should().BeFalse();
        }

        internal void SubscribeToDummyJobCompletionEvents()
        {
            SubscriptionBus.SubscribeTo<DummyJob>(
                JobStatus.Processing.TopicId(),
                DispatcherRepository.ProcessingJobsSubscriptionId,
                OnDummyJobProcessing
            );
        }

        private void OnDummyJobProcessing(object message)
        {
            DummyJob = (DummyJob)message;
            if (DummyJob.ExecutionStatus == JobStatus.Processing)
                ProcessingJob.Add(DummyJob);
        }
    }
}