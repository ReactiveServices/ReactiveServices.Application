using System;
using System.Threading;
using ReactiveServices.Extensions;
using TechTalk.SpecFlow;

namespace ReactiveServices.ComputationalUnit.Dispatching.Tests.Specifications
{
    sealed partial class StepDefinition : IDisposable
    {
        static StepDefinition()
        {
            AppDomain.CurrentDomain.LogExceptions();
        }
        
        private StepsExecutor StepsExecutor;

        [BeforeScenario("TO_REMOVE_ConfigureAndIntializeDispatcher")]
        public void Setup_ConfigurarRecebimentoDeSolicitacoesDeTrabalhoPendentes()
        {
            StepsExecutor = StepsExecutor ?? new StepsExecutor();
            StepsExecutor.ConfigureConnectionsForWorkDispatcher();
            StepsExecutor.InstantiateWorkDispatcher();
        }

        [BeforeScenario]
        public void Setup()
        {
            StepsExecutor = StepsExecutor ?? new StepsExecutor();
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
