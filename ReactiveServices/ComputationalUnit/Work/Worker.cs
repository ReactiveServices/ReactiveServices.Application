using NLog;
using ReactiveServices.MessageBus;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ReactiveServices.Configuration;
using ReactiveServices.Extensions;

namespace ReactiveServices.ComputationalUnit.Work
{
    /// <summary>
    /// Classe base para implementação de Workers.
    /// </summary>
    /// <remarks>
    /// Implemente os métodos TryExecute, Complete e Fail e poderá usar seu Worker dentro das unidades computacionais do framework Reactive Services
    /// </remarks>
    public abstract class Worker : IWorker
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        protected ISubscriptionBus SubscriptionBus { get; private set; }
        protected IPublishingBus PublishingBus { get; private set; }
        protected IRequestBus RequestBus { get; private set; }
        protected IResponseBus ResponseBus { get; private set; }
        protected ISendingBus SendingBus { get; private set; }
        protected IReceivingBus ReceivingBus { get; private set; }

        protected Worker()
        {
            SubscriptionBus = DependencyResolver.Get<ISubscriptionBus>();
            PublishingBus = DependencyResolver.Get<IPublishingBus>();
            RequestBus = DependencyResolver.Get<IRequestBus>();
            ResponseBus = DependencyResolver.Get<IResponseBus>();
            SendingBus = DependencyResolver.Get<ISendingBus>();
            ReceivingBus = DependencyResolver.Get<IReceivingBus>();

            WorkerId = WorkerId.New();
            ExecutingJob = null;
            CompletionCallback = null;
        }

        public WorkerId WorkerId { get; private set; }

        public Job ExecutingJob { get; private set; }
        protected CancellationTokenSource CancellationTokenSource { get; private set; }
        private Action<Job> CompletionCallback { get; set; }

        public void Execute(Job job, Action<Job> completionCallback, CancellationTokenSource cancellationTokenSource)
        {
            ExecutingJob = job;
            CompletionCallback = completionCallback;
            CancellationTokenSource = cancellationTokenSource;

            bool executionSucceeded;
            var hasTimedOut = false;
            var succeeded = false;

            try
            {
                ExecutingJob.ExecutionStatus = JobStatus.Processing;

                PublishingBus.Publish(JobStatus.Processing.TopicId(), ExecutingJob, StorageType.NonPersistent);

                // Tenta executar o trabalho
                executionSucceeded = TryExecuteWithTimeout();

                // Após executar, checa se não demorou mais tempo que o permitido para concluir a execução
                hasTimedOut = ExecutingJob.HasTimedOut();
                // Se a execução foi bem sucedida e se não demorou tempo demais, 
                // marca para consolidar as alterações de estado e publicar os eventos decorrentes da conclusão da execução com sucesso
                if (executionSucceeded && !hasTimedOut)
                    succeeded = true;
            }
            catch (ThreadAbortException threadAbortException)
            {
                // ThreadAbortException ocorrerá caso o Dispatcher em que o Worker está rodando seja finalizado, 
                // portanto deve ser ignorado e considerado como uma execução não completada
                ExecutingJob.ExecutionException = threadAbortException;
                executionSucceeded = false;
                succeeded = false;
            }
            catch (Exception executionException)
            {
                // Se houve exceção, armazena na solicitação de trabalho para posterior análise
                ExecutingJob.ExecutionException = executionException;
                executionSucceeded = false;
                succeeded = false;
                Log.Error(String.Format("Error when trying to execute {0}!", GetType().Name), executionException);
            }

            // Se demorou demais para concluir a execução
            if (hasTimedOut)
            {
                // Um trabalho que não concluiu em tempo é considerado um trabalho que falhou
                ExecutingJob.ExecutionStatus = JobStatus.Failed;
                Timeout();
            }
            // Se a execução concluiu dentro do tempo esperado
            else
            {
                // Armazena o resultado da execução para posterior análise
                ExecutingJob.ExecutionStatus = executionSucceeded ? JobStatus.Succeeded : JobStatus.Failed;
            }

            // Consolida ou cancela as alterações de estado decorrentes da execução do trabalho
            if (succeeded)
            {
                PublishingBus.Publish(JobStatus.Succeeded.TopicId(), ExecutingJob, StorageType.NonPersistent);
                Complete();
            }
            else if (!hasTimedOut)
            {
                PublishingBus.Publish(JobStatus.Failed.TopicId(), ExecutingJob, StorageType.NonPersistent);
                Fail();
            }
            // Executa callback de confirmação da execução
            if (HasCompletionCallback())
                CompletionCallback(ExecutingJob);
        }

        private bool TryExecuteWithTimeout()
        {
            var result = false;
            Exception exception = null;
            var executorThread = new Thread(() =>
            {
                try
                {
                    result = TryExecute();
                }
                catch (ThreadAbortException)
                {
                }
                catch (Exception e)
                {
                    exception = e;
                }
            })
            {
                IsBackground = true
            };

            var sw = new Stopwatch();
            sw.Start();
            executorThread.Start();
            while (sw.Elapsed < ExecutingJob.RequestTimeout && executorThread.IsAlive)
                Thread.Sleep(10);
            sw.Stop();

            if (executorThread.IsAlive)
                executorThread.Abort();

            if (exception != null)
                throw exception;

            return result;
        }

        protected bool IsCancellationRequested
        {
            get
            {
                return CancellationTokenSource.Token.IsCancellationRequested;
            }
        }

        /// <summary>
        /// This method is executed when the work is timed out
        /// </summary>
        /// <remarks>
        /// You should use this method to revert the changes made during the work execution, in case of timeout
        /// </remarks>
        protected abstract void Timeout();

        /// <summary>
        /// This method is executed when the work is completed with success, nor failed or timed out
        /// </summary>
        /// <remarks>
        /// You should use this method to commit the changes made during the work execution, in case of success
        /// </remarks>
        protected abstract void Complete();

        /// <summary>
        /// This method is executed when the work fails or times out
        /// </summary>
        /// <remarks>
        /// You should use this method to rollback the changes made during the work execution, in case of failure
        /// </remarks>
        protected abstract void Fail();

        public bool HasCompletionCallback()
        {
            return CompletionCallback != null;
        }

        public bool IsExecuting(Job job)
        {
            return ExecutingJob == job;
        }

        protected abstract bool TryExecute();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                SubscriptionBus.Dispose();
                PublishingBus.Dispose();
                RequestBus.Dispose();
                ResponseBus.Dispose();
                SendingBus.Dispose();
                ReceivingBus.Dispose();
            }
        }
    }
}
