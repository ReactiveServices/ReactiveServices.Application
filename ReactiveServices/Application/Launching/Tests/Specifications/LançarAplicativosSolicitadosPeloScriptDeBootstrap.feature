#language: pt-BR

Funcionalidade: Lançar aplicativos solicitados pelo script de bootstrap

@stable @fast
Cenário: Lançar um aplicativo solicitado por um script de bootstrap
	Dado que tenha sido solicitada a execução de um aplicativo válido como aplicativo de boostrap
	Quando a execução do aplicativo for solicitada
	Então uma mensagem vinda deste aplicativo deve ser recebida

@stable @fast
Cenário: Lançar aplicativos solicitados pelo script de bootstrap
	Dado que tenha sido informado um script de bootstrap válido
	Quando o script de bootstrap for executado
	Então os aplicativos que tal script solitita devem ser executados

@stable @fast
Cenário: Disparar um solicitação de trabalho solicitadas pelo script de boostrap
	Dado que tenha sido informado um script de bootstrap válido
	E que este script tenha apenas uma solicitação de trabalho
	Quando o script de bootstrap for executado
	Então as solicitações de trabalho que tal script solitita devem ser executadas

@stable @fast
Cenário: Disparar duas solicitações de trabalho solicitadas pelo script de boostrap
	Dado que tenha sido informado um script de bootstrap válido para duas solicitações do mesmo tipo
	Quando o script de bootstrap for executado
	Então as solicitações de trabalho que tal script solitita devem ser executadas
