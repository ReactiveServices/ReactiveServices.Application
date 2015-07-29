#language:pt-BR

Funcionalidade: Configurar recebimento de solicitações de trabalho pendentes
	De modo a ser capaz de encaminhar trabalhos para serem processados
	Como um despachante de trabalhos
	Eu quero me configurar para receber as solicitações de trabalho pendentes conforme indicado nos parametros que me forem passados

@stable
@TO_REMOVE_ConfigureAndIntializeDispatcher
Cenário: Se configurar para receber mensagens de um dado tipo com sucesso
	Dado que tenham sido informados parametros corretos de configuração solicitando o recebimento de solicitações de trabalho do tipo A
	Quando o despachante de trabalhos for inicializado
	Então o despachante é capaz de interpretar as configurações de recebimento informadas
	E o despachante se inscreve para receber solicitações de trabalho pendentes do tipo A

@stable
@TO_REMOVE_ConfigureAndIntializeDispatcher
Cenário: Se configurar para receber mensagens de um dado tipo com configuração inválida
	Dado que tenham sido informados parametros incorretos de configuração solicitando o recebimento de solicitações de trabalho do tipo A
	Quando o despachante de trabalhos for inicializado
	Então o despachante não é capaz de interpretar as configurações de recebimento informadas
	E o despachante não se inscreve para receber solicitações de trabalho pendentes do tipo A
	E o despachante deve mostrar uma mensagem de erro do tipo InvalidOperationException
