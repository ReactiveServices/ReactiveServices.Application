using System;
using System.Threading;
using FluentAssertions;
using PostSharp.Patterns.Diagnostics;
using TechTalk.SpecFlow;

namespace ReactiveServices.Application.Monitoring.Tests.Specifications
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

        [Given(@"que um despachante de trabalhos de identificador '(.*)' não seja instanciado")]
        public void DadoQueUmDespachanteDeTrabalhosDeIdentificadorNaoSejaInstanciado(string p0)
        {
            // Does nothing
        }

        [Given(@"que um supervisor seja configurado para monitorar o despachante de trabalhos de identificador '(.*)'")]
        public void DadoQueUmSupervisorSejaConfiguradoParaMonitorarODespachanteDeTrabalhosDeIdentificador(string p0)
        {
            Context.ConfigureSupervisorFor(p0);
        }

        [Given(@"que um despachante de trabalhos de identificador '(.*)' seja instanciado")]
        public void DadoQueUmDespachanteDeTrabalhosDeIdentificadorSejaInstanciado(string p0)
        {
            Context.InitializeDispatcher(p0);
        }


        [When(@"uma solicitação de encerramento de execução do tipo Kill for postada pelo supervisor para a instância '(.*)'")]
        public void QuandoUmaSolicitacaoDeEncerramentoDeExecucaoDoTipoKillForPostadaPeloSupervisorParaAInstancia(string p0)
        {
            Context.TerminateDispatcher(p0);
        }

        [When(@"o supervisor for executado")]
        public void QuandoOSupervisorForExecutado()
        {
            Context.StartSupervisorWithoutServiceRestoration();
        }


        [Then(@"o supervisor deve identificar que o despachante '(.*)' não está em execução")]
        public void EntaoOSupervisorDeveIdentificarQueODespachanteNaoEstaEmExecucao(string p0)
        {
            Thread.Sleep(TimeSpan.FromSeconds(7));

            Context.ReceivedDispatcherOfflineEventFor(p0).Should().BeTrue();
        }

        [Then(@"o supervisor deve identificar que o despachante '(.*)' está em execução")]
        public void EntaoOSupervisorDeveIdentificarQueODespachanteEstaEmExecucao(string p0)
        {
            Thread.Sleep(TimeSpan.FromSeconds(7));

            Context.ReceivedDispatcherOnlineEventFor(p0).Should().BeTrue();
        }
    }
}
