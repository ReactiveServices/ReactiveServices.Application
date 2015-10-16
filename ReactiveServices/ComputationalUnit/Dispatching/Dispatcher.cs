using NLog;
using ReactiveServices.ComputationalUnit.Settings;
using ReactiveServices.ComputationalUnit.Work;
using ReactiveServices.Configuration;
using ReactiveServices.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReactiveServices.ComputationalUnit.Dispatching
{
    public sealed class Dispatcher : IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public bool IsRunning { get; private set; }

        public DateTime StartupTime { get; set; }

        public ThreadExecutor CheckingThreads = new ThreadExecutor();

        internal DispatcherRepository Repository { get; private set; }
        private Dictionary<WorkerId, IWorker> ExecutingWorkers { get; set; }

        private readonly CancellationTokenSource CancellationTokenForCheckIfDispatcherShouldBeKeptAliveThatMuchTimeAfterInitialized = new CancellationTokenSource();
        private readonly CancellationTokenSource CancellationTokenForCheckingPendingJobs = new CancellationTokenSource();
        private readonly CancellationTokenSource CancellationTokenForExecutionOfJob = new CancellationTokenSource();

        public bool IsDisposed { get; private set; }

        public Dispatcher()
        {
            IsDisposed = true;
            Disposed += delegate { };
            ExecutingWorkers = new Dictionary<WorkerId, IWorker>();
        }

        public void Initialize(DispatcherSettings settings)
        {
            try
            {
                if (!DependencyResolver.IsInitialized)
                    throw new InvalidOperationException("The DependencyResolver must be initialized before initializing the Dispatcher!");

                settings.Validate();

                Repository = new DispatcherRepository(settings);
                Repository.SubscribeToPoisonPill(TakePoisonPill);
                Repository.SubscribeToPendingJobs(ReceiveJob);
                Repository.StartSendingLifeSignal();

                StartCheckingIfDispatcherShouldBeKeptAliveThatMuchTimeAfterInitialized();

                ConfirmInitialization();

                IsDisposed = false;
            }
            catch (Exception e)
            {
                Log.Error(e, "Could not initialize the Dispatcher.");
                throw new InvalidOperationException("Could not initialize the Dispatcher.", e);
            }
        }

        private void ConfirmInitialization()
        {
            StartupTime = DateTime.Now;
            IsRunning = true;
            Repository.PublishLaunchConfirmation();
            Log.DispatcherActivity(Repository.Settings.DispatcherId, DispatcherActivity.DispatcherInitialized);
            WriteRuntimeInfo();
        }

        private void WriteRuntimeInfo()
        {
            Log.WriteRuntimeInfo();
        }

        private void StartCheckingIfDispatcherShouldBeKeptAliveThatMuchTimeAfterInitialized()
        {
            CheckingThreads.Repeat(
                "CheckIfDispatcherShouldBeKeptAliveThatMuchTimeAfterInitialized",
                CheckIfDispatcherShouldBeKeptAliveThatMuchTimeAfterInitialized,
                HandleBackgroundThreadException,
                1000,
                cancellationToken: CancellationTokenForCheckIfDispatcherShouldBeKeptAliveThatMuchTimeAfterInitialized.Token
            );
        }

        private void CheckIfDispatcherShouldBeKeptAliveThatMuchTimeAfterInitialized()
        {
            if (IsTakingPoisonPill)
                return;

            if ((!ShallTerminateAfterTimeout) || (DateTime.Now - StartupTime < Repository.Settings.DispatcherLifeSpan.Timeout))
                return;

            if (HasExecutingWorkers())
            {
                if (DateTime.Now - StartupTime < Repository.Settings.MaximumExecutionTime)
                    return;

                TakePoisonPill();
            }
            else
            {
                TakePoisonPill();
            }
        }

        private bool ShallTerminateAfterTimeout
        {
            get
            {
                return ((Repository.Settings.DispatcherLifeSpan.Mode == DispatcherLifeSpanMode.UntilTimedOut) ||
                        (Repository.Settings.DispatcherLifeSpan.Mode == DispatcherLifeSpanMode.UntilFirstJobIsCompletedOrTimedOut));
            }
        }

        private bool HasExecutingWorkers()
        {
            lock (ExecutingWorkers)
            {
                return ExecutingWorkers.Count > 0;
            }
        }

        private bool IsTakingPoisonPill;

        private void TakePoisonPill()
        {
            TakePoisonPill(new PoisonPill
            {
                DispatcherId = Repository.Settings.DispatcherId,
                EffectOnCurrentWork = PoisonPillEffect.Cancel
            });
        }

        private void TakePoisonPill(PoisonPill poisonPill)
        {
            try
            {
                if (IsDisposed)
                    return;

                Log.DispatcherActivity(Repository.Settings.DispatcherId, DispatcherActivity.PoisonPillWasReceived, poisonPill);

                if (poisonPill.DispatcherId != Repository.Settings.DispatcherId)
                    return;

                // Kill the process imediatelly non-gracefully
                if (poisonPill.EffectOnCurrentWork == PoisonPillEffect.Kill)
                {
                    Log.Error("Poison Pill with Kill request received! Terminating the process now...");
                    Environment.Exit(0);
                }

                // If another poison pill request is already being processed
                if (CancellationTokenForCheckIfDispatcherShouldBeKeptAliveThatMuchTimeAfterInitialized.IsCancellationRequested)
                    return;

                IsTakingPoisonPill = true;

                // Stop checking if can still be alive
                CancellationTokenForCheckIfDispatcherShouldBeKeptAliveThatMuchTimeAfterInitialized.Cancel();

                // Stop receiving pending work requests.
                CancellationTokenForCheckingPendingJobs.Cancel();

                // Wait the execution of the current work requests
                if (poisonPill.EffectOnCurrentWork == PoisonPillEffect.Wait)
                {
                    Thread.CurrentThread.SleepWhile(
                        () => ExecutingWorkers.Count > 0,
                        Repository.Settings.MaximumExecutionTime,
                        TimeoutWaitingWorkerToFinish);
                }

                // Abort the execution of the current work requests
                if (poisonPill.EffectOnCurrentWork == PoisonPillEffect.Abort)
                {
                    CancellationTokenForExecutionOfJob.Cancel();

                    Log.DispatcherActivity(Repository.Settings.DispatcherId, DispatcherActivity.AbortedAllWorkerThreads);
                }

                // Cancel the execution of the current work requests
                if (poisonPill.EffectOnCurrentWork == PoisonPillEffect.Cancel)
                {
                    CancellationTokenForExecutionOfJob.Cancel();

                    Thread.CurrentThread.SleepWhile(
                        () => ExecutingWorkers.Count > 0,
                        Repository.Settings.MaximumExecutionTime,
                        TimeoutWaitingWorkerToFinish);
                }

                Log.DispatcherActivity(Repository.Settings.DispatcherId, DispatcherActivity.PoisonPillWasTaken, poisonPill);

                // Must dispose in another thread to allow the handler that received the poison pill 
                // to acknoledge the message before the connection is disposed
                DisposeAsync();
            }
            catch (ThreadAbortException tae)
            {
                Log.Info(tae as Exception, "ThreadAbortException while taking poison pill!");
            }
            catch (Exception e)
            {
                Log.Error(e, "Exception taking poison pill!");
                Environment.Exit(0);
            }
        }

        private void DisposeAsync()
        {
            Task.Run(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                Dispose();
            });
        }

        private void TimeoutWaitingWorkerToFinish()
        {
            throw new TimeoutException(
                string.Format(
                    "All background threads should have been stopped within {0} seconds. Worker {1} could not be stopped!",
                    Repository.Settings.MaximumExecutionTime.TotalSeconds, ExecutingWorkers.First().Key)
                );
        }

        private void HandleBackgroundThreadException(Exception exception)
        {
            if (exception is ThreadAbortException)
            {
                Log.DispatcherActivity(Repository.Settings.DispatcherId, DispatcherActivity.BackgroundThreadException, null, exception, false);
            }
            else
            {
                Log.DispatcherActivity(Repository.Settings.DispatcherId, DispatcherActivity.BackgroundThreadException, exception: exception);
                if ((!IsTakingPoisonPill) && (!IsDisposed))
                    TakePoisonPill();
            }
        }

        private readonly object ReceivingJobLock = new object();

        private void ReceiveJob(Job job)
        {
            // Since ReceiveJob is called asyncronously, this lock avoid reentrance while processing a received work request
            lock (ReceivingJobLock)
            {
                Log.DispatcherActivity(Repository.Settings.DispatcherId, DispatcherActivity.JobWasReceived, job, job.ExecutionStatus, job.ExecutionStatus);
                DispatchJob(job);
            }
        }

        private void DispatchJob(Job job)
        {
            var configuration = Repository.Settings.JobConfigurations.Single(c => c.JobAndWorkerType.JobType.Type == job.GetType());

            ApplyConfigurationToJob(job, configuration);

            var worker = InitializeWorkerAndJob(job);

            Log.DispatcherActivity(Repository.Settings.DispatcherId, DispatcherActivity.JobWasDispatched, job, job.ExecutionStatus, job.ExecutionStatus);

            ExecuteJob(job, worker);
        }

        private static void ApplyConfigurationToJob(Job job, JobConfiguration configuration)
        {
            job.RequestMaxAttempts = configuration.RequestMaxAttempts;
            job.RequestTimeout = configuration.RequestTimeout;
            job.ProcessingAttempts += 1;

            job.RequestMaxTime = configuration.RequestTimeout == Timeout.InfiniteTimeSpan
                ? Timeout.InfiniteTimeSpan
                : TimeSpan.FromSeconds(configuration.RequestTimeout.TotalSeconds * configuration.RequestMaxAttempts);

            job.WorkerType = configuration.JobAndWorkerType.WorkerType.Type;
        }

        private IWorker InitializeWorkerAndJob(Job job)
        {
            var worker = NewWorkerFor(job);
            Debug.Assert(worker != null, "worker != null");
            job.WorkerId = worker.WorkerId;

            lock (ExecutingWorkers)
            {
                ExecutingWorkers[job.WorkerId] = worker;
            }

            //Repository.SubscribeToWorkerLifeSignal(job.WorkerId);

            return worker;
        }

        private void ExecuteJob(Job job, IWorker worker)
        {
            try
            {
                Log.Debug("Dispatching {0} to {1}", job.GetType().FullName, worker.GetType().FullName);
                worker.Execute(job, ProcessAsCompleted, CancellationTokenForExecutionOfJob);
            }
            finally
            {
                FinalizeWorker(worker);
            }
        }

        private void FinalizeWorker(IWorker worker)
        {
            lock (ExecutingWorkers)
            {
                if (ExecutingWorkers.ContainsKey(worker.WorkerId))
                {
                    var workerId = worker.WorkerId;
                    var job = worker.ExecutingJob;
                    var jobExecutionStatus = job != null ? job.ExecutionStatus : JobStatus.Pending;
                    worker.Dispose();

                    Log.DispatcherActivity(Repository.Settings.DispatcherId, DispatcherActivity.RequestWorkerWasFinalized, job, jobExecutionStatus);

                    ExecutingWorkers.Remove(workerId);

                    CheckIfDispatcherShouldBeTerminatedAfterFirstWorkCompleted();
                }
            }
        }

        private static IWorker NewWorkerFor(Job job)
        {
            var worker = DependencyResolver.Get<Worker>(job.WorkerType);
            return worker;
        }

        private void ProcessAsCompleted(Job job)
        {
            Repository.RegisterCompletionOf(job);

            switch (job.ExecutionStatus)
            {
                case JobStatus.Succeeded:
                    break;
                case JobStatus.Failed:
                    CheckIfShallRepeatOrLogTheExecutionOf(job);
                    break;
                default:
                    Debug.Fail(String.Format("Invalid execution status for work request {0}", job.MessageId));
                    break;
            }
        }

        private void CheckIfDispatcherShouldBeTerminatedAfterFirstWorkCompleted()
        {
            if (IsTakingPoisonPill) return;

            if (ShallTerminateAfterFirstJobCompleted)
                TakePoisonPill();
        }

        private bool ShallTerminateAfterFirstJobCompleted
        {
            get
            {
                return (Repository.Settings.DispatcherLifeSpan.Mode == DispatcherLifeSpanMode.UntilFirstJobIsCompleted) ||
                       (Repository.Settings.DispatcherLifeSpan.Mode == DispatcherLifeSpanMode.UntilFirstJobIsCompletedOrTimedOut);
            }
        }

        private void CheckIfShallRepeatOrLogTheExecutionOf(Job job)
        {
            switch (job.FailureAction)
            {
                case JobFailureAction.Repeat:
                    if (job.HasExceededMaxAttempts())
                        FailJobExecution(job);
                    else
                    {
                        Repository.RepublishAsPending(job);
                    }
                    break;
                case JobFailureAction.Log:
                    FailJobExecution(job);
                    break;
            }
        }

        private void FailJobExecution(Job job)
        {
            if (job.ExecutionException != null)
            {
                Log.DispatcherActivity(
                    Repository.Settings.DispatcherId,
                    DispatcherActivity.BackgroundThreadException,
                    job,
                    JobStatus.Failed,
                    JobStatus.Failed
                );
            }
            Repository.RepublishAsFailed(job);
        }

        public bool CanReceiveRequestsOfType(Type requestType)
        {
            return IsRunning && Repository.Settings.JobConfigurations.Select(c => c.JobAndWorkerType.JobType.Type).Contains(requestType);
        }

        public bool CanDispatchRequestsTo(Type handler)
        {
            return IsRunning && Repository.Settings.JobConfigurations.Select(c => c.JobAndWorkerType.WorkerType.Type).Contains(handler);
        }

        public bool HasExecutingWorkerFor(Job job)
        {
            lock (ExecutingWorkers)
            {
                IWorker worker;
                if (ExecutingWorkers.TryGetValue(job.WorkerId, out worker))
                {
                    return worker.IsExecuting(job);
                }
            }
            return false;
        }

        public bool IsWaitingForCompletionOf(Job job)
        {
            if (job.WorkerId == null)
                return false;

            lock (ExecutingWorkers)
            {
                IWorker worker;
                if (ExecutingWorkers.TryGetValue(job.WorkerId, out worker))
                {
                    return worker.HasCompletionCallback();
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Repository.Settings.DispatcherId.Value.GetHashCode();
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            try
            {
                CancellationTokenForCheckIfDispatcherShouldBeKeptAliveThatMuchTimeAfterInitialized.Cancel();
                CancellationTokenForExecutionOfJob.Cancel();

                var timeToWaitForBackgroundThreads = Repository.Settings.MaximumExecutionTime;
                var finishCheckingThreadsTask = CheckingThreads.WaitAllAsync(timeToWaitForBackgroundThreads);

                finishCheckingThreadsTask.Wait();

                CheckingThreads.Dispose();

                DisposeAnyLeftingExecutingWorkers();

                Repository.Dispose();

                CancellationTokenForCheckIfDispatcherShouldBeKeptAliveThatMuchTimeAfterInitialized.Dispose();
                CancellationTokenForExecutionOfJob.Dispose();
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                Log.Error(e, String.Format("Exception disposing dispatcher of id '{0}'", Repository.Settings.DispatcherId));
                throw;
            }

            Debug.Assert(Disposed != null, "Disposed != null");
            Disposed(this, EventArgs.Empty);
        }

        private void DisposeAnyLeftingExecutingWorkers()
        {
            lock (ExecutingWorkers)
            {
                while (ExecutingWorkers.Count > 0)
                {
                    FinalizeWorker(ExecutingWorkers.First().Value);
                }
            }
        }

        public event EventHandler Disposed;
    }
}
