using System.Diagnostics;
using FluentAssertions;
//using Matrix;
using Newtonsoft.Json;
using PostSharp.Patterns.Diagnostics;
using ReactiveServices.ComputationalUnit.Settings;
using ReactiveServices.ComputationalUnit.Work;
using ReactiveServices.Configuration;
using ReactiveServices.MessageBus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace ReactiveServices.ComputationalUnit.Dispatching.Tests.Specifications
{
    sealed class StepsExecutor : IDisposable
    {
        DispatcherSettings Settings { get; set; }
        DispatcherSettings AnotherSettings { get; set; }
        Dispatcher WorkDispatcher { get; set; }
        Dispatcher AnotherWorkDispatcher { get; set; }
        DummyJob DummyJob { get; set; }
        ISubscriptionBus SubscriptionBus { get; set; }
        IPublishingBus PublishingBus { get; set; }
        ISendingBus SendingBus { get; set; }
        List<Job> CompletedJob { get; set; }
        List<Job> ProcessingJob { get; set; }
        //List<string> ListOfMatrixJsonFiles { get; set; }
        //List<MatrixParsingRequest> ListOfMatrixParsingRequests { get; set; }
        internal Exception LastException { get; private set; }

        internal StepsExecutor()
        {
            InitializeDependencies();

            CompletedJob = new List<Job>();
            ProcessingJob = new List<Job>();
            //ListOfMatrixJsonFiles = new List<string>();
            //ListOfMatrixParsingRequests = new List<MatrixParsingRequest>();

            SubscriptionBus = DependencyResolver.Get<ISubscriptionBus>();
            PublishingBus = DependencyResolver.Get<IPublishingBus>();
            SendingBus = DependencyResolver.Get<ISendingBus>();

            DeleteQueuesExchanges();
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

        private void DeleteQueuesExchanges()
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
                bus.DeleteSubscriptionQueue(typeof(MatrixReversingRequest), DispatcherRepository.PendingJobsSubscriptionId);
                bus.DeleteSubscriptionQueue(typeof(MatrixReversingRequest), DispatcherRepository.ProcessingJobsSubscriptionId);
                bus.DeleteSubscriptionQueue(typeof(MatrixReversingRequest), DispatcherRepository.SucceededJobsSubscriptionId);
                bus.DeleteSubscriptionQueue(typeof(MatrixReversingRequest), DispatcherRepository.FailedJobsSubscriptionId);
                if ((Settings != null) && (Settings.DispatcherId != null))
                    bus.DeleteSubscriptionQueue(typeof(PoisonPill), Settings.DispatcherId.PoisonPillSubscriptionId);
                if ((AnotherSettings != null) && (AnotherSettings.DispatcherId != null))
                    bus.DeleteSubscriptionQueue(typeof(PoisonPill), AnotherSettings.DispatcherId.PoisonPillSubscriptionId);
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
        }

        internal void ConfigureConnectionsForAnotherWorkDispatcher()
        {
            AnotherSettings = new DispatcherSettings
            {
                DispatcherId = DispatcherId.FromString(String.Format("AnotherWorkDispatcher_{0}", Guid.NewGuid())),
                IntervalForCheckingUnfinishedJobs = TimeSpan.FromMilliseconds(800),
                MaximumNumberOfProcessingJobs = 10
            };
        }

        internal void InstantiateWorkDispatcher()
        {
            WorkDispatcher = new Dispatcher();
        }

        internal void InstantiateAnotherWorkDispatcher()
        {
            AnotherWorkDispatcher = new Dispatcher();
        }

        internal void InstantiateDummyJobAndConfigureSubscriptionOnWorkDispatcher()
        {
            DummyJob = new DummyJob();
            ConfigureSubscriptionForDummyJobOnWorkDispatcher();
        }

        internal void InstantiateDummyJob()
        {
            DummyJob = new DummyJob();
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

        //internal void ConfigureSubscriptionForMatrixRequestsForMultipleDispatchers()
        //{
        //    foreach (var settings in ListOfSettingsForMultipleDispatchers)
        //    {
        //        var parsing = new JobConfiguration
        //        {
        //            JobAndWorkerType =
        //            {
        //                JobType = RuntimeType.From(typeof(MatrixParsingRequest)),
        //                WorkerType = RuntimeType.From(typeof(MatrixParsingWorker))
        //            },
        //            RequestMaxAttempts = 3,
        //            RequestTimeout = TimeSpan.FromSeconds(10)
        //        };

        //        settings.JobConfigurations.Add(parsing);

        //        var reversing = new JobConfiguration
        //        {
        //            JobAndWorkerType =
        //            {
        //                JobType = RuntimeType.From(typeof(MatrixReversingRequest)),
        //                WorkerType = RuntimeType.From(typeof(MatrixReversingWorker))
        //            },
        //            RequestMaxAttempts = 3,
        //            RequestTimeout = TimeSpan.FromSeconds(10)
        //        };

        //        settings.JobConfigurations.Add(reversing);

        //        settings.DispatcherLifeSpan = new DispatcherLifeSpan
        //        {
        //            Mode = DispatcherLifeSpanMode.UntilTimedOut,
        //            Timeout = TimeSpan.FromSeconds(30)
        //        };
        //    }
        //}

        internal void ConfigureLifeSpanOnSettings(DispatcherLifeSpanMode lifeSpanMode, int timeoutInSeconds)
        {
            Settings.DispatcherLifeSpan = new DispatcherLifeSpan
            {
                Mode = lifeSpanMode,
                Timeout = TimeSpan.FromSeconds(timeoutInSeconds)
            };
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

        internal void InvalidateSettingsForFirstWorkDispatcher()
        {
            Settings.DispatcherId = null; //Configure dispatcher with invalid configuration
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
            WaitForExecution();
        }

        internal void WaitForExecution()
        {
            Thread.Sleep(DummyJob.WaitingTimeForWorkExecutionInMilliseconds);
        }

        internal void PublishDummyJob()
        {
            PublishingBus.Publish(JobStatus.Pending.TopicId(), DummyJob, StorageType.NonPersistent);
        }

        internal void WaitForVerification(int someSeconds)
        {
            Thread.Sleep(TimeSpan.FromSeconds(someSeconds));
        }

        internal void PublishDummyJobAndWaitForVerificationExpecting(JobStatus expectedJobStatus, JobFailureAction? expectedJobFailureAction = null, int? withDuration = null)
        {
            DummyJob.ExpectedExecutionStatus = expectedJobStatus;
            DummyJob.FailureAction = expectedJobFailureAction ?? DummyJob.FailureAction;
            DummyJob.WorkDurationInMilliseconds = withDuration ?? DummyJob.WorkDurationInMilliseconds;

            PublishingBus.Publish(JobStatus.Pending.TopicId(), DummyJob, StorageType.NonPersistent);

            Thread.Sleep(DummyJob.WaitingTimeForWorkExecutionInMilliseconds);
        }

        internal void PublishPoisonPillToWorkDispatcher(PoisonPillEffect effectiveness)
        {
            var poisonPill = new PoisonPill { EffectOnCurrentWork = effectiveness, DispatcherId = Settings.DispatcherId };

            SendingBus.Send(poisonPill, Settings.DispatcherId.PoisonPillSubscriptionId, StorageType.NonPersistent);

            switch (effectiveness)
            {
                case PoisonPillEffect.Abort:
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    break;

                case PoisonPillEffect.Cancel:
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                    break;

                case PoisonPillEffect.Wait:
                    Thread.Sleep(Settings.MaximumExecutionTime);
                    break;
            }
        }

        internal void PublishPoisonPillToAnotherWorkDispatcher(PoisonPillEffect effectiveness)
        {
            var poisonPill = new PoisonPill { EffectOnCurrentWork = effectiveness, DispatcherId = AnotherSettings.DispatcherId };

            SendingBus.Send(poisonPill, AnotherSettings.DispatcherId.PoisonPillSubscriptionId, StorageType.NonPersistent);

            switch (effectiveness)
            {
                case PoisonPillEffect.Cancel:
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                    break;

                case PoisonPillEffect.Wait:
                    Thread.Sleep(Settings.MaximumExecutionTime);
                    break;
            }
        }

        internal void AssertThatWorkDispatcherCommittedSuicide()
        {
            WorkDispatcher.IsDisposed.Should().BeTrue();
        }

        internal void AssertThatWorkDispatcherIsAlive()
        {
            WorkDispatcher.IsDisposed.Should().BeFalse();
        }

        internal void AssertThatAnotherWorkDispatcherIsAlive()
        {
            AnotherWorkDispatcher.IsDisposed.Should().BeFalse();
        }

        internal void AssertThatWorkDispatcherHasValidSettings()
        {
            WorkDispatcher.IsRunning.Should().BeTrue();
        }

        internal void AssertThatWorkDispatcherCanReceiveDummyJobs()
        {
            WorkDispatcher.CanReceiveRequestsOfType(Settings.JobConfigurations[0].JobAndWorkerType.JobType.Type).Should().BeTrue();
        }

        internal void AssertThatWorkDispatcherHasInvalidSettings()
        {
            WorkDispatcher.IsRunning.Should().BeFalse();
        }

        internal void AssertThatWorkDispatcherCannotReceiveDummyJobs()
        {
            WorkDispatcher.CanReceiveRequestsOfType(Settings.JobConfigurations[0].JobAndWorkerType.JobType.Type).Should().BeFalse();
        }

        internal void AssertThatWorkDispatcherCouldNotBeInitializedDueToInvalidSettings()
        {
            LastException.Should().NotBeNull();
            LastException.Message.Should().BeEquivalentTo("DispatcherId cannot be null");
        }

        internal void AssertThatWorkDispatcherCanDispatchDummyJobs()
        {
            WorkDispatcher.CanDispatchRequestsTo(Settings.JobConfigurations[0].JobAndWorkerType.WorkerType.Type).Should().BeTrue();
        }

        internal void AssertOnWorkDispatcherThatRequestTimeoutsForDummyJobAreCorrect()
        {
            Thread.Sleep(1000);

            var dispatcherLog = WorkDispatcher.Repository.GetDispatcherEvents();
            var activity = dispatcherLog.SingleOrDefault(e => e.Activity == DispatcherActivity.JobWasDispatched);
            activity.Should().NotBeNull();
            Debug.Assert(activity != null, "activity != null");
            activity.Job.RequestTimeout.TotalSeconds.Should().Be(Settings.JobConfigurations[0].RequestTimeout.TotalSeconds);
            activity.Job.RequestMaxTime.TotalSeconds.Should().Be(Settings.JobConfigurations[0].RequestTimeout.TotalSeconds * Settings.JobConfigurations[0].RequestMaxAttempts);
        }

        internal void AssertOnWorkDispatcherThatJobIsWaitingForTheCompletionOfTheDummyJob()
        {
            var dispatcherLog = WorkDispatcher.Repository.GetDispatcherEvents();
            var activity = dispatcherLog.SingleOrDefault(e => e.Activity == DispatcherActivity.JobWasDispatched);
            activity.Should().NotBeNull();
            Debug.Assert(activity != null, "activity != null");
            WorkDispatcher.IsWaitingForCompletionOf(activity.Job).Should().BeTrue();
        }

        internal void AssertOnWorkDispatcherThatJobHasDispatchedTheDummyJobForTheDummyWorker()
        {
            var containsActivity = WorkDispatcher.Repository.GetDispatcherEvents().Any(e => e.Activity == DispatcherActivity.JobWasDispatched);
            containsActivity.Should().BeTrue();
        }

        //internal void AssertOnWorkDispatcherThatTheWorkDispatcherHaveKeptTheDummyJobAsProcessing()
        //{
        //    ProcessingJob.Any(wr => wr.MessageId == DummyJob.MessageId).Should().BeTrue("Job must be published in the Processing queue!");
        //    var containsActivity = WorkDispatcher.Repository.GetDispatcherEvents().Any(e => e.Activity == DispatcherActivity.WorkStillInProgress);
        //    containsActivity.Should().BeTrue("Job must have a log entry indicating it is being processed!");
        //}

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

        internal void AssertOnAnotherWorkDispatcherThatTheDummyJobHaveNotPassedThrough(DispatcherActivity thisActivity)
        {
            var dispatcherLogs = AnotherWorkDispatcher.Repository.GetDispatcherEvents();
            var containsActivity = dispatcherLogs.Any(e => e.Activity == thisActivity);
            containsActivity.Should().BeFalse();
        }

        //internal void AssertThatDummyJobValueHasChangedDuringExecution()
        //{
        //    Thread.Sleep(1000);
        //    DummyJob.ValueChangedDuringRequestExecution.Should().HaveValue();
        //}

        internal void AssertThatDummyJobHasBeenCompletedSuccessfuly()
        {
            DummyJob.ExecutionStatus.Should().Be(JobStatus.Succeeded);
        }

        //internal void AssertThatDummyJobValueHasNotChangedDuringExecution()
        //{
        //    DummyJob.ValueChangedDuringRequestExecution.Should().NotHaveValue();
        //}

        //internal void AssertThatDummyJobWasNotCompleted()
        //{
        //    new[] { JobStatus.Pending, JobStatus.Processing }.Should().Contain(DummyJob.ExecutionStatus);
        //}

        internal void AssertOnWorkDispatcherThatTheCompletionEventsHaveBeenPublishedByDummyJob()
        {
            CompletedJob.Any(e => e.WorkerId == DummyJob.WorkerId).Should().BeTrue();
        }

        //internal void AssertOnWorkDispatcherThatNoCompletionEventsHaveBeenPublishedByDummyJob()
        //{
        //    CompletedJob.Any(e => e.MessageId == DummyJob.MessageId).Should().BeFalse();
        //}

        internal void AssertOnWorkDispatcherThatThePoisonPillWasReceivedWithEffectiveness(PoisonPillEffect effect)
        {
            var containsActivity = WorkDispatcher.Repository.GetDispatcherEvents().Any(e => e.Activity == DispatcherActivity.PoisonPillWasReceived && e.PoisonPill.EffectOnCurrentWork == effect);
            containsActivity.Should().BeTrue();
        }

        internal void SubscribeToDummyJobCompletionEvents()
        {
            SubscriptionBus.SubscribeTo<DummyJob>(
                JobStatus.Processing.TopicId(),
                DispatcherRepository.ProcessingJobsSubscriptionId,
                OnDummyJobProcessing
            );
            SubscriptionBus.SubscribeTo<DummyJob>(
                JobStatus.Succeeded.TopicId(),
                DispatcherRepository.SucceededJobsSubscriptionId,
                OnDummyJobSucceeded
            );
            SubscriptionBus.SubscribeTo<DummyJob>(
                JobStatus.Failed.TopicId(),
                DispatcherRepository.FailedJobsSubscriptionId,
                OnDummyJobFailed
            );
        }

        private void OnDummyJobProcessing(object message)
        {
            DummyJob = (DummyJob)message;
            if (DummyJob.ExecutionStatus == JobStatus.Processing)
                ProcessingJob.Add(DummyJob);
        }

        private void OnDummyJobSucceeded(object message)
        {
            DummyJob = (DummyJob)message;
            if (DummyJob.ExecutionStatus == JobStatus.Succeeded)
                CompletedJob.Add(DummyJob);
        }

        private void OnDummyJobFailed(object message)
        {
            DummyJob = (DummyJob)message;
            if (DummyJob.ExecutionStatus == JobStatus.Failed)
                CompletedJob.Add(DummyJob);
        }

        private const int NumberOfMatrices = 5;

        //internal void CreateSerializedMatricesIfNecessary()
        //{
        //    for (var i = 1; i <= NumberOfMatrices; i++)
        //    {
        //        var folderName = Environment.CurrentDirectory + @"\Resources\";
        //        if (!Directory.Exists(folderName))
        //            Directory.CreateDirectory(folderName);

        //        var fileName = String.Format("{0}Matrix{1}.json", folderName, i);
        //        if (!File.Exists(fileName))
        //        {
        //            var random = new Random();
        //            var z = CreateMatrix(30, random);

        //            using (var sw = new StreamWriter(fileName))
        //            {
        //                var serializer = new JsonSerializer();
        //                serializer.Serialize(sw, z);

        //                sw.Flush();
        //            }
        //        }
        //    }
        //}

        //private static ComplexMatrix CreateMatrix(int dimension, Random random)
        //{
        //    // Cria matrix aleatória
        //    var matrixBuilder = new ComplexMatrixBuilder();
        //    for (var i = 0; i < dimension; i++)
        //    {
        //        var line = new Complex[dimension];
        //        for (var j = 0; j < dimension; j++)
        //        {
        //            Thread.Sleep(1);
        //            line[j] = new Complex(random.NextDouble(), random.NextDouble());
        //        }
        //        matrixBuilder = matrixBuilder.WithLine(line);
        //    }
        //    return matrixBuilder.Build();
        //}

        //internal void SubscribeToMatrixReversedEvent()
        //{
        //    SubscriptionBus.SubscribeTo<MatrixReversingRequest>(
        //        JobStatus.Succeeded.TopicId(),
        //        DispatcherRepository.JobSucceededSubscriptionId,
        //        OnMatrixReversedEventReceived
        //    );
        //}

        //private void OnMatrixReversedEventReceived(IMessage message)
        //{
        //    CompletedJob.Add(message as Job);
        //}

        //internal void LoadMatrixJsonFiles()
        //{
        //    for (var i = 1; i <= NumberOfMatrices; i++)
        //    {
        //        var jsonContents = string.Empty;
        //        var jsonFile = Environment.CurrentDirectory + @"\Resources\Matrix" + i + ".json";

        //        if (File.Exists(jsonFile))
        //        {
        //            using (var reader = new StreamReader(jsonFile))
        //            {
        //                jsonContents = reader.ReadToEnd();
        //            }
        //        }

        //        ListOfMatrixJsonFiles.Add(jsonContents);
        //    }
        //}

        //internal void CreateMatrixParsingRequests()
        //{
        //    foreach (var jsonMatrix in ListOfMatrixJsonFiles)
        //    {
        //        var parsingRequest = new MatrixParsingRequest { SerializedMatrix = jsonMatrix };

        //        ListOfMatrixParsingRequests.Add(parsingRequest);
        //    }
        //}

        //internal void PublishMatrixParsingRequests()
        //{
        //    foreach (var parsingRequest in ListOfMatrixParsingRequests)
        //    {
        //        PublishingBus.Publish(JobStatus.Pending.TopicId(), parsingRequest, StorageType.NonPersistent);
        //    }
        //}

        //internal void AssertAllMatrixReversingResultsAreCorrect()
        //{
        //    foreach (var completedJob in CompletedJob)
        //    {
        //        AssertThatMatrixReversingResultIsCorrect(completedJob as MatrixReversingRequest);
        //    }
        //}

        //private static void AssertThatMatrixReversingResultIsCorrect(MatrixReversingRequest reversingRequest)
        //{
        //    var a = ComplexMatrix.FromTuplesOfDoubles(reversingRequest.ParsingRequest.ParsedMatrix);
        //    var b = ComplexMatrix.FromTuplesOfDoubles(reversingRequest.ReversedMatrix);
        //    var i = a.MultiplyBy(b);
        //    i.Equal(ComplexMatrix.Identity(i.Values.ColumnCount), 0.001);
        //}

        //internal void AssertAllMatrixParsingResultedInAWorkCompletedEvent()
        //{
        //    // Asserta que tem 5 diferentes resultados
        //    CompletedJob.Distinct().Should().HaveCount(5);
        //}

        //internal void AssertAllWorkCompletedEventsContainAMatrixReversingRequest()
        //{
        //    // Asserta que tem 5 diferentes MatrixReversingRequest dentre os resultados
        //    CompletedJob.Where(wc => (wc as MatrixReversingRequest) != null).Distinct().Should().HaveCount(5);
        //}

        //internal void AssertAllMatrixReversingRequestsContainAMatrixParsingRequest()
        //{
        //    // Asserta que tem 5 diferentes MatrixParsingRequest dentre os resultados
        //    CompletedJob.Where(wc => ((MatrixReversingRequest)wc).ParsingRequest != null).Distinct().Should().HaveCount(5);
        //}

        internal void WaitForReceivedJobCompletionEvents(int tries, int interval)
        {
            for (var i = 0; i < tries; i++)
            {
                if (CompletedJob.Count < NumberOfMatrices)
                    WaitForVerification(interval);
            }
        }
    }
}