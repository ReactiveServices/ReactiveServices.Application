#language:pt-BR

Funcionalidade: Encerrar execução de despachante
	De modo a ser capaz de encerrar a execução de um despachante de trabalhos
	Como um supervisor
	Eu quero que um despachante de trabalho seja encerrado ao receber minha solicitação de encerramento de execução

@stable @fast
Cenário: Receber uma solicitação de encerramento de execução do tipo Cancel
	Dado que exista um despachante de trabalhos em execução
	Quando uma solicitação de encerramento de execução do tipo Cancel for postada pelo supervisor
	Então o despachante deve receber a solicitação de encerramento de execução
	E o despachante de trabalhos deve encerrar sua execução imediatamente cancelando a operação

@stable @slow
Cenário: Receber uma solicitação de encerramento de execução do tipo Wait
	Dado que exista um despachante de trabalhos em execução
	Quando uma solicitação de encerramento de execução do tipo Wait for postada pelo supervisor
	Então o despachante deve receber a solicitação de encerramento de execução
	E o despachante de trabalhos deve encerrar sua execução aguardando a conclusão da operação

@stable @fast
Cenário: Enviar uma solicitação de encerramento de execução do tipo Cancel com dois despachantes
	Dado que exista um despachante de trabalhos em execução
	E que exista um outro despachante de trabalhos em execução
	Quando uma solicitação de encerramento de execução do tipo Cancel for postada pelo supervisor
	Então o despachante deve receber a solicitação de encerramento de execução
	Então o outro despachante não deve receber a solicitação de encerramento de execução
	E o despachante de trabalhos deve encerrar sua execução imediatamente cancelando a operação
	E o outro despachante de trabalhos deve continuar sua execução

@stable @slow
Cenário: Enviar uma solicitação de encerramento de execução do tipo Wait com dois despachantes
	Dado que exista um despachante de trabalhos em execução
	E que exista um outro despachante de trabalhos em execução
	Quando uma solicitação de encerramento de execução do tipo Wait for postada pelo supervisor
	Então o despachante deve receber a solicitação de encerramento de execução
	Então o outro despachante não deve receber a solicitação de encerramento de execução
	E o despachante de trabalhos deve encerrar sua execução aguardando a conclusão da operação
	E o outro despachante de trabalhos deve continuar sua execução

@stable @fast
Cenário: Receber uma solicitação de encerramento de execução do tipo Cancel durante a execução de um trabalho
	Dado que exista um despachante de trabalhos em execução
	E que o despachante de trabalhos tenha recebido uma solicitação de trabalho com duração de 5 segundos
	Quando uma solicitação de encerramento de execução do tipo Cancel for postada pelo supervisor
	Então o despachante deve receber a solicitação de encerramento de execução
	E o despachante de trabalhos deve encerrar sua execução imediatamente cancelando a operação

@stable @slow
Cenário: Receber uma solicitação de encerramento de execução do tipo Wait durante a execução de um trabalho
	Dado que exista um despachante de trabalhos em execução
	E que o despachante de trabalhos tenha recebido uma solicitação de trabalho com duração de 5 segundos
	Quando uma solicitação de encerramento de execução do tipo Wait for postada pelo supervisor
	Então o despachante deve receber a solicitação de encerramento de execução
	E o despachante de trabalhos deve terminar a execução da solicitação de trabalho
	E o despachante de trabalhos deve encerrar sua execução aguardando a conclusão da operação

@stable @fast
Cenário: Receber uma solicitação de encerramento de execução do tipo Abort durante a execução de um trabalho
	Dado que exista um despachante de trabalhos em execução
	E que o despachante de trabalhos tenha recebido uma solicitação de trabalho com duração de 15 segundos
	Quando uma solicitação de encerramento de execução do tipo Abort for postada pelo supervisor
	Então o despachante deve receber a solicitação de encerramento de execução
	E o despachante de trabalhos deve abortar a execução da solicitação de trabalho
	E o despachante de trabalhos deve encerrar sua execução imediatamente abortando a operação