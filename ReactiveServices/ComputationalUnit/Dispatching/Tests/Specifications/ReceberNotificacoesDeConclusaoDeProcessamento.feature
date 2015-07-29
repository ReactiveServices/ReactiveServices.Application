#language:pt-BR

Funcionalidade: Receber notificações de conclusão de processamento
	De modo a saber se uma solicitação de trabalho enviada para processamento deve ser reenviada para uma segunda tentativa de processamento
	Como um despachante de trabalhos
	Eu quero receber notificações informando se o processamento foi bem sucedido, mal sucedido e deve ser repetido ou mal sucedido e deve ser logado como erro

@stable
@TO_REMOVE_ConfigureAndIntializeDispatcher
Cenário: Receber uma notificação informando que uma solicitação de trabalho foi executada com sucesso
	Dado que o despachante tenha se inscrito para receber notificações de conclusão de processamento para uma solicitação de trabalho do tipo A com identificador X
	E que uma solicitação de trabalho tenha sido marcada com um tempo máximo de processamento de 10 segundos
	E que a solicitação de trabalho precise de 9 segundos para ser concluída
	E que a solicitação de trabalho tenha sido marcada como em andamento há 30 segundos atrás
	Quando for recebida uma notificação informando que uma solicitação de trabalho do tipo A com identificador X foi executada com sucesso
	Então o despachante deve remover a solicitação da lista de trabalhos em andamento

@stable
@TO_REMOVE_ConfigureAndIntializeDispatcher
Cenário: Receber uma notificação informando que uma solicitação de trabalho foi executada com falha e deve ser repetida
	Dado que o despachante tenha se inscrito para receber notificações de conclusão de processamento para uma solicitação de trabalho do tipo A com identificador X
	E que uma solicitação de trabalho tenha sido marcada com um tempo máximo de processamento de 10 segundos
	E que a solicitação de trabalho precise de 9 segundos para ser concluída
	E que a solicitação de trabalho tenha sido marcada como em andamento há 30 segundos atrás
	Quando for recebida uma notificação informando que uma solicitação de trabalho do tipo A com identificador X foi executada com falha e deve ser repetida
	Então o despachante deve remover a solicitação da lista de trabalhos em andamento
	E o despachante deve republicar a solicitação de trabalho como pendente

@stable
@TO_REMOVE_ConfigureAndIntializeDispatcher
Cenário: Receber uma notificação informando que uma solicitação de trabalho foi executada com falha e deve ser logada como erro
	Dado que o despachante tenha se inscrito para receber notificações de conclusão de processamento para uma solicitação de trabalho do tipo A com identificador X
	E que uma solicitação de trabalho tenha sido marcada com um tempo máximo de processamento de 10 segundos
	E que a solicitação de trabalho precise de 9 segundos para ser concluída
	E que a solicitação de trabalho tenha sido marcada como em andamento há 30 segundos atrás
	Quando for recebida uma notificação informando que uma solicitação de trabalho do tipo A com identificador X foi executada com falha e deve ser logada como erro
	Então o despachante deve remover a solicitação da lista de trabalhos em andamento
	E o despachante deve republicar a solicitação de trabalho como mal sucedida
