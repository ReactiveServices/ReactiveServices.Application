using FluentAssertions;
using MessageBus.Infrastructure;
using MessageBus.Infrastructure.InMemory;
using System.Linq;
using System.Threading;
using TechTalk.SpecFlow;
using WorkDispatcher.Infrastructure.Tests.Specifications;

namespace WorkDispatcher.Infrastructure.Tests
{
    [Binding]
    [Scope(Feature = "Receber notificações de conclusão de processamento")]
    public class ReceberNotificacoesDeConclusaoDeProcessamentoSteps
    {
        static Dispatcher WorkDispatcher = null;
        static Settings Settings;
        static PublishingBusBuilder PublishingBusBuilder = null;

        [AfterScenario("ReceberNotificacoesDeConclusaoDeProcessamentoSteps_Cenario1")]
        [AfterScenario("ReceberNotificacoesDeConclusaoDeProcessamentoSteps_Cenario2")]
        [AfterScenario("ReceberNotificacoesDeConclusaoDeProcessamentoSteps_Cenario3")]
        [AfterScenario("ReceberNotificacoesDeConclusaoDeProcessamentoSteps_Cenario4")]
        public static void TearDown()
        {
            WorkDispatcher.Dispose();
            //StepsExecutor.TearDown();
        }

        [Given(@"que o despachante tenha se inscrevido para receber notificações de conclusão de processamento para uma solicitação de trabalho do tipo '(.*)' com identificador '(.*)'")]
        public void DadoQueODespachanteTenhaSeInscrevidoParaReceberNotificacoesDeConclusaoDeProcessamentoParaUmaSolicitacaoDeTrabalhoDoTipoComIdentificador(string p0, string p1)
        {
            Settings = SettingsBuilder.Build();
            var inMemorySet = new InMemorySet();

            var subscriptionBusBuilder = new SubscriptionBusBuilder()
                .WithConnectionString(Settings.ConnectionString)
                //.WithImplementation(typeof(InMemorySingleProcessSubscriptionBus)).WithConstructorParameters(inMemorySet);
                .WithImplementation(typeof(EasyNetQSubscriptionBus));

            PublishingBusBuilder = new PublishingBusBuilder()
                .WithConnectionString(Settings.ConnectionString)
                //.WithImplementation(typeof(InMemorySingleProcessPublishingBus)).WithConstructorParameters(inMemorySet)
                .WithImplementation(typeof(EasyNetQPublishingBus))
                .WithPublishConfirms();

            var dispatcherLog = new DispatcherLog();
            WorkDispatcher = new Dispatcher(dispatcherLog, subscriptionBusBuilder, PublishingBusBuilder);

            Settings.RoutingTable[0].RequestMaxAttempts = 2;

            WorkDispatcher.LoadSettings(Settings);
            WorkDispatcher.Initialize();
        }

        [When(@"for recebida uma notificação informando que uma solicitação de trabalho do tipo '(.*)' com identificador '(.*)' foi executada com sucesso")]
        public void QuandoForRecebidaUmaNotificacaoInformandoQueUmaSolicitacaoDeTrabalhoDoTipoComIdentificadorFoiExecutadaComSucesso(string p0, string p1)
        {
            using (var publishingBus = PublishingBusBuilder.Build())
            {
                var job = new DummyJob
                {
                    MessageId = MessageId.New(),
                    WorkDurationInMilliseconds = 0,
                    ExpectedExecutionStatus = JobStatus.Succeeded
                };
                publishingBus.Publish(JobStatus.Pending.TopicId(), job);
            }

            Thread.Sleep(5000);
        }

        [When(@"for recebida uma notificação informando que uma solicitação de trabalho do tipo '(.*)' com identificador '(.*)' foi executada com falha e deve ser repetida")]
        public void QuandoForRecebidaUmaNotificacaoInformandoQueUmaSolicitacaoDeTrabalhoDoTipoComIdentificadorFoiExecutadaComFalhaEDeveSerRepetida(string p0, string p1)
        {
            using (var publishingBus = PublishingBusBuilder.Build())
            {
                var job = new DummyJob
                {
                    MessageId = MessageId.New(),
                    ExpectedExecutionStatus = JobStatus.Failed,
                    FailureAction = JobFailureAction.Repeat
                };
                publishingBus.Publish(JobStatus.Pending.TopicId(), job);
            }

            Thread.Sleep(5000);
        }

        [When(@"for recebida uma notificação informando que uma solicitação de trabalho do tipo '(.*)' com identificador '(.*)' foi executada com falha e deve ser ignorada")]
        public void QuandoForRecebidaUmaNotificacaoInformandoQueUmaSolicitacaoDeTrabalhoDoTipoComIdentificadorFoiExecutadaComFalhaEDeveSerIgnorada(string p0, string p1)
        {
            using (var publishingBus = PublishingBusBuilder.Build())
            {
                var job = new DummyJob
                {
                    MessageId = MessageId.New(),
                    ExpectedExecutionStatus = JobStatus.Failed,
                    FailureAction = JobFailureAction.Ignore
                };
                publishingBus.Publish(JobStatus.Pending.TopicId(), job);
            }

            Thread.Sleep(5000);
        }

        [When(@"for recebida uma notificação informando que uma solicitação de trabalho do tipo '(.*)' com identificador '(.*)' foi executada com falha e deve ser logada como erro")]
        public void QuandoForRecebidaUmaNotificacaoInformandoQueUmaSolicitacaoDeTrabalhoDoTipoComIdentificadorFoiExecutadaComFalhaEDeveSerLogadaComoErro(string p0, string p1)
        {
            using (var publishingBus = PublishingBusBuilder.Build())
            {
                var job = new DummyJob
                {
                    MessageId = MessageId.New(),
                    ExpectedExecutionStatus = JobStatus.Failed,
                    FailureAction = JobFailureAction.Log
                };
                publishingBus.Publish(JobStatus.Pending.TopicId(), job);
            }

            Thread.Sleep(5000);
        }

        [Then(@"o despachante deve remover a solicitação da lista de trabalhos em andamento")]
        public void EntaoODespachanteDeveRemoverASolicitacaoDaListaDeTrabalhosEmAndamento()
        {
            var log = WorkDispatcher.DispatcherLog.First(l => l.Activity == DispatcherActivity.RequestWorkerFinalized);
            log.Should().NotBeNull();
        }

        [Then(@"o despachante deve republicar a solicitacao de trabalho como pendente")]
        public void EntaoODespachanteDeveRepublicarASolicitacaoDeTrabalhoComoPendente()
        {
            var log = WorkDispatcher.DispatcherLog.First(l => l.Activity == DispatcherActivity.RequestRepublished && l.OldStatus == JobStatus.Failed && l.NewStatus == JobStatus.Pending);
            log.Should().NotBeNull();
        }

        [Then(@"o despachante deve republicar a solicitacao de trabalho como mal sucedida")]
        public void EntaoODespachanteDeveRepublicarASolicitacaoDeTrabalhoComoMalSucedida()
        {
            var log = WorkDispatcher.DispatcherLog.First(l => l.Activity == DispatcherActivity.RequestRepublished && l.OldStatus == JobStatus.Failed && l.NewStatus == JobStatus.Failed);
            log.Should().NotBeNull();
        }
    }
}
