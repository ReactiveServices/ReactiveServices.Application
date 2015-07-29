using PostSharp.Patterns.Diagnostics;
using TechTalk.SpecFlow;
using FluentAssertions;

namespace ReactiveServices.Application.Launching.Tests.Specifications
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

        [Given(@"que tenha sido solicitada a execução de um aplicativo válido como aplicativo de boostrap")]
        public void DadoQueTenhaSidoSolicitadaAExecucaoDeUmAplicativoValidoComoAplicativoDeBoostrap()
        {
            Context.ConfigureApplicationExecution();
        }

        [Given(@"que tenha sido informado um script de bootstrap válido")]
        public void DadoQueTenhaSidoInformadoUmScriptDeBootstrapValido()
        {
            Context.PrepareBootstrapScript();
        }

        [Given(@"que este script tenha apenas uma solicitação de trabalho")]
        public void DadoQueEsteScriptTenhaApenasUmaSolicitacaoDeTrabalho()
        {
            Context.EnsureScriptHasOnlyOneJob();
        }

        [Given(@"que tenha sido informado um script de bootstrap válido para duas solicitações do mesmo tipo")]
        public void DadoQueTenhaSidoInformadoUmScriptDeBootstrapValidoParaDuasSolicitacoesDoMesmoTipo()
        {
            Context.PrepareBootstrapScriptForTwoJobsOfSameType();
        }


        [When(@"a execução do aplicativo for solicitada")]
        public void QuandoAExecucaoDoAplicativoForSolicitada()
        {
            Context.ExecuteApplication();
        }

        [When(@"o script de bootstrap for executado")]
        public void QuandoOScriptDeBootstrapForExecutado()
        {
            Context.ExecuteBootstrapScript();
        }


        [Then(@"uma mensagem vinda deste aplicativo deve ser recebida")]
        public void EntaoUmaMensagemVindaDesteAplicativoDeveSerRecebida()
        {
            Context.HasReceivedLaunchConfirmation().Should().BeTrue();
        }

        [Then(@"os aplicativos que tal script solitita devem ser executados")]
        public void EntaoOsAplicativosQueTalScriptSolititaDevemSerExecutados()
        {
            Context.HasReceivedLaunchConfirmationFromAllDispatchers().Should().BeTrue();
        }

        [Then(@"as solicitações de trabalho que tal script solitita devem ser executadas")]
        public void EntaoAsSolicitacoesDeTrabalhoQueTalScriptSolititaDevemSerExecutadas()
        {
            Context.AllJobsFromBootstrapScriptHaveStarted().Should().BeTrue();
        }
    }
}
