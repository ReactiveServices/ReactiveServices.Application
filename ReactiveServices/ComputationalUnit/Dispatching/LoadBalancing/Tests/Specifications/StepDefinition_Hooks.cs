using System;
using ReactiveServices.Extensions;
using TechTalk.SpecFlow;

namespace ReactiveServices.ComputationalUnit.Dispatching.LoadBalancing.Tests.Specifications
{
    sealed partial class StepDefinition : IDisposable
    {
        static StepDefinition()
        {
            AppDomain.CurrentDomain.LogExceptions();
        }
        
        private StepsExecutor StepsExecutor;

        [BeforeScenario]
        public void Setup_BalanceamentoDeCargaEntreDespachantes()
        {
            StepsExecutor = new StepsExecutor();
            StepsExecutor.ConfigureConnectionsForWorkDispatcher();
            StepsExecutor.InstantiateWorkDispatcher();
            StepsExecutor.ConfigureConnectionsForAnotherWorkDispatcher();
            StepsExecutor.InstantiateAnotherWorkDispatcher();
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
