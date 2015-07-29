using NLog;
using ReactiveServices.ComputationalUnit.Work;
using System;
using System.Threading;

namespace ReactiveServices.ComputationalUnit.Dispatching.Tests
{
    public class DummyWorker : Worker
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private DummyJob Job { get { return ExecutingJob as DummyJob; } }

        protected override bool TryExecute()
        {
            return ExecuteSomeDummyWork(Job.WorkDurationInMilliseconds);
        }

        protected override void Complete()
        {
        }
        protected override void Timeout()
        {
        }
        protected override void Fail()
        {
        }

        private bool ExecuteSomeDummyWork(int executionDuration)
        {
            var executionCompleted = true;
            Log.Debug("Worker {0} -> Execution simulation of request {1} started at {2} with duration of {3}", WorkerId, Job.MessageId, DateTime.Now, executionDuration);
            try
            {
                var startTime = DateTime.Now;
                while ((DateTime.Now - startTime).TotalMilliseconds < executionDuration)
                {
                    Thread.Sleep(100);
                    if (IsCancellationRequested)
                    {
                        executionCompleted = false;
                        break;
                    }
                    if (Job.ExpectedExecutionStatus == JobStatus.Failed)
                        throw new Exception("Exception on DummyJob!");
                }
                Job.ValueChangedDuringRequestExecution = new Random().Next();
            }
            finally
            {
                Log.Debug(
                    executionCompleted
                        ? "Worker {0} -> Execution simulation of request {1} ended at {2} with duration of {3}"
                        : "Worker {0} -> Execution simulation of request {1} cancelled at {2} with duration of {3}", WorkerId, Job.MessageId, DateTime.Now,
                    executionDuration);
            }
            return executionCompleted;
        }
    }
}
