#language: pt-BR

Funcionalidade: Levantamento de despachantes de trabalho que deveriam estar em execução

@stable @fast
Cenário: Levantamento de um despachante de trabalhos que deveria estar em execução
	Dado que as configurações solicitem a execução de uma instância do despachante de trabalhos a ser identificada como despachante 'RestorationTest1'
	E que as configurações informem que o despachante 'RestorationTest1' será capaz de processar até 1 solicitações de trabalho por vez
	E que as configurações informem que o despachante 'RestorationTest1' será capaz de processar solicitações de trabalho do tipo 'Worker'
	E que as configurações solicitem a execução de 1 solicitações de trabalho do tipo 'Worker' na partida do sistema
	E que um supervisor seja configurado para monitorar o despachante de trabalhos de identificador 'RestorationTest1'
	Quando o supervisor for executado
	Então o supervisor deve colocar o despachante 'RestorationTest1' em execução pela primeira vez

@stable @slow
Cenário: Levantamento de um despachante de trabalhos que não está mais em execução
	Dado que as configurações solicitem a execução de uma instância do despachante de trabalhos a ser identificada como despachante 'RestorationTest2'
	E que as configurações informem que o despachante 'RestorationTest2' será capaz de processar até 1 solicitações de trabalho por vez
	E que as configurações informem que o despachante 'RestorationTest2' será capaz de processar solicitações de trabalho do tipo 'Worker'
	E que as configurações solicitem a execução de 1 solicitações de trabalho do tipo 'Worker' na partida do sistema
	E que um supervisor seja configurado para monitorar o despachante de trabalhos de identificador 'RestorationTest2'
	Quando o supervisor for executado
	E uma solicitação de encerramento de execução do tipo Kill for postada para a instância 'RestorationTest2'
	Então o supervisor deve colocar o despachante 'RestorationTest2' novamente em execução

@stable @slow
Cenário: Levantamento de um despachante de trabalhos que não está mais em execução, com um listener
	Dado que as configurações solicitem a execução de uma instância do despachante de trabalhos a ser identificada como despachante 'RestorationTest3'
	E que as configurações informem que o despachante 'RestorationTest3' será capaz de processar até 1 solicitações de trabalho por vez
	E que as configurações informem que o despachante 'RestorationTest3' será capaz de processar solicitações de trabalho do tipo 'Listener'
	E que as configurações solicitem a execução de 1 solicitações de trabalho do tipo 'Listener' na partida do sistema
	E que um supervisor seja configurado para monitorar o despachante de trabalhos de identificador 'RestorationTest3'
	Quando o supervisor for executado
	E uma solicitação de encerramento de execução do tipo Kill for postada para a instância 'RestorationTest3'
	Então o supervisor deve colocar o despachante 'RestorationTest3' novamente em execução
	