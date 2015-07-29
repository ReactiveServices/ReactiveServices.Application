using FluentAssertions;
using PostSharp.Patterns.Diagnostics;
using TechTalk.SpecFlow;

namespace ReactiveServices.Application.Restoration.Tests.Specifications
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

        [Given(@"que as configurações solicitem a execução de uma instância do despachante de trabalhos a ser identificada como despachante '(.*)'")]
        public void DadoQueAsConfiguracoesSolicitemAExecucaoDeUmaInstanciaDoDespachanteDeTrabalhosASerIdentificadaComoDespachante(string p0)
        {
            Context.AddDispatcherConfigurationFor(p0);
        }

        [Given(@"que um supervisor seja configurado para monitorar o despachante de trabalhos de identificador '(.*)'")]
        public void DadoQueUmSupervisorSejaConfiguradoParaMonitorarODespachanteDeTrabalhosDeIdentificador(string p0)
        {
            Context.ConfigureSupervisorFor(p0);
        }

        [Given(@"que as configurações informem que o despachante '(.*)' será capaz de processar até (.*) solicitações de trabalho por vez")]
        public void DadoQueAsConfiguracoesInformemQueODespachanteSeraCapazDeProcessarAteSolicitacoesDeTrabalhoPorVez(string p0, int p1)
        {
            Context.SetMaximumParallelJobsForDispatcher(p0, p1);
        }

        [Given(@"que as configurações informem que o despachante '(.*)' será capaz de processar solicitações de trabalho do tipo '(.*)'")]
        public void DadoQueAsConfiguracoesInformemQueODespachanteSeraCapazDeProcessarSolicitacoesDeTrabalhoDoTipo(string p0, string p1)
        {
            Context.ConfigureDispatcherToHandleJob(p0, p1);
        }

        [Given(@"que as configurações solicitem a execução de (.*) solicitações de trabalho do tipo '(.*)' na partida do sistema")]
        public void DadoQueAsConfiguracoesSolicitemAExecucaoDeSolicitacoesDeTrabalhoDoTipoNaPartidaDoSistema(int p0, string p1)
        {
            Context.ConfigureBootstrapToExecuteJob(p0, p1);
        }


        [When(@"uma solicitação de encerramento de execução do tipo Kill for postada para a instância '(.*)'")]
        public void QuandoUmaSolicitacaoDeEncerramentoDeExecucaoDoTipoKillForPostadaParaAInstancia(string p0)
        {
            Context.TerminateDispatcher(p0);
        }

        [When(@"o supervisor for executado")]
        public void QuandoOSupervisorForExecutado()
        {
            Context.ExecuteSupervisor();
        }


        [Then(@"o supervisor deve colocar o despachante '(.*)' em execução pela primeira vez")]
        public void EntaoOSupervisorDeveColocarODespachanteEmExecucaoPelaPrimeiraVez(string p0)
        {
            Context.IsDispatcherOnline(p0).Should().BeTrue();
        }

        [Then(@"o supervisor deve colocar o despachante '(.*)' novamente em execução")]
        public void EntaoOSupervisorDeveColocarODespachanteNovamenteEmExecucao(string p0)
        {
            Context.HasDispatcherRestarted(p0).Should().BeTrue();
        }
    }
}
