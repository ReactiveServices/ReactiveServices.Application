using PostSharp.Patterns.Diagnostics;
using ReactiveServices.ComputationalUnit.Work;
using TechTalk.SpecFlow;

namespace ReactiveServices.ComputationalUnit.Dispatching.LoadBalancing.Tests.Specifications
{
    [Binding]
    [Log(AttributeExclude = true)]
    [LogException(AttributeExclude = true)]
    partial class StepDefinition
    {
        #region Givens

        [Given(@"que a solicitação de trabalho tenha sido marcada como em andamento há (.*) segundos atrás")]
        public void DadoQueASolicitacaoDeTrabalhoTenhaSidoMarcadaComoEmAndamentoHaSegundosAtras(int tempoPassado)
        {
            StepsExecutor.ConfigureWaitingTimeForWorkExecutionTo(tempoPassado);
        }

        [Given(@"que dois despachantes de trabalhos tenham sido configurados para receber solicitacoes pendentes do tipo A")]
        public void DadoQueDoisDespachantesDeTrabalhosTenhamSidoConfiguradosParaReceberSolicitacoesPendentesDoTipoA()
        {
            StepsExecutor.InstantiateWorkDispatcher();
            StepsExecutor.ConfigureSubscriptionForDummyJobOnWorkDispatcher();
            StepsExecutor.InitializeWorkDispatcher();

            StepsExecutor.InstantiateAnotherWorkDispatcher();
            StepsExecutor.ConfigureSubscriptionForDummyJobOnAnotherWorkDispatcher();
            StepsExecutor.InitializeAnotherWorkDispatcher();

            StepsExecutor.InstantiateDummyJob();
        }

        [Given(@"que um despachante de trabalhos tenha sido configurado para receber solicitacoes pendentes do tipo A")]
        public void DadoQueUmDespachanteDeTrabalhosTenhaSidoConfiguradoParaReceberSolicitacoesPendentesDoTipoA()
        {
            StepsExecutor.InstantiateWorkDispatcher();
            StepsExecutor.ConfigureSubscriptionForDummyJobOnWorkDispatcher();
            StepsExecutor.InitializeWorkDispatcher();
            StepsExecutor.InstantiateDummyJob();
        }

        [Given(@"que outro despachante de trabalhos tenha sido configurado para receber solicitacoes pendentes do tipo B")]
        public void DadoQueOutroDespachanteDeTrabalhosTenhaSidoConfiguradoParaReceberSolicitacoesPendentesDoTipoB()
        {
            StepsExecutor.InstantiateAnotherWorkDispatcher();
            StepsExecutor.ConfigureSubscriptionForAnotherDummyJobOnAnotherWorkDispatcher();
            StepsExecutor.InitializeAnotherWorkDispatcher();
            StepsExecutor.InstantiateAnotherDummyJob();
        }

        [Given(@"que exista um despachante de trabalhos ocioso")]
        public void DadoQueExistaUmDespachanteDeTrabalhosOcioso()
        {
            StepsExecutor.InstantiateWorkDispatcher();
            StepsExecutor.ConfigureSubscriptionForDummyJobOnWorkDispatcher();
            StepsExecutor.InitializeWorkDispatcher();
        }

        [Given(@"que uma solicitação de trabalho tenha sido encaminhada para execução por um outro despachante de trabalhos")]
        public void DadoQueUmaSolicitacaoDeTrabalhoTenhaSidoEncaminhadaParaExecucaoPorUmOutroDespachanteDeTrabalhos()
        {
            StepsExecutor.InstantiateAnotherWorkDispatcher();
            StepsExecutor.ConfigureSubscriptionForDummyJobOnAnotherWorkDispatcher();
            StepsExecutor.ConfigureIntervalForCheckingUnfinishedJobsOnAnotherSettingsTo(11000);
            StepsExecutor.InitializeAnotherWorkDispatcher();
            StepsExecutor.InstantiateDummyJob();

            StepsExecutor.ConfigureDummyJobDurationTo(14);
        }

        [Given(@"que a solicitação de trabalho tenha sido marcada com um tempo máximo de processamento de (.*) segundos")]
        public void DadoQueASolicitacaoDeTrabalhoTenhaSidoMarcadaComUmTempoMaximoDeProcessamentoDeSegundos(int requestTimeout)
        {
            StepsExecutor.ConfigureRequestTimeoutsForDummyJobOnAnotherSettingsAs(requestTimeout, 2);
            StepsExecutor.ConfigureIntervalForCheckingUnfinishedJobsOnAnotherSettingsTo(800);
        }

        [Given(@"que (.*) despachantes tenham se inscrito para processar solicitações de trabalho do tipo A")]
        public void DadoQueDespachantesTenhamSeInscritoParaProcessarSolicitacoesDeTrabalhoDoTipoA(int numberOfDispatchers)
        {
            StepsExecutor.ConfigureConnectionsForMultipleDispatchers(numberOfDispatchers);
            StepsExecutor.ConfigureSubscriptionForDummyJobForMultipleDispatchers();
            StepsExecutor.InitializeMultipleDispatchers();
        }

        [Given(@"que um despachante de trabalhos tenha sido configurado para processar (.*) solicitação de trabalho do tipo A")]
        public void DadoQueUmDespachanteDeTrabalhosTenhaSidoConfiguradoParaProcessarSolicitacaoDeTrabalhoDoTipoA(int numberOfRequests)
        {
            StepsExecutor.ConfigureSubscriptionForDummyJobOnWorkDispatcher();
            StepsExecutor.ConfigureMaximumNumberOfProcessingJobsOnSettingsTo(numberOfRequests);
            StepsExecutor.InitializeWorkDispatcher();
        }

