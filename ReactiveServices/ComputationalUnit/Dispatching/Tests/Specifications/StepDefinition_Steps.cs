using System.Threading;
using PostSharp.Patterns.Diagnostics;
using ReactiveServices.ComputationalUnit.Settings;
using ReactiveServices.ComputationalUnit.Work;
using TechTalk.SpecFlow;

namespace ReactiveServices.ComputationalUnit.Dispatching.Tests.Specifications
{
    [Binding]
    [Log(AttributeExclude = true)]
    [LogException(AttributeExclude = true)]
    partial class StepDefinition
    {
        #region Givens

        [Given(@"que tenham sido informados parametros corretos de configuração solicitando o recebimento de solicitações de trabalho do tipo A")]
        public void DadoQueTenhamSidoInformadosParametrosCorretosDeConfiguracaoSolicitandoORecebimentoDeSolicitacoesDeTrabalhoDoTipo()
        {
            StepsExecutor.ConfigureSubscriptionForDummyJobOnWorkDispatcher();
        }

        [Given(@"que tenham sido informados parametros incorretos de configuração solicitando o recebimento de solicitações de trabalho do tipo A")]
        public void DadoQueTenhamSidoInformadosParametrosIncorretosDeConfiguracaoSolicitandoORecebimentoDeSolicitacoesDeTrabalhoDoTipo()
        {
            StepsExecutor.ConfigureSubscriptionForDummyJobOnWorkDispatcher();
            StepsExecutor.InvalidateSettingsForFirstWorkDispatcher();
        }

        [Given(@"que tenham sido informados parametros de configuração solicitando o encaminhamento de solicitações de trabalho a um dado responsável pelo processamento")]
        public void DadoQueTenhamSidoInformadosParametrosDeConfiguracaoSolicitandoOEncaminhamentoDeSolicitacoesDeTrabalhoAUmDadoResponsavelPeloProcessamento()
        {
            StepsExecutor.ConfigureSubscriptionForDummyJobOnWorkDispatcher();
        }

        [Given(@"que o despachante de trabalhos tenha sido configurado para encaminhar mensagens do tipo A para um dado responsável")]
        public void DadoQueODespachanteDeTrabalhosTenhaSidoConfiguradoParaEncaminharMensagensDoTipoAParaUmDadoResponsavel()
        {
            StepsExecutor.InstantiateDummyJobAndConfigureSubscriptionOnWorkDispatcher();
            StepsExecutor.SubscribeToDummyJobCompletionEvents();
        }

        [Given(@"que uma solicitação de trabalho tenha sido marcada com um tempo máximo de processamento de (.*) segundos")]
        public void DadoQueUmaSolicitacaoDeTrabalhoTenhaSidoMarcadaComUmTempoMaximoDeProcessamentoDeSegundos(int requestTimeout)
        {
            StepsExecutor.ConfigureRequestTimeoutsOnDummyJobAs(requestTimeout, null);
            StepsExecutor.ConfigureIntervalForCheckingUnfinishedJobsOnSettingsTo(800);
        }

        [Given(@"que o despachante de trabalhos tenha sido configurado para receber solicitacoes pendentes do tipo A")]
        public void DadoQueODespachanteDeTrabalhosTenhaSidoConfiguradoParaReceberSolicitacoesPendentesDoTipo()
        {
            StepsExecutor.InstantiateDummyJobAndConfigureSubscriptionOnWorkDispatcher();
            StepsExecutor.InitializeWorkDispatcher();
        }

        [Given(@"que o despachante tenha se inscrito para receber notificações de conclusão de processamento para uma solicitação de trabalho do tipo A com identificador X")]
        public void DadoQueODespachanteTenhaSeinscritoParaReceberNotificacoesDeConclusaoDeProcessamentoParaUmaSolicitacaoDeTrabalhoDoTipoComIdentificador()
        {
            StepsExecutor.InstantiateDummyJobAndConfigureSubscriptionOnWorkDispatcher();
            StepsExecutor.InitializeWorkDispatcher();
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

        [Given(@"que exista uma única solicitação de trabalho em andamento encaminhada para execução")]
        public void DadoQueExistaUmaUnicaSolicitacaoDeTrabalhoEmAndamentoEncaminhadaParaExecucao()
        {
            StepsExecutor.InstantiateDummyJob();
            StepsExecutor.ConfigureDummyJobDurationTo(1);
            StepsExecutor.ConfigureWaitingTimeForWorkExecutionTo(3);
            StepsExecutor.PublishDummyJobAndWaitForVerification();
        }

        [Given(@"que a solicitação de trabalho tenha sido encaminhada para execução por um outro despachante de trabalhos")]
        public void DadoQueASolicitacaoDeTrabalhoTenhaSidoEncaminhadaParaExecucaoPorUmOutroDespachanteDeTrabalhos()
        {
            StepsExecutor.ConfigureDummyJobDurationTo(15);
            StepsExecutor.PublishDummyJob();
        }

        [Given(@"que a solicitação de trabalho encaminhada para o outro despachante de trabalhos tenha sido marcada como em andamento há '(.*)' segundos atrás")]
        public void DadoQueASolicitacaoDeTrabalhoEncaminhadaParaOOutroDespachanteDeTrabalhosTenhaSidoMarcadaComoEmAndamentoHaSegundosAtras(int tempoPassado)
        {
            StepsExecutor.WaitForVerification(tempoPassado);
        }

        [Given(@"que o agente de execução esteja executando um trabalho")]
        public void DadoQueOAgenteDeExecucaoEstejaExecutandoUmTrabalho()
        {
            StepsExecutor.ConfigureConnectionsForWorkDispatcher();
            StepsExecutor.InstantiateWorkDispatcher();
            StepsExecutor.InstantiateDummyJob();
            StepsExecutor.ConfigureSubscriptionForDummyJobOnWorkDispatcher();
            StepsExecutor.ConfigureIntervalForCheckingUnfinishedJobsOnSettingsTo(800);
            StepsExecutor.ConfigureWaitingTimeForWorkExecutionTo(15);
            StepsExecutor.InitializeWorkDispatcher();
        }

        [Given(@"que nenhum erro aconteça durante a execução do trabalho")]
        public void DadoQueNenhumErroAcontecaDuranteAExecucaoDoTrabalho()
        {
            StepsExecutor.SetExpectedExecutionStatusForDummyJobTo(JobStatus.Succeeded);
        }

        [Given(@"que a solicitação de execução do trabalho ainda não tenha expirado ao ser concluída")]
        public void DadoQueASolicitacaoDeExecucaoDoTrabalhoAindaNaoTenhaExpiradoAoSerConcluida()
        {
            StepsExecutor.ConfigureDummyJobDurationTo(1);
            StepsExecutor.ConfigureRequestTimeoutsOnDummyJobAs(5/*15*/, 2);
        }

        [Given(@"que a solicitação de execução do trabalho já tenha expirado ao ser concluída")]
        public void DadoQueASolicitacaoDeExecucaoDoTrabalhoJaTenhaExpiradoAoSerConcluida()
        {
            StepsExecutor.ConfigureDummyJobDurationTo(2);
            StepsExecutor.ConfigureRequestTimeoutsOnDummyJobAs(1, 1);
        }

        [Given(@"que algum erro tenha acontecido durante a execução")]
        public void DadoQueAlgumErroTenhaAcontecidoDuranteAExecucao()
        {
            StepsExecutor.SetExpectedExecutionStatusForDummyJobTo(JobStatus.Failed);
        }

        [Given(@"que o erro ocorrido seja do tipo que implique na repetição da execução")]
        public void DadoQueOErroOcorridoSejaDoTipoQueImpliqueNaRepeticaoDaExecucao()
        {
            StepsExecutor.SetFailureActionForDummyJobTo(JobFailureAction.Repeat);
        }

        [Given(@"que o erro ocorrido seja do tipo que implique no registro do erro em log, sem repetição")]
        public void DadoQueOErroOcorridoSejaDoTipoQueImpliqueNoRegistroDoErroEmLogSemRepeticao()
        {
            StepsExecutor.SetFailureActionForDummyJobTo(JobFailureAction.Log);
        }

        [Given(@"que exista um despachante de trabalhos em execução")]
        public void DadoQueExistaUmDespachanteDeTrabalhosEmExecucao()
        {
            StepsExecutor.ConfigureConnectionsForWorkDispatcher();
            StepsExecutor.ConfigureSubscriptionForDummyJobOnWorkDispatcher();
            StepsExecutor.ConfigureConnectionsForAnotherWorkDispatcher();
            StepsExecutor.InstantiateWorkDispatcher();
            StepsExecutor.InitializeWorkDispatcher();
        }

        [Given(@"que exista um outro despachante de trabalhos em execução")]
        public void DadoQueExistaUmOutroDespachanteDeTrabalhosEmExecucao()
        {
            StepsExecutor.ConfigureConnectionsForAnotherWorkDispatcher();
            StepsExecutor.ConfigureSubscriptionForDummyJobOnAnotherWorkDispatcher();
            StepsExecutor.InstantiateAnotherWorkDispatcher();
            StepsExecutor.InitializeAnotherWorkDispatcher();
        }

        [Given(@"que o despachante de trabalhos tenha recebido uma solicitação de trabalho com duração de (.*) segundos")]
        public void DadoQueODespachanteDeTrabalhosTenhaRecebidoUmaSolicitacaoDeTrabalhoComDuracaoDeSegundos(int duracao)
        {
            StepsExecutor.InstantiateDummyJob();
            StepsExecutor.ConfigureDummyJobDurationTo(duracao);
            StepsExecutor.PublishDummyJobAndWaitForVerification();
        }

        [Given(@"que um despachante de trabalhos tenha sido configurado para se manter ativo continuamente")]
        public void DadoQueUmDespachanteDeTrabalhosTenhaSidoConfiguradoParaSeManterAtivoContinuamente()
        {
            StepsExecutor.ConfigureConnectionsForWorkDispatcher();
            StepsExecutor.ConfigureLifeSpanOnSettings(DispatcherLifeSpanMode.Perpetual, 0);
            StepsExecutor.ConfigureSubscriptionForDummyJobOnWorkDispatcher();
            StepsExecutor.InstantiateWorkDispatcher();
        }

        [Given(@"que um despachante de trabalhos tenha sido configurado para se manter ativo até concluir seu primeiro trabalho com sucesso ou com falha irrecuperável")]
        public void DadoQueUmDespachanteDeTrabalhosTenhaSidoConfiguradoParaSeManterAtivoAteConcluirSeuPrimeiroTrabalhoComSucessoOuComFalhaIrrecuperavel()
        {
            StepsExecutor.ConfigureConnectionsForWorkDispatcher();
            StepsExecutor.ConfigureLifeSpanOnSettings(DispatcherLifeSpanMode.UntilFirstJobIsCompleted, 0);
            StepsExecutor.ConfigureSubscriptionForDummyJobOnWorkDispatcher();
            StepsExecutor.InstantiateWorkDispatcher();
        }

        [Given(@"que um despachante de trabalhos tenha sido configurado para se manter ativo até que (.*) segundos tenham se passado")]
        public void DadoQueUmDespachanteDeTrabalhosTenhaSidoConfiguradoParaSeManterAtivoAteQueSegundosTenhamSePassado(int tempoPassado)
        {
            StepsExecutor.ConfigureConnectionsForWorkDispatcher();
            StepsExecutor.ConfigureLifeSpanOnSettings(DispatcherLifeSpanMode.UntilTimedOut, tempoPassado);
            StepsExecutor.ConfigureSubscriptionForDummyJobOnWorkDispatcher();
            StepsExecutor.InstantiateWorkDispatcher();
        }

        [Given(@"que existam cinco arquivos contendo cada um uma matriz de números complexos de trinta por trinta serializados em formato JSON")]
        public void DadoQueExistamCincoArquivosContendoCadaUmUmaMatrizDeNumerosComplexosDeTrintaPorTrintaSerializadosEmFormatoJson()
        {
            //StepsExecutor.CreateSerializedMatricesIfNecessary();
        }

        [Given(@"que exista um script de inicialização configurando três despachantes de trabalho para tratar mensagens do tipo MatrixParsing e MatrixReversing")]
        public void DadoQueExistaUmScriptDeInicializacaoConfigurandoTresDespachantesDeTrabalhoParaTratarMensagensDoTipoMatrixParsingEMatrixReversing()
        {
            //const int numberOfDispatchers = 3;
            //StepsExecutor.ConfigureConnectionsForMultipleDispatchers(numberOfDispatchers);
            //StepsExecutor.ConfigureSubscriptionForMatrixRequestsForMultipleDispatchers();
            //StepsExecutor.ConfigureBootstrapScriptForMultipleDispatchers();
        }

        [Given(@"que haja uma assinatura para recebimento de eventos do tipo WorkCompleted do tipo MatrixReversed")]
        public void DadoQueHajaUmaAssinaturaParaRecebimentoDeEventosDoTipoWorkCompletedDoTipoMatrixReversed()
        {
            //StepsExecutor.SubscribeToMatrixReversedEvent();
        }

        #endregion

        #region Whens

        [When(@"se passarem (.*) segundos")]
        public void QuandoSePassaremSegundos(int tempoASePassar)
        {
            StepsExecutor.WaitForVerification(tempoASePassar);
        }


        [When(@"que o despachante de trabalhos receba uma solicitação de trabalho com duração de (.*) segundos")]
        public void QuandoQueODespachanteDeTrabalhosRecebaUmaSolicitacaoDeTrabalhoComDuracaoDeSegundos(int duracao)
        {
            StepsExecutor.InstantiateDummyJob();
            StepsExecutor.ConfigureDummyJobDurationTo(duracao);
            StepsExecutor.PublishDummyJobAndWaitForVerification();
        }

        [When(@"o despachante de trabalhos for inicializado")]
        public void QuandoODespachanteDeTrabalhosForInicializado()
        {
            StepsExecutor.InitializeWorkDispatcher();
        }

        [When(@"uma solicitação de trabalho do tipo A for recebida pelo despachante de trabalhos")]
        public void QuandoUmaSolicitacaoDeTrabalhoDoTipoAForRecebidaPeloDespachanteDeTrabalhos()
        {
            StepsExecutor.PublishDummyJobAndWaitForVerification();
        }

        [When(@"uma solicitação de trabalho do tipo A for postada como pendente")]
        public void QuandoUmaSolicitacaoDeTrabalhoDoTipoAForPostadaComoPendente()
        {
            StepsExecutor.PublishDummyJobAndWaitForVerification();
        }

        [When(@"for recebida uma notificação informando que uma solicitação de trabalho do tipo A com identificador X foi executada com sucesso")]
        public void QuandoForRecebidaUmaNotificacaoInformandoQueUmaSolicitacaoDeTrabalhoDoTipoAComIdentificadorFoiExecutadaComSucesso()
        {
            StepsExecutor.PublishDummyJobAndWaitForVerificationExpecting(JobStatus.Succeeded, withDuration: 0);
        }

        [When(@"for recebida uma notificação informando que uma solicitação de trabalho do tipo A com identificador X foi executada com falha e deve ser repetida")]
        public void QuandoForRecebidaUmaNotificacaoInformandoQueUmaSolicitacaoDeTrabalhoDoTipoAComIdentificadorFoiExecutadaComFalhaEDeveSerRepetida()
        {
            StepsExecutor.PublishDummyJobAndWaitForVerificationExpecting(JobStatus.Failed, JobFailureAction.Repeat);
        }

        [When(@"for recebida uma notificação informando que uma solicitação de trabalho do tipo A com identificador X foi executada com falha e deve ser logada como erro")]
        public void QuandoForRecebidaUmaNotificacaoInformandoQueUmaSolicitacaoDeTrabalhoDoTipoAComIdentificadorFoiExecutadaComFalhaEDeveSerLogadaComoErro()
        {
            StepsExecutor.PublishDummyJobAndWaitForVerificationExpecting(JobStatus.Failed, JobFailureAction.Log);
        }

        [When(@"a execução do trabalho for concluída")]
        public void QuandoAExecucaoDoTrabalhoForConcluida()
        {
            StepsExecutor.SubscribeToDummyJobCompletionEvents();
            StepsExecutor.PublishDummyJobAndWaitForVerification();
        }

        [When(@"uma solicitação de encerramento de execução do tipo Cancel for postada pelo supervisor")]
        public void QuandoUmaSolicitacaoDeEncerramentoDeExecucaoDoTipoCancelForPostadaPeloSupervisor()
        {
            StepsExecutor.PublishPoisonPillToWorkDispatcher(PoisonPillEffect.Cancel);
        }

        [When(@"uma solicitação de encerramento de execução do tipo Wait for postada pelo supervisor")]
        public void QuandoUmaSolicitacaoDeEncerramentoDeExecucaoDoTipoWaitForPostadaPeloSupervisor()
        {
            StepsExecutor.PublishPoisonPillToWorkDispatcher(PoisonPillEffect.Wait);
        }

        [When(@"uma solicitação de encerramento de execução de um outro despachante for postada pelo supervisor")]
        public void QuandoUmaSolicitacaoDeEncerramentoDeExecucaoDeUmOutroDespachanteForPostadaPeloSupervisor()
        {
            StepsExecutor.PublishPoisonPillToAnotherWorkDispatcher(PoisonPillEffect.Cancel);
        }

        [When(@"uma solicitação de encerramento de execução do tipo Abort for postada pelo supervisor")]
        public void QuandoUmaSolicitacaoDeEncerramentoDeExecucaoDoTipoAbortForPostadaPeloSupervisor()
        {
            StepsExecutor.PublishPoisonPillToWorkDispatcher(PoisonPillEffect.Abort);
        }

        [When(@"os três despachantes de trabalho forem iniciados em um mesmo processo")]
        public void QuandoOsTresDespachantesDeTrabalhoForemIniciadosEmUmMesmoProcesso()
        {
            //StepsExecutor.LoadMatrixJsonFiles();
            //StepsExecutor.CreateMatrixParsingRequests();
            //StepsExecutor.ExecuteWorkDisparchersOnSingleProcess();
        }

        [When(@"os três despachantes de trabalho forem iniciados pelo bootstrapper")]
        public void QuandoOsTresDespachantesDeTrabalhoForemIniciadosPeloBootstrapper()
        {
            //StepsExecutor.LoadMatrixJsonFiles();
            //StepsExecutor.CreateMatrixParsingRequests();
            //StepsExecutor.ExecuteWorkDisparchersWithBoostrapper();
        }

        [When(@"as solicitações de MatrixParsing contendo os cinco arquivos forem publicadas")]
        public void QuandoAsSolicitacoesDeMatrixParsingContendoOsCincoArquivosForemPublicadas()
        {
            //StepsExecutor.PublishMatrixParsingRequests();
        }

        [When(@"o tempo necessário para término do processamento já tiver passado")]
        public void QuandoOTempoNecessarioParaTerminoDoProcessamentoJaTiverPassado()
        {
            StepsExecutor.WaitForReceivedJobCompletionEvents(60, 10);
        }

        #endregion

        #region Thens
        [Then(@"o despachante é capaz de interpretar as configurações de recebimento informadas")]
        public void EntaoODespachanteECapazDeInterpretarAsConfiguracoesDeRecebimentoInformadas()
        {
            StepsExecutor.AssertThatWorkDispatcherHasValidSettings();
        }

        [Then(@"o despachante se inscreve para receber solicitações de trabalho pendentes do tipo A")]
        public void EntaoODespachanteSeInscreveParaReceberSolicitacoesDeTrabalhoPendentesDoTipoA()
        {
            StepsExecutor.AssertThatWorkDispatcherCanReceiveDummyJobs();
        }

        [Then(@"o despachante não é capaz de interpretar as configurações de recebimento informadas")]
        public void EntaoODespachanteNaoECapazDeInterpretarAsConfiguracoesDeRecebimentoInformadas()
        {
            StepsExecutor.AssertThatWorkDispatcherHasInvalidSettings();
        }

        [Then(@"o despachante é capaz de interpretar as configurações de encaminhamento informadas")]
        public void EntaoODespachanteECapazDeInterpretarAsConfiguracoesDeEncaminhamentoInformadas()
        {
            StepsExecutor.AssertThatWorkDispatcherHasValidSettings();
        }

        [Then(@"o despachante deve configurar o tempo máximo de execução na solicitação de trabalho")]
        public void EntaoODespachanteDeveConfigurarOTempoMaximoDeExecucaoNaSolicitacaoDeTrabalho()
        {
            StepsExecutor.AssertOnWorkDispatcherThatRequestTimeoutsForDummyJobAreCorrect();
        }

        [Then(@"o despachante deve se inscrever para receber uma notificação de conclusão de execução da solicitação")]
        public void EntaoODespachanteDeveSeInscreverParaReceberUmaNotificacaoDeConclusaoDeExecucaoDaSolicitacao()
        {
            StepsExecutor.AssertOnWorkDispatcherThatJobIsWaitingForTheCompletionOfTheDummyJob();
        }

        [Then(@"o despachante deve enviar a solicitação de trabalho para o responsável por sua execução")]
        public void EntaoODespachanteDeveEnviarASolicitacaoDeTrabalhoParaOResponsavelPorSuaExecucao()
        {
            StepsExecutor.AssertOnWorkDispatcherThatJobHasDispatchedTheDummyJobForTheDummyWorker();
        }

        //[Then(@"o despachante deve manter a solicitação de trabalho como uma solicitação de trabalho em processamento.")]
        //public void EntaoODespachanteDeveManterASolicitacaoDeTrabalhoComoUmaSolicitacaoDeTrabalhoEmProcessamento_()
        //{
        //    StepsExecutor.AssertOnWorkDispatcherThatTheWorkDispatcherHaveKeptTheDummyJobAsProcessing();
        //}

        [Then(@"o despachante não se inscreve para receber solicitações de trabalho pendentes do tipo A")]
        public void EntaoODespachanteNaoSeInscreveParaReceberSolicitacoesDeTrabalhoPendentesDoTipoA()
        {
            StepsExecutor.AssertThatWorkDispatcherCannotReceiveDummyJobs();
        }

        [Then(@"o despachante é capaz de encaminhar solicitações de trabalho a este responsável informado")]
        public void EntaoODespachanteECapazDeEncaminharSolicitacoesDeTrabalhoAEsteResponsavelInformado()
        {
            StepsExecutor.AssertThatWorkDispatcherCanDispatchDummyJobs();
        }

        [Then(@"o despachante deve mostrar uma mensagem de erro do tipo InvalidOperationException")]
        public void EntaoODespachanteDeveLancarUmaExcecaoDoTipoInvalidOperationException()
        {
            StepsExecutor.AssertThatWorkDispatcherCouldNotBeInitializedDueToInvalidSettings();
        }

        [Then(@"o despachante deve receber a solicitação de trabalho do tipo A recém postada como pendente")]
        public void EntaoODespachanteDeveReceberASolicitacaoDeTrabalhoDoTipoARecemPostadaComoPendente()
        {
            StepsExecutor.AssertOnWorkDispatcherThatTheDummyJobPassedThrough(DispatcherActivity.JobWasReceived);
        }

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

        [Then(@"o despachante não deve registrar a falha no log de operações do despachante de trabalhos")]
        public void EntaoODespachanteNaoDeveRegistrarAFalhaNoLogDeOperacoesDoDespachanteDeTrabalhos()
        {
            StepsExecutor.AssertOnWorkDispatcherThatTheDummyJobHaveNotBeenRepublishedAs(JobStatus.Failed);
        }

        //[Then(@"o agente de execução deve consolidar as alterações de estado realizadas durante a operação")]
        //public void EntaoOAgenteDeExecucaoDeveConsolidarAsAlteracoesDeEstadoRealizadasDuranteAOperacao()
        //{
        //    StepsExecutor.AssertThatDummyJobValueHasChangedDuringExecution();
        //}

        [Then(@"o agente de execução deve sinalizar a operação como concluída com sucesso")]
        public void EntaoOAgenteDeExecucaoDeveSinalizarAOperacaoComoConcluidaComSucesso()
        {
            StepsExecutor.AssertThatDummyJobHasBeenCompletedSuccessfuly();
        }

        [Then(@"o agente de execução deve publicar os eventos de conclusão da operação")]
        public void EntaoOAgenteDeExecucaoDevePublicarOsEventosDeConclusaoDaOperacao()
        {
            StepsExecutor.AssertOnWorkDispatcherThatTheCompletionEventsHaveBeenPublishedByDummyJob();
        }

        //[Then(@"o agente de execução deve cancelar as alterações de estado realizadas durante a operação")]
        //public void EntaoOAgenteDeExecucaoDeveCancelarAsAlteracoesDeEstadoRealizadasDuranteAOperacao()
        //{
        //    StepsExecutor.AssertThatDummyJobValueHasNotChangedDuringExecution();
        //}

        [Then(@"o agente de execução deve sinalizar a operação como concluída com falha e que deva ser repetida")]
        public void EntaoOAgenteDeExecucaoDeveSinalizarAOperacaoComoConcluidaComFalhaEQueDevaSerRepetida()
        {
            StepsExecutor.AssertOnWorkDispatcherThatTheDummyJobHaveBeenRepublishedAs(JobStatus.Failed);
            StepsExecutor.AssertOnWorkDispatcherThatTheDummyJobHaveBeenRepublishedAs(JobStatus.Pending);
        }

        //[Then(@"o agente de execução não deve publicar os eventos de conclusão da operação")]
        //public void EntaoOAgenteDeExecucaoNaoDevePublicarOsEventosDeConclusaoDaOperacao()
        //{
        //    StepsExecutor.AssertOnWorkDispatcherThatNoCompletionEventsHaveBeenPublishedByDummyJob();
        //}

        [Then(@"o agente de execução deve sinalizar a operação como concluída com falha e que deva ser logada como erro")]
        public void EntaoOAgenteDeExecucaoDeveSinalizarAOperacaoComoConcluidaComFalhaEQueDevaSerLogadaComoErro()
        {
            StepsExecutor.AssertOnWorkDispatcherThatTheDummyJobHaveBeenRepublishedAs(JobStatus.Failed);
            StepsExecutor.AssertOnWorkDispatcherThatTheDummyJobHaveNotBeenRepublishedAs(JobStatus.Pending);
        }

        [Then(@"o agente de execução deve sinalizar a operação como concluída com falha e que deva ser ignorada")]
        public void EntaoOAgenteDeExecucaoDeveSinalizarAOperacaoComoConcluidaComFalhaEQueDevaSerIgnorada()
        {
            StepsExecutor.AssertOnWorkDispatcherThatTheDummyJobHaveNotBeenRepublishedAs(JobStatus.Failed);
            StepsExecutor.AssertOnWorkDispatcherThatTheDummyJobHaveNotBeenRepublishedAs(JobStatus.Pending);
        }

        //[Then(@"o agente de execução não deve sinalizar a operação como concluída")]
        //public void EntaoOAgenteDeExecucaoNaoDeveSinalizarAOperacaoComoConcluida()
        //{
        //    StepsExecutor.AssertThatDummyJobWasNotCompleted();
        //}

        [Then(@"o despachante deve receber a solicitação de encerramento de execução")]
        public void EntaoODespachanteDeveReceberASolicitacaoDeEncerramentoDeExecucao()
        {
            StepsExecutor.AssertOnWorkDispatcherThatTheDummyJobPassedThrough(DispatcherActivity.PoisonPillWasReceived);
        }

        [Then(@"o despachante de trabalhos deve encerrar sua execução imediatamente cancelando a operação")]
        public void EntaoODespachanteDeTrabalhosDeveEncerrarSuaExecucaoImediatamenteCancelandoAOperação()
        {
            StepsExecutor.AssertOnWorkDispatcherThatThePoisonPillWasReceivedWithEffectiveness(PoisonPillEffect.Cancel);
            StepsExecutor.AssertThatWorkDispatcherCommittedSuicide();
        }

        [Then(@"o despachante de trabalhos deve encerrar sua execução imediatamente abortando a operação")]
        public void EntaoODespachanteDeTrabalhosDeveEncerrarSuaExecucaoImediatamenteAbortandoAOperação()
        {
            StepsExecutor.AssertOnWorkDispatcherThatThePoisonPillWasReceivedWithEffectiveness(PoisonPillEffect.Abort);
            StepsExecutor.AssertThatWorkDispatcherCommittedSuicide();
        }

        [Then(@"o despachante de trabalhos deve encerrar sua execução aguardando a conclusão da operação")]
        public void EntaoODespachanteDeTrabalhosDeveEncerrarSuaExecucaoAguardandoAConclusãoDaOperação()
        {
            StepsExecutor.AssertOnWorkDispatcherThatThePoisonPillWasReceivedWithEffectiveness(PoisonPillEffect.Wait);
            StepsExecutor.AssertThatWorkDispatcherCommittedSuicide();
        }

        [Then(@"o outro despachante não deve receber a solicitação de encerramento de execução")]
        public void EntaoOOutroDespachanteNaoDeveReceberASolicitacaoDeEncerramentoDeExecucao()
        {
            StepsExecutor.AssertOnAnotherWorkDispatcherThatTheDummyJobHaveNotPassedThrough(DispatcherActivity.PoisonPillWasReceived);
        }

        [Then(@"o outro despachante de trabalhos deve continuar sua execução")]
        public void EntaoOOutroDespachanteDeTrabalhosDeveContinuarSuaExecucao()
        {
            StepsExecutor.AssertThatAnotherWorkDispatcherIsAlive();
        }

        [Then(@"o despachante de trabalhos deve continuar sua execução")]
        public void EntaoODespachanteDeTrabalhosDeveContinuarSuaExecucao()
        {
            StepsExecutor.AssertThatWorkDispatcherIsAlive();
        }

        [Then(@"o despachante de trabalhos deve terminar a execução da solicitação de trabalho")]
        public void EntaoODespachanteDeTrabalhosDeveTerminarAExecucaoDaSolicitacaoDeTrabalho()
        {
            StepsExecutor.AssertOnWorkDispatcherThatTheDummyJobPassedThrough(DispatcherActivity.JobWasCompleted);
        }

        [Then(@"o despachante de trabalhos deve abortar a execução da solicitação de trabalho")]
        public void EntaoODespachanteDeTrabalhosDeveAbortarAExecucaoDaSolicitacaoDeTrabalho()
        {
            StepsExecutor.AssertOnWorkDispatcherThatTheDummyJobPassedThrough(DispatcherActivity.AbortedAllWorkerThreads);
        }

        [Then(@"o despachante não deve ser encerrado")]
        public void EntaoODespachanteNaoDeveSerEncerrado()
        {
            StepsExecutor.AssertThatWorkDispatcherIsAlive();
        }

        [Then(@"o despachante deve ser encerrado")]
        public void EntaoODespachanteDeveSerEncerrado()
        {
            StepsExecutor.AssertThatWorkDispatcherCommittedSuicide();
        }

        [Then(@"cinco eventos do tipo WorkCompleted contendo o resultado das solicitações deverão ser recebidos no tópico MatrixReversed")]
        public void EntaoCincoEventosDoTipoWorkCompletedContendoOResultadoDasSolicitacoesDeveraoSerRecebidosNoTopicoMatrixReversed()
        {
            //StepsExecutor.AssertAllMatrixParsingResultedInAWorkCompletedEvent();
        }

        [Then(@"cada uma dessas WorkCompleted conterá uma MatrixReversing que fora processada por um despachante de trabalho \(não necessariamente uma por despachante\)")]
        public void EntaoCadaUmaDessasWorkCompletedConteraUmaMatrixReversingQueForaProcessadaPorUmDespachanteDeTrabalhoNaoNecessariamenteUmaPorDespachante()
        {
            //StepsExecutor.AssertAllWorkCompletedEventsContainAMatrixReversingRequest();
        }

        [Then(@"cada uma dessas MatrixParsing conterá uma MatrixParsing terá sido processada por um despachante de trabalho \(não necessariamente uma por despachante\)")]
        public void EntaoCadaUmaDessasMatrixParsingConteraUmaMatrixParsingTeraSidoProcessadaPorUmDespachanteDeTrabalhoNaoNecessariamenteUmaPorDespachante()
        {
            //StepsExecutor.AssertAllMatrixReversingRequestsContainAMatrixParsingRequest();
        }

        [Then(@"o resultado contido no evento MatrixReversed resultante de cada MatrixParsing deve ser o resultado esperado")]
        public void EntaoOResultadoContidoNoEventoMatrixReversedResultanteDeCadaMatrixParsingDeveSerOResultadoEsperado()
        {
            //StepsExecutor.AssertAllMatrixReversingResultsAreCorrect();
        }

        #endregion
    }
}
