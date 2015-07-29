using PostSharp.Patterns.Diagnostics;
using TechTalk.SpecFlow;
using FluentAssertions;

namespace ReactiveServices.Application.Termination.Tests.Specifications
{
    [Binding]
    [Log(AttributeExclude = true)]
    [LogException(AttributeExclude = true)]
    public class StepsDefinition
    {
        private readonly StepsContext Context;

        /// <summary>
        /// See http://www.specflow.org/documentation/Context-Injection/
        /// </summary>
        public StepsDefinition(StepsContext context)
        {
            Context = context;
        }

        [Given(@"que tenha sido solicitada a execução de um sistema de serviços através de um script de bootstrap")]
        public void DadoQueTenhaSidoSolicitadaAExecucaoDeUmSistemaDeServicosAtravesDeUmScriptDeBootstrap()
        {
            Context.ConfigureApplicationExecution();
        }

        [Given(@"que este sistema de serviços esteja em execução")]
        public void DadoQueEsteSistemaDeServicosEstejaEmExecucao()
        {
            Context.ExecuteBootstrapScript();
        }


        [When(@"o encerramento desse sistema for solicitado")]
        public void QuandoOEncerramentoDesseSistemaForSolicitado()
        {
            Context.TerminateApplication();
        }

        [When(@"tiver passado tempo suficiente para que todos os serviços se encerrem")]
        public void QuandoTiverPassadoTempoSuficienteParaQueTodosOsServicosSeEncerrem()
        {
            Context.WaitForApplicationTermination();
        }


        [Then(@"todos os serviços que compõem esse sistema devem ser encerrados")]
        public void EntaoTodosOsServicosQueCompoemEsseSistemaDevemSerEncerrados()
        {
            Context.AllDispatchersHaveTerminated().Should().BeTrue();
        }
    }
}
