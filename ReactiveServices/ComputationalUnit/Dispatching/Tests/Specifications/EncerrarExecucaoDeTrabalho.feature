#language: pt-BR

Funcionalidade: Encerrar execução de trabalho
	De modo a dar a resposta correta ao despachante de trabalhos
	Como um agente de execução de trabalhos
	Desejo encerrar a execução dos trabalhos apropriadamente

@stable @fast
Cenário: Encerrar execução de trabalho com sucesso dentro do prazo
	Dado que o agente de execução esteja executando um trabalho
	E que nenhum erro aconteça durante a execução do trabalho
	E que a solicitação de execução do trabalho ainda não tenha expirado ao ser concluída
	Quando a execução do trabalho for concluída
	Então o agente de execução deve sinalizar a operação como concluída com sucesso
	E o agente de execução deve publicar os eventos de conclusão da operação

@stable @slow
Cenário: Encerrar execução de trabalho com falha que implique que a execução deva ser repetida
	Dado que o agente de execução esteja executando um trabalho
	E que algum erro tenha acontecido durante a execução
	E que o erro ocorrido seja do tipo que implique na repetição da execução
	E que a solicitação de execução do trabalho ainda não tenha expirado ao ser concluída
	Quando a execução do trabalho for concluída
	Então o agente de execução deve sinalizar a operação como concluída com falha e que deva ser repetida
	E o agente de execução deve publicar os eventos de conclusão da operação

@stable @fast
Cenário: Encerrar execução de trabalho com falha que implique que a execução deva ser logada como erro
	Dado que o agente de execução esteja executando um trabalho
	E que algum erro tenha acontecido durante a execução
	E que o erro ocorrido seja do tipo que implique no registro do erro em log, sem repetição
	E que a solicitação de execução do trabalho ainda não tenha expirado ao ser concluída
	Quando a execução do trabalho for concluída
	Então o agente de execução deve sinalizar a operação como concluída com falha e que deva ser logada como erro
	E o agente de execução deve publicar os eventos de conclusão da operação

@stable @fast
Cenário: Encerrar execução de trabalho após o prazo ter expirado
	Dado que o agente de execução esteja executando um trabalho
	E que a solicitação de execução do trabalho já tenha expirado ao ser concluída
	Quando a execução do trabalho for concluída
	Então o agente de execução deve sinalizar a operação como concluída com falha e que deva ser logada como erro
	E o agente de execução deve publicar os eventos de conclusão da operação