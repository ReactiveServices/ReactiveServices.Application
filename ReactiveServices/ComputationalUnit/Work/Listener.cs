using System;
using System.Threading;
using ReactiveServices.MessageBus;

namespace ReactiveServices.ComputationalUnit.Work
{
    /// <summary>
    /// Classe base para Listeners.
    /// </summary>
    /// <remarks>
    /// Um listener é um tipo especial de Worker que permanece continuamente em execução, aguardando por eventos externos,
    /// não possuindo portanto resultados a serem efetivados ou cancelados ao término de sua execução.
    /// </remarks>
    public abstract class Listener : Worker
    {
        protected sealed override bool TryExecute()
        {
            Start();
            WaitForCancellationResquestedEvent();
            return true;
        }

        /// <summary>
        /// Keep the listener in loop, executing the Loop method and checking for cancellation at every <see cref="LoopInterval"/>
        /// </summary>
        private void WaitForCancellationResquestedEvent()
        {
            while (true)
            {
                Thread.Sleep(LoopInterval);
                if (IsCancellationRequested)
                    break;
                Loop();
            }
        }

        /// <summary>
        /// Interval to wait between each loop cicle, after the Start method is executed
        /// </summary>
        protected TimeSpan LoopInterval = TimeSpan.FromMilliseconds(10);

        /// <summary>
        /// This method is executed when the execution of the work of the listener is started
        /// </summary>
        protected virtual void Start()
        {
        }

        /// <summary>
        /// This method is executed at every <see cref="LoopInterval"/> milliseconds after the Start method finishes
        /// </summary>
        protected virtual void Loop()
        {
        }

        /// <summary>
        /// This method is executed when the listener fails and is marked as timed out
        /// </summary>
        protected override void Timeout()
        {
        }

        /// <summary>
        /// This method is executed when the listener completes its work
        /// </summary>
        protected override void Complete()
        {
        }

        /// <summary>
        /// This method is executed when the listener fails to complete its work
        /// </summary>
        protected override void Fail()
        {
        }
    }
}