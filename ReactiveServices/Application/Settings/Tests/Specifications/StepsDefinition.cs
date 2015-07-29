using FluentAssertions;
using PostSharp.Patterns.Diagnostics;
using TechTalk.SpecFlow;

namespace ReactiveServices.Application.Settings.Tests.Specifications
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


        [When(@"as configurações forem carregadas no gerenciador de partida")]
        public void QuandoAsConfiguracoesForemCarregadasNoGerenciadorDePartida()
        {
            // Does nothing
        }

        [When(@"as configurações forem salvas em um arquivo")]
        public void QuandoAsConfiguracoesForemSalvasEmUmArquivo()
        {
            Context.SaveBootstrapSettings();
        }

        [When(@"o arquivo for carregado nas configurações do gerenciador de partida")]
        public void QuandoOArquivoForCarregadoNasConfiguracoesDoGerenciadorDePartida()
        {
            Context.LoadBootstrapSettings();
        }


        [Then(@"o gerenciador de partida deve solicitar a execução de uma instância do despachante de trabalhos a ser identificada como despachante '(.*)'")]
        public void EntaoOGerenciadorDePartidaDeveSolicitarAExecucaoDeUmaInstanciaDoDespachanteDeTrabalhosASerIdentificadaComoDespachante(string p0)
        {
            Context.CountOfRequestedInstancesOfDispatchersOfId(p0).Should().Be(1);
        }

        [Then(@"o despachante '(.*)' deve ser capaz de processar até (.*) solicitações de trabalho por vez")]
        public void EntaoODespachanteDeveSerCapazDeProcessarAteSolicitacoesDeTrabalhoPorVez(string p0, int p1)
        {
            Context.NumberOfJobsThatCanBeProcessedInParallel(p0).Should().Be(p1);
        }

        [Then(@"o despachante '(.*)' deve ser capaz de processar solicitações de trabalho do tipo '(.*)'")]
        public void EntaoODespachanteDeveSerCapazDeProcessarSolicitacoesDeTrabalhoDoTipo(string p0, string p1)
        {
            Context.CanProcessJobsOfType(p0, p1).Should().BeTrue();
        }

        [Then(@"o gerenciador de partida deve solicitar a execução de (.*) solicitações de trabalho do tipo '(.*)' na partida do sistema")]
        public void EntaoOGerenciadorDePartidaDeveSolicitarAExecucaoDeSolicitacoesDeTrabalhoDoTipoNaPartidaDoSistema(int p0, string p1)
        {
            Context.NumberOfJobsToBeRequestedAtBootstrap(p1).Should().Be(p0);
        }
    }
}