        #endregion

        #region Whens
        [When(@"uma solicitação de trabalho do tipo A for postada como pendente")]
        public void QuandoUmaSolicitacaoDeTrabalhoDoTipoAForPostadaComoPendente()
        {
            StepsExecutor.PublishDummyJobAndWaitForVerification();
        }

        [When(@"duas solicitações de trabalho do tipo A forem postadas como pendente")]
        public void QuandoDuasSolicitacoesDeTrabalhoDoTipoAForemPostadasComoPendente()
        {
            StepsExecutor.PublishDummyJobAndWaitForVerification();
            StepsExecutor.PublishDummyJobAndWaitForVerification();
        }

        [When(@"uma solicitação de trabalho do tipo B for postada como pendente")]
        public void QuandoUmaSolicitacaoDeTrabalhoDoTipoBForPostadaComoPendente()
        {
            StepsExecutor.PublishAnotherDummyJobAndWaitForVerification();
        }

        [When(@"o outro despachante de trabalhos verificar que a solicitação de trabalho expirou")]
        public void QuandoOOutroDespachanteDeTrabalhosVerificarQueASolicitacaoDeTrabalhoExpirou()
        {
            StepsExecutor.PublishDummyJobAndWaitForVerification();
        }

        [When(@"(.*) solicitações de trabalho do tipo A forem enviadas para processamento")]
        public void QuandoSolicitacoesDeTrabalhoDoTipoAForemEnviadasParaProcessamento(int numeroDeSolicitacoes)
        {
            StepsExecutor.PublishMultipleDummyJobs(numeroDeSolicitacoes);
        }

        #endregion

        #region Thens

        [Then(@"o outro despachante deve republicar a solicitação de trabalho como pendente")]
        public void EntaoOOutroDespachanteDeveRepublicarASolicitacaoDeTrabalhoComoPendente()
        {
            StepsExecutor.AssertOnAnotherWorkDispatcherThatTheDummyJobHaveBeenRepublishedAs(JobStatus.Pending);
        }
        [Then(@"apenas um dos despachantes deve receber a solicitação de trabalho do tipo A recém postada como pendente")]
        public void EntaoApenasUmDosDespachantesDeveReceberASolicitacaoDeTrabalhoDoTipoARecemPostadaComoPendente()
        {
            StepsExecutor.AssertOnASingleWorkDispatcherThatTheDummyJobPassedThrough(DispatcherActivity.JobWasReceived);
        }

        [Then(@"o outro despachante de trabalhos irá processar a segunda solicitação A")]
        public void EntaoOOutroDespachanteDeTrabalhosIraProcessarASegundaSolicitacaoA()
        {
            StepsExecutor.AssertOnAnotherWorkDispatcherThatTheDummyJobPassedThrough(DispatcherActivity.JobWasReceived);
        }

        [Then(@"o primeiro despachante deve receber a solicitação de trabalho do tipo A recém postada como pendente")]
        public void EntaoOPrimeiroDespachanteDeveReceberASolicitacaoDeTrabalhoDoTipoARecemPostadaComoPendente()
        {
            StepsExecutor.AssertOnWorkDispatcherThatTheDummyJobPassedThrough(DispatcherActivity.JobWasReceived);
        }

        [Then(@"um dos despachantes de trabalhos irá processar a primeira solicitação A recebida")]
        public void EntaoUmDosDespachantesDeTrabalhosIraProcessarAPrimeiraSolicitacaoARecebida()
        {
            StepsExecutor.AssertOnAnyWorkDispatcherThatTheDummyJobPassedThrough(DispatcherActivity.JobWasReceived);
        }

        [Then(@"o segundo despachante deve receber a solicitação de trabalho do tipo B recém postada como pendente")]
        public void EntaoOSegundoDespachanteDeveReceberASolicitacaoDeTrabalhoDoTipoBRecemPostadaComoPendente()
        {
            StepsExecutor.AssertOnAnotherWorkDispatcherThatTheDummyJobPassedThrough(DispatcherActivity.JobWasReceived);
        }

        [Then(@"ao menos 2 dos (.*) despachantes de trabalho devem ser acionados para atendimento às solicitacoes")]
        public void AoMenosDoisDosDespachantesDeTrabalhoDevemSerAcionadosParaAtendimentoAsSolicitacoes(int p0)
        {
            StepsExecutor.AssertThatAtLeastTwoOfTheMultipleDispatchersHaveCompletedAJob();
        }

        [Then(@"o despachante de trabalhos deve completar as (.*) solicitações de trabalho")]
        public void EntaoODespachanteDeTrabalhosDeveCompletarAsSolicitacoesDeTrabalho(int numberOfCompletedJobs)
        {
            StepsExecutor.AssertOnWorkDispatcherThatANumberOfDummyJobsWereCompleted(numberOfCompletedJobs);
        }

        [Then(@"a primeira solicitação de trabalho deve ter sido completada antes que a segunda tenha sido recebida")]
        public void EntaoAPrimeiraSolicitacaoDeTrabalhoDeveTerSidoCompletadaAntesQueASegundaTenhaSidoRecebida()
        {
            StepsExecutor.AssertOnWorkDispatcherThatOneJobHasCompletedBeforeTheSecondHasStarted();
        }

        #endregion
    }
}
