using NLog;
using System.Threading;
using ReactiveServices.ComputationalUnit.Work;

namespace ReactiveServices.ComputationalUnit.Dispatching.Tests
{
    /// <summary>
    /// Exemplo de Worker, usado para testes manuais, atrvés do arquivo SampleSettings.xml
    /// </summary>
    public class SampleWorker : Worker
    {
        protected override bool TryExecute()
        {
            Thread.Sleep(2000);
            ExecutingJob.ExecutionStatus = JobStatus.Succeeded;
            return true;
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
    }

    public class AnotherSampleWorker : SampleWorker
    {
    }

    public class SampleListener : Listener
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        protected override void Start()
        {
            while (!IsCancellationRequested)
            {
                Log.Info("SampleListener looping...");
                Thread.Sleep(1000);
            }
        }
    }

    /// <summary>
    /// Exemplo de Job, usado para testes manuais, através do arquivo SampleSettings.xml
    /// </summary>
    public class SampleJob : Job
    {
    }

    public class AnotherSampleJob : Job
    {
    }
    
    public class SampleListenerJob : Job
    {
    }
}
