#language:pt-BR

Funcionalidade: Verificar solicitações não concluídas
	De modo a ser capaz de enviar as solicitações de trabalho cujo processamento foi mal sucedido para uma nova tentativa de processamento
	Como um despachante de trabalhos
	Eu quero verificar se há solicitações de trabalho em processamento por mais tempo que o permitido

@stable @slow
Cenário: Verificar que há uma solicitação de trabalho marcada como em andamento por mais tempo que o permitido com repetição em caso de erro
	Dado que uma solicitação de trabalho tenha sido marcada com um tempo máximo de processamento de 8 segundos
	E que a solicitação de trabalho aceite ser repetida caso falhe
	E que a solicitação de trabalho precise de 4 segundos para ser concluída
	E que a solicitação de trabalho apresente alguma falha durante sua execução
	E que a solicitação de trabalho tenha sido marcada como em andamento há 24 segundos atrás
	Quando o despachante de trabalhos verificar a lista de trabalhos em andamento
	Então o despachante deve remover a solicitação da lista de trabalhos em andamento
	E o despachante deve republicar a solicitação de trabalho como pendente

@stable @slow
Cenário: Verificar que há uma solicitação de trabalho marcada como em andamento por mais tempo que o permitido com repetição em caso de timeout
	Dado que uma solicitação de trabalho tenha sido marcada com um tempo máximo de processamento de 8 segundos
	E que a solicitação de trabalho aceite ser repetida caso falhe
	E que a solicitação de trabalho precise de 12 segundos para ser concluída
	E que a solicitação de trabalho não apresente nenhuma falha durante sua execução
	E que a solicitação de trabalho tenha sido marcada como em andamento há 60 segundos atrás
	Quando o despachante de trabalhos verificar a lista de trabalhos em andamento
	Então o despachante deve remover a solicitação da lista de trabalhos em andamento
	E o despachante deve republicar a solicitação de trabalho como pendente

@stable @slow
Cenário: Verificar que há uma solicitação de trabalho marcada como em andamento por mais tempo que o permitido com log em caso de erro
	Dado que uma solicitação de trabalho tenha sido marcada com um tempo máximo de processamento de 8 segundos
	E que a solicitação de trabalho não aceite ser repetida caso falhe, tendo que ser registrada em log nesse caso
	E que a solicitação de trabalho precise de 4 segundos para ser concluída
	E que a solicitação de trabalho apresente alguma falha durante sua execução
	E que a solicitação de trabalho tenha sido marcada como em andamento há 24 segundos atrás
	Quando o despachante de trabalhos verificar a lista de trabalhos em andamento
	Então o despachante deve remover a solicitação da lista de trabalhos em andamento
	E o despachante não deve republicar a solicitação de trabalho como pendente
	E o despachante deve registrar a falha no log de operações do despachante de trabalhos

@stable @slow
Cenário: Verificar que há uma solicitação de trabalho marcada como em andamento por mais tempo que o permitido com log em caso de timeout
	Dado que uma solicitação de trabalho tenha sido marcada com um tempo máximo de processamento de 8 segundos
	E que a solicitação de trabalho não aceite ser repetida caso falhe, tendo que ser registrada em log nesse caso
	E que a solicitação de trabalho precise de 12 segundos para ser concluída
	E que a solicitação de trabalho não apresente nenhuma falha durante sua execução
	E que a solicitação de trabalho tenha sido marcada como em andamento há 48 segundos atrás
	Quando o despachante de trabalhos verificar a lista de trabalhos em andamento
	Então o despachante deve remover a solicitação da lista de trabalhos em andamento
	E o despachante não deve republicar a solicitação de trabalho como pendente
	E o despachante deve registrar a falha no log de operações do despachante de trabalhos

@stable @slow
Cenário: Verificar que há uma solicitação de trabalho marcada como em andamento por mais tempo que o permitido e que sua execução já foi repetida diversas vezes devido a erro
	Dado que uma solicitação de trabalho tenha sido marcada com um tempo máximo de processamento de 10 segundos
	E que no máximo 3 tentativas possam ser feitas para execução da solicitação de trabalho
	E que a solicitação de trabalho aceite ser repetida caso falhe
	E que a solicitação de trabalho precise de 9 segundos para ser concluída
	E que a solicitação de trabalho apresente alguma falha durante sua execução
	E que a solicitação de trabalho tenha sido marcada como em andamento há 60 segundos atrás
	Quando o despachante de trabalhos verificar a lista de trabalhos em andamento
	Então o despachante deve remover a solicitação da lista de trabalhos em andamento
	E o despachante deve republicar a solicitação de trabalho como mal sucedida

@stable @slow
Cenário: Verificar que há uma solicitação de trabalho marcada como em andamento por mais tempo que o permitido e que sua execução já foi repetida diversas vezes devido a timeout
	Dado que uma solicitação de trabalho tenha sido marcada com um tempo máximo de processamento de 10 segundos
	E que no máximo 3 tentativas possam ser feitas para execução da solicitação de trabalho
	E que a solicitação de trabalho precise de 11 segundos para ser concluída
	E que a solicitação de trabalho não apresente nenhuma falha durante sua execução
	E que a solicitação de trabalho tenha sido marcada como em andamento há 60 segundos atrás
	Quando o despachante de trabalhos verificar a lista de trabalhos em andamento
	Então o despachante deve remover a solicitação da lista de trabalhos em andamento
	E o despachante deve republicar a solicitação de trabalho como mal sucedida
