﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.0.77
//      SpecFlow Generator Version:1.9.0.0
//      Runtime Version:4.0.30319.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace ReactiveServices.ComputationalUnit.Dispatching.Tests.Specifications
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Encerrar execução de despachante")]
    public partial class EncerrarExecucaoDeDespachanteFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "EncerrarExecucaoDeDespachante.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("pt-BR"), "Encerrar execução de despachante", "De modo a ser capaz de encerrar a execução de um despachante de trabalhos\r\nComo u" +
                    "m supervisor\r\nEu quero que um despachante de trabalho seja encerrado ao receber " +
                    "minha solicitação de encerramento de execução", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.TestFixtureTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Receber uma solicitação de encerramento de execução do tipo Cancel")]
        [NUnit.Framework.CategoryAttribute("stable")]
        [NUnit.Framework.CategoryAttribute("fast")]
        public virtual void ReceberUmaSolicitacaoDeEncerramentoDeExecucaoDoTipoCancel()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Receber uma solicitação de encerramento de execução do tipo Cancel", new string[] {
                        "stable",
                        "fast"});
#line 9
this.ScenarioSetup(scenarioInfo);
#line 10
 testRunner.Given("que exista um despachante de trabalhos em execução", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Dado ");
#line 11
 testRunner.When("uma solicitação de encerramento de execução do tipo Cancel for postada pelo super" +
                    "visor", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Quando ");
#line 12
 testRunner.Then("o despachante deve receber a solicitação de encerramento de execução", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Então ");
#line 13
 testRunner.And("o despachante de trabalhos deve encerrar sua execução imediatamente cancelando a " +
                    "operação", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "E ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Receber uma solicitação de encerramento de execução do tipo Wait")]
        [NUnit.Framework.CategoryAttribute("stable")]
        [NUnit.Framework.CategoryAttribute("slow")]
        public virtual void ReceberUmaSolicitacaoDeEncerramentoDeExecucaoDoTipoWait()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Receber uma solicitação de encerramento de execução do tipo Wait", new string[] {
                        "stable",
                        "slow"});
#line 16
this.ScenarioSetup(scenarioInfo);
#line 17
 testRunner.Given("que exista um despachante de trabalhos em execução", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Dado ");
#line 18
 testRunner.When("uma solicitação de encerramento de execução do tipo Wait for postada pelo supervi" +
                    "sor", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Quando ");
#line 19
 testRunner.Then("o despachante deve receber a solicitação de encerramento de execução", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Então ");
#line 20
 testRunner.And("o despachante de trabalhos deve encerrar sua execução aguardando a conclusão da o" +
                    "peração", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "E ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Enviar uma solicitação de encerramento de execução do tipo Cancel com dois despac" +
            "hantes")]
        [NUnit.Framework.CategoryAttribute("stable")]
        [NUnit.Framework.CategoryAttribute("fast")]
        public virtual void EnviarUmaSolicitacaoDeEncerramentoDeExecucaoDoTipoCancelComDoisDespachantes()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Enviar uma solicitação de encerramento de execução do tipo Cancel com dois despac" +
                    "hantes", new string[] {
                        "stable",
                        "fast"});
#line 23
this.ScenarioSetup(scenarioInfo);
#line 24
 testRunner.Given("que exista um despachante de trabalhos em execução", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Dado ");
#line 25
 testRunner.And("que exista um outro despachante de trabalhos em execução", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "E ");
#line 26
 testRunner.When("uma solicitação de encerramento de execução do tipo Cancel for postada pelo super" +
                    "visor", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Quando ");
#line 27
 testRunner.Then("o despachante deve receber a solicitação de encerramento de execução", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Então ");
#line 28
 testRunner.Then("o outro despachante não deve receber a solicitação de encerramento de execução", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Então ");
#line 29
 testRunner.And("o despachante de trabalhos deve encerrar sua execução imediatamente cancelando a " +
                    "operação", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "E ");
#line 30
 testRunner.And("o outro despachante de trabalhos deve continuar sua execução", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "E ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Enviar uma solicitação de encerramento de execução do tipo Wait com dois despacha" +
            "ntes")]
        [NUnit.Framework.CategoryAttribute("stable")]
        [NUnit.Framework.CategoryAttribute("slow")]
        public virtual void EnviarUmaSolicitacaoDeEncerramentoDeExecucaoDoTipoWaitComDoisDespachantes()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Enviar uma solicitação de encerramento de execução do tipo Wait com dois despacha" +
                    "ntes", new string[] {
                        "stable",
                        "slow"});
#line 33
this.ScenarioSetup(scenarioInfo);
#line 34
 testRunner.Given("que exista um despachante de trabalhos em execução", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Dado ");
#line 35
 testRunner.And("que exista um outro despachante de trabalhos em execução", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "E ");
#line 36
 testRunner.When("uma solicitação de encerramento de execução do tipo Wait for postada pelo supervi" +
                    "sor", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Quando ");
#line 37
 testRunner.Then("o despachante deve receber a solicitação de encerramento de execução", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Então ");
#line 38
 testRunner.Then("o outro despachante não deve receber a solicitação de encerramento de execução", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Então ");
#line 39
 testRunner.And("o despachante de trabalhos deve encerrar sua execução aguardando a conclusão da o" +
                    "peração", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "E ");
#line 40
 testRunner.And("o outro despachante de trabalhos deve continuar sua execução", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "E ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Receber uma solicitação de encerramento de execução do tipo Cancel durante a exec" +
            "ução de um trabalho")]
        [NUnit.Framework.CategoryAttribute("stable")]
        [NUnit.Framework.CategoryAttribute("fast")]
        public virtual void ReceberUmaSolicitacaoDeEncerramentoDeExecucaoDoTipoCancelDuranteAExecucaoDeUmTrabalho()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Receber uma solicitação de encerramento de execução do tipo Cancel durante a exec" +
                    "ução de um trabalho", new string[] {
                        "stable",
                        "fast"});
#line 43
this.ScenarioSetup(scenarioInfo);
#line 44
 testRunner.Given("que exista um despachante de trabalhos em execução", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Dado ");
#line 45
 testRunner.And("que o despachante de trabalhos tenha recebido uma solicitação de trabalho com dur" +
                    "ação de 5 segundos", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "E ");
#line 46
 testRunner.When("uma solicitação de encerramento de execução do tipo Cancel for postada pelo super" +
                    "visor", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Quando ");
#line 47
 testRunner.Then("o despachante deve receber a solicitação de encerramento de execução", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Então ");
#line 48
 testRunner.And("o despachante de trabalhos deve encerrar sua execução imediatamente cancelando a " +
                    "operação", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "E ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Receber uma solicitação de encerramento de execução do tipo Wait durante a execuç" +
            "ão de um trabalho")]
        [NUnit.Framework.CategoryAttribute("stable")]
        [NUnit.Framework.CategoryAttribute("slow")]
        public virtual void ReceberUmaSolicitacaoDeEncerramentoDeExecucaoDoTipoWaitDuranteAExecucaoDeUmTrabalho()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Receber uma solicitação de encerramento de execução do tipo Wait durante a execuç" +
                    "ão de um trabalho", new string[] {
                        "stable",
                        "slow"});
#line 51
this.ScenarioSetup(scenarioInfo);
#line 52
 testRunner.Given("que exista um despachante de trabalhos em execução", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Dado ");
#line 53
 testRunner.And("que o despachante de trabalhos tenha recebido uma solicitação de trabalho com dur" +
                    "ação de 5 segundos", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "E ");
#line 54
 testRunner.When("uma solicitação de encerramento de execução do tipo Wait for postada pelo supervi" +
                    "sor", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Quando ");
#line 55
 testRunner.Then("o despachante deve receber a solicitação de encerramento de execução", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Então ");
#line 56
 testRunner.And("o despachante de trabalhos deve terminar a execução da solicitação de trabalho", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "E ");
#line 57
 testRunner.And("o despachante de trabalhos deve encerrar sua execução aguardando a conclusão da o" +
                    "peração", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "E ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Receber uma solicitação de encerramento de execução do tipo Abort durante a execu" +
            "ção de um trabalho")]
        [NUnit.Framework.CategoryAttribute("stable")]
        [NUnit.Framework.CategoryAttribute("fast")]
        public virtual void ReceberUmaSolicitacaoDeEncerramentoDeExecucaoDoTipoAbortDuranteAExecucaoDeUmTrabalho()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Receber uma solicitação de encerramento de execução do tipo Abort durante a execu" +
                    "ção de um trabalho", new string[] {
                        "stable",
                        "fast"});
#line 60
this.ScenarioSetup(scenarioInfo);
#line 61
 testRunner.Given("que exista um despachante de trabalhos em execução", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Dado ");
#line 62
 testRunner.And("que o despachante de trabalhos tenha recebido uma solicitação de trabalho com dur" +
                    "ação de 15 segundos", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "E ");
#line 63
 testRunner.When("uma solicitação de encerramento de execução do tipo Abort for postada pelo superv" +
                    "isor", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Quando ");
#line 64
 testRunner.Then("o despachante deve receber a solicitação de encerramento de execução", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Então ");
#line 65
 testRunner.And("o despachante de trabalhos deve abortar a execução da solicitação de trabalho", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "E ");
#line 66
 testRunner.And("o despachante de trabalhos deve encerrar sua execução imediatamente abortando a o" +
                    "peração", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "E ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
