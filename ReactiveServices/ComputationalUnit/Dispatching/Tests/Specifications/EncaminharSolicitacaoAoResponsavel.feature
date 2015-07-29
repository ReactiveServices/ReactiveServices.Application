#language:pt-BR

Funcionalidade: Encaminhar solicitacoes para o responsável
	De modo a ter uma solicitação de trabalho processada
	Como um despachante de trabalhos
	Eu quero encaminhar as solicitações de trabalho aos responsáveis pelo processamento

@stable
@TO_REMOVE_ConfigureAndIntializeDispatcher
Cenário: Encaminhar uma solicitação de trabalho de um determinado tipo
	Dado que o despachante de trabalhos tenha sido configurado para encaminhar mensagens do tipo A para um dado responsável
	E que a solicitação de trabalho precise de 10 segundos para ser concluída
	E que a solicitação de trabalho tenha sido marcada como em andamento há 1 segundos atrás
	Quando o despachante de trabalhos for inicializado
	E uma solicitação de trabalho do tipo A for recebida pelo despachante de trabalhos
	Então o despachante deve configurar o tempo máximo de execução na solicitação de trabalho
	E o despachante deve se inscrever para receber uma notificação de conclusão de execução da solicitação
	E o despachante deve enviar a solicitação de trabalho para o responsável por sua execução