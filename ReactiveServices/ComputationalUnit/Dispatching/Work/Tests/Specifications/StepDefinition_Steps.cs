using PostSharp.Patterns.Diagnostics;
using ReactiveServices.ComputationalUnit.Settings;
using ReactiveServices.ComputationalUnit.Work;
using TechTalk.SpecFlow;

namespace ReactiveServices.ComputationalUnit.Dispatching.Work.Tests.Specifications
{
    [Binding]
    [Log(AttributeExclude = true)]
    [LogException(AttributeExclude = true)]
    partial class StepDefinition
    {
        #region Givens

        [Given(@"que no máximo (.*) tentativas possam ser feitas para execução da solicitação de trabalho")]
        public void DadoQueNoMaximoTentativasPossamSerFeitasParaExecucaoDaSolicitacaoDeTrabalho(int maximoDeTentativas)
        {
            StepsExecutor.ConfigureRequestTimeoutsOnDummyJobAs(null, maximoDeTentativas);
            StepsExecutor.ConfigureIntervalForCheckingUnfinishedJobsOnSettingsTo(5000);
        }

        [Given(@"que uma solicitação de trabalho tenha sido marcada com um tempo máximo de processamento de (.*) segundos")]
        public void DadoQueUmaSolicitacaoDeTrabalhoTenhaSidoMarcadaComUmTempoMaximoDeProcessamentoDeSegundos(int requestTimeout)
        {
            StepsExecutor.ConfigureRequestTimeoutsOnDummyJobAs(requestTimeout, null);
            StepsExecutor.ConfigureIntervalForCheckingUnfinishedJobsOnSettingsTo(800);
        }

        [Given(@"que a solicitação de trabalho precise de (.*) segundos para ser concluída")]
        public void DadoQueASolicitacaoDeTrabalhoPreciseDeSegundosParaSerConcluida(int duracaoDoTrabalho)
        {
            StepsExecutor.ConfigureDummyJobDurationTo(duracaoDoTrabalho);
        }

        [Given(@"que a solicitação de trabalho tenha sido marcada como em andamento há (.*) segundos atrás")]
        public void DadoQueASolicitacaoDeTrabalhoTenhaSidoMarcadaComoEmAndamentoHaSegundosAtras(int tempoPassado)
        {
            StepsExecutor.ConfigureWaitingTimeForWorkExecutionTo(tempoPassado);
        }

        [Given(@"que a solicitação de trabalho aceite ser repetida caso falhe")]
        public void DadoQueASolicitacaoDeTrabalhoAceiteSerRepetidaCasoFalhe()
        {
            StepsExecutor.SetFailureActionForDummyJobTo(JobFailureAction.Repeat);
        }

        [Given(@"que a solicitação de trabalho não aceite ser repetida caso falhe, tendo que ser registrada em log nesse caso")]
        public void DadoQueASolicitacaoDeTrabalhoNaoAceiteSerRepetidaCasoFalheTendoQueSerRegistradaEmLogNesseCaso()
        {
            StepsExecutor.SetFailureActionForDummyJobTo(JobFailureAction.Log);
        }

        [Given(@"que a solicitação de trabalho não apresente nenhuma falha durante sua execução")]
        public void DadoQueASolicitacaoDeTrabalhoNaoApresenteNenhumaFalhaDuranteSuaExecucao()
        {
            StepsExecutor.SetExpectedExecutionStatusForDummyJobTo(JobStatus.Succeeded);
        }

        [Given(@"que a solicitação de trabalho apresente alguma falha durante sua execução")]
        public void DadoQueASolicitacaoDeTrabalhoApresenteAlgumaFalhaDuranteSuaExecucao()
        {
            StepsExecutor.SetExpectedExecutionStatusForDummyJobTo(JobStatus.Failed);
        }

        #endregion

        #region Whens
        [When(@"o despachante de trabalhos verificar a lista de trabalhos em andamento")]
        public void QuandoODespachanteDeTrabalhosVerificarAListaDeTrabalhosEmAndamento()
        {
            StepsExecutor.SubscribeToDummyJobCompletionEvents();
            StepsExecutor.PublishDummyJobAndWaitForVerification();
        }

        #endregion

        #region Thens
        [Then(@"o despachante deve remover a solicitação da lista de trabalhos em andamento")]
        public void EntaoODespachanteDeveRemoverASolicitacaoDaListaDeTrabalhosEmAndamento()
        {
            StepsExecutor.AssertOnWorkDispatcherThatTheDummyJobPassedThrough(DispatcherActivity.RequestWorkerWasFinalized);
        }

        [Then(@"o despachante deve republicar a solicitação de trabalho como pendente")]
        public void EntaoODespachanteDeveRepublicarASolicitacaoDeTrabalhoComoPendente()
        {
            StepsExecutor.AssertOnWorkDispatcherThatTheDummyJobHaveBeenRepublishedAs(JobStatus.Pending);
        }

        [Then(@"o despachante deve republicar a solicitação de trabalho como mal sucedida")]
        public void EntaoODespachanteDeveRepublicarASolicitacaoDeTrabalhoComoMalSucedida()
        {
            StepsExecutor.AssertOnWorkDispatcherThatTheDummyJobHaveBeenRepublishedAs(JobStatus.Failed);
        }

        //[Then(@"o despachante deve manter a solicitação de trabalho em andamento")]
        //public void EntaoODespachanteDeveManterASolicitacaoDeTrabalhoEmAndamento()
        //{
        //    StepsExecutor.AssertOnWorkDispatcherThatTheWorkDispatcherHaveKeptTheDummyJobAsProcessing();
        //}

        [Then(@"o despachante não deve republicar a solicitação de trabalho como pendente")]
        public void EntaoODespachanteNaoDeveRepublicarASolicitacaoDeTrabalhoComoPendente()
        {
            StepsExecutor.AssertOnWorkDispatcherThatTheDummyJobHaveNotBeenRepublishedAs(JobStatus.Pending);
        }

        [Then(@"o despachante deve registrar a falha no log de operações do despachante de trabalhos")]
        public void EntaoODespachanteDeveRegistrarAFalhaNoLogDeOperacoesDoDespachanteDeTrabalhos()
        {
            StepsExecutor.AssertOnWorkDispatcherThatTheDummyJobHaveBeenRepublishedAs(JobStatus.Failed);
        }

        #endregion
    }
}
