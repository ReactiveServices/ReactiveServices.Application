#language: pt-BR

Funcionalidade: Verificação de despachantes de trabalho em execução

@stable @fast
Cenário: Constatação de que um despachante de trabalhos não foi inicializado
	Dado que um despachante de trabalhos de identificador 'MonitoringTest1' não seja instanciado
	E que um supervisor seja configurado para monitorar o despachante de trabalhos de identificador 'MonitoringTest1'
	Quando o supervisor for executado
	Então o supervisor deve identificar que o despachante 'MonitoringTest1' não está em execução

@stable @fast
Cenário: Constatação de que um despachante de trabalhos foi inicializado
	Dado que um despachante de trabalhos de identificador 'MonitoringTest2' seja instanciado
	E que um supervisor seja configurado para monitorar o despachante de trabalhos de identificador 'MonitoringTest2'
	Quando o supervisor for executado
	Então o supervisor deve identificar que o despachante 'MonitoringTest2' está em execução

@unstable @slow
Cenário: Constatação de que um despachante de trabalhos não está mais em execução
	Dado que um despachante de trabalhos de identificador 'MonitoringTest3' seja instanciado
	E que um supervisor seja configurado para monitorar o despachante de trabalhos de identificador 'MonitoringTest3'
	Quando o supervisor for executado
	E uma solicitação de encerramento de execução do tipo Kill for postada pelo supervisor para a instância 'MonitoringTest3'
	Então o supervisor deve identificar que o despachante 'MonitoringTest3' não está em execução	