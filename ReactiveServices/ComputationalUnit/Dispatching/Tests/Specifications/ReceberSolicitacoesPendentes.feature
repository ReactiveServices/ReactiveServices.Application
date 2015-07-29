#language:pt-BR

Funcionalidade: Receber Solicitações Pendentes
	De modo a ser capaz de encaminhar trabalhos para serem processados
	Como um despachante de trabalhos
	Eu quero receber as solicitações de trabalho pendentes

@stable
@TO_REMOVE_ConfigureAndIntializeDispatcher
Cenário: Receber uma solicitação de trabalho de um determinado tipo
	Dado que o despachante de trabalhos tenha sido configurado para receber solicitacoes pendentes do tipo A
	Quando uma solicitação de trabalho do tipo A for postada como pendente
	Então o despachante deve receber a solicitação de trabalho do tipo A recém postada como pendente