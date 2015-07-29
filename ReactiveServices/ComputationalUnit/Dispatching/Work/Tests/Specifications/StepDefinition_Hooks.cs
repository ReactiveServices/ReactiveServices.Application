using System;
using NLog;
using ReactiveServices.Extensions;
using TechTalk.SpecFlow;

namespace ReactiveServices.ComputationalUnit.Dispatching.Work.Tests.Specifications
{
    sealed partial class StepDefinition : IDisposable
    {
        private Logger Log = LogManager.GetCurrentClassLogger();

        static StepDefinition()
        {
            AppDomain.CurrentDomain.LogExceptions();
        }
        
        private StepsExecutor StepsExecutor;

        [BeforeScenario]
        public void Setup_VerificarSolicitacoesNaoConcluidas()
        {
            StepsExecutor = new StepsExecutor();
            StepsExecutor.ConfigureConnectionsForWorkDispatcher();
            StepsExecutor.InstantiateWorkDispatcher();

            StepsExecutor.InstantiateDummyJobAndConfigureSubscriptionOnWorkDispatcher();
            StepsExecutor.InitializeWorkDispatcher();
        }

        [AfterScenario]
        public void TearDown()
        {
            StepsExecutor.Dispose();
            StepsExecutor = null;
        }

        public void Dispose()
        {
            // Be carefull to not call here something that is already executed in a teardown
            if (StepsExecutor != null)
                StepsExecutor.Dispose();
        }
    }
}
