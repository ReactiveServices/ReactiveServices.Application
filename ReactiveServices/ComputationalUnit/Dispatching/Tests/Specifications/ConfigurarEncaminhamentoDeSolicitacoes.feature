#language:pt-BR

Funcionalidade: Configurar o encaminhamento das solicitações de trabalho recebidos
	De modo a ser capaz de encaminhar trabalhos para serem processados
	Como um despachante de trabalhos
	Eu quero me configurar para identificar a quem devo encaminhar as solicitações de trabalho que receber

@stable
@TO_REMOVE_ConfigureAndIntializeDispatcher
Cenário: Verificar se será possível encaminhar solicitações de trabalho ao responsável informado
	Dado que tenham sido informados parametros de configuração solicitando o encaminhamento de solicitações de trabalho a um dado responsável pelo processamento
	Quando o despachante de trabalhos for inicializado
	Então o despachante é capaz de interpretar as configurações de encaminhamento informadas
	E o despachante é capaz de encaminhar solicitações de trabalho a este responsável informado
	
