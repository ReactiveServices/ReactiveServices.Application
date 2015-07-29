#language:pt-BR

Funcionalidade: Balanceamento de Carga entre Despachantes
	De modo a ter minhas solicitações de trabalho processadas de maneira mais rápida e segura
	Como um solicitante de trabalhos
	Eu quero que múltiplos despachantes de trabalho atuem ao mesmo tempo para atender as minhas solicitações

@stable @fast
Cenário: Encaminhar uma solicitação de trabalho para dois despachantes
	Dado que dois despachantes de trabalhos tenham sido configurados para receber solicitacoes pendentes do tipo A
	Quando uma solicitação de trabalho do tipo A for postada como pendente
	Então apenas um dos despachantes deve receber a solicitação de trabalho do tipo A recém postada como pendente

@stable @fast
Cenário: Encaminhar duas solicitações de trabalho para dois despachantes
	Dado que dois despachantes de trabalhos tenham sido configurados para receber solicitacoes pendentes do tipo A
	Quando duas solicitações de trabalho do tipo A forem postadas como pendente
	Então um dos despachantes de trabalhos irá processar a primeira solicitação A recebida
	E o outro despachante de trabalhos irá processar a segunda solicitação A

@stable @fast
Cenário: Encaminhar duas solicitações de trabalho de tipos diferentes para dois despachantes
	Dado que um despachante de trabalhos tenha sido configurado para receber solicitacoes pendentes do tipo A
	Dado que outro despachante de trabalhos tenha sido configurado para receber solicitacoes pendentes do tipo B
	Quando uma solicitação de trabalho do tipo A for postada como pendente
	E uma solicitação de trabalho do tipo B for postada como pendente
	Então o primeiro despachante deve receber a solicitação de trabalho do tipo A recém postada como pendente
	E o segundo despachante deve receber a solicitação de trabalho do tipo B recém postada como pendente

@stable @fast
Cenário: Verificar que há uma solicitação de trabalho postada por outro despachante e marcada como em andamento por mais tempo que o permitido
	Dado que uma solicitação de trabalho tenha sido encaminhada para execução por um outro despachante de trabalhos
	E que a solicitação de trabalho tenha sido marcada com um tempo máximo de processamento de 4 segundos
	E que a solicitação de trabalho tenha sido marcada como em andamento há 12 segundos atrás
	E que exista um despachante de trabalhos ocioso
	Quando o outro despachante de trabalhos verificar que a solicitação de trabalho expirou
	Então o outro despachante deve republicar a solicitação de trabalho como pendente

@stable @slow
Esquema do Cenário: Múltiplas solicitações de trabalho concorrentes
	Dado que <n> despachantes tenham se inscrito para processar solicitações de trabalho do tipo A
	Quando <n> solicitações de trabalho do tipo A forem enviadas para processamento
	Entao ao menos 2 dos <n> despachantes de trabalho devem ser acionados para atendimento às solicitacoes

	Exemplos: 
	| n		|
	| 2		|
	| 15	|

@stable @slow
Cenário: Encaminhar duas solicitações de trabalho para um despachante que processa somente uma solicitação por vez
	Dado que um despachante de trabalhos tenha sido configurado para processar 1 solicitação de trabalho do tipo A
	Quando 2 solicitações de trabalho do tipo A forem enviadas para processamento
	Então o despachante de trabalhos deve completar as 2 solicitações de trabalho
	E a primeira solicitação de trabalho deve ter sido completada antes que a segunda tenha sido recebida