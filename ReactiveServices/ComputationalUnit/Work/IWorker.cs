
using System;
using System.Threading;
using ReactiveServices.MessageBus;

namespace ReactiveServices.ComputationalUnit.Work
{
    public class WorkerId : Id<WorkerId>
    {
        //private const string LifeSignalSubscriptionIdPrefix = "LifeSignalSubscriptionForWorker";
        //public SubscriptionId LifeSignalSubscriptionId
        //{
        //    get
        //    {
        //        return SubscriptionId.FromString(String.Format("{0}_{1}", LifeSignalSubscriptionIdPrefix, Value));
        //    }
        //}
    }

    public interface IWorker : IDisposable
    {
        WorkerId WorkerId { get; }
        Job ExecutingJob { get; }
        void Execute(Job job, Action<Job> completionCallback, CancellationTokenSource cancellationTokenSource);
        bool IsExecuting(Job job);
        bool HasCompletionCallback();
    }
}