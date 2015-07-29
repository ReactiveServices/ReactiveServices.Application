#language: pt-BR

Funcionalidade: Encerrar aplicativos solicitados pelo script de bootstrap

@stable @fast
Cenário: Encerrar um aplicativo solicitado por um script de bootstrap
	Dado que tenha sido solicitada a execução de um sistema de serviços através de um script de bootstrap
	E que este sistema de serviços esteja em execução
	Quando o encerramento desse sistema for solicitado
	E tiver passado tempo suficiente para que todos os serviços se encerrem
	Então todos os serviços que compõem esse sistema devem ser encerrados