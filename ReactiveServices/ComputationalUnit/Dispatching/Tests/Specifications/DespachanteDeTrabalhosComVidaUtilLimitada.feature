#language: pt-BR

Funcionalidade: DespachanteDeTrabalhosComVidaUtilLimitada
	De modo a poder controlar por quanto tempo um despachante de trabalhos se mantém ativo
	Como um executor de despachante de trabalhos
	Desejo que o despachante se mantenha ativo de acordo com as configurações de vida útil que eu informar

@stable @fast
Cenário: Despachante de trabalhos deve permanecer vivo continuamente
	Dado que um despachante de trabalhos tenha sido configurado para se manter ativo continuamente
	Quando o despachante de trabalhos for inicializado
	E se passarem 5 segundos
	Então o despachante não deve ser encerrado

@stable @fast
Cenário: Despachante de trabalhos deve permanecer vivo até concluir seu primeiro trabalho
	Dado que um despachante de trabalhos tenha sido configurado para se manter ativo até concluir seu primeiro trabalho com sucesso ou com falha irrecuperável
	Quando o despachante de trabalhos for inicializado
	E que o despachante de trabalhos receba uma solicitação de trabalho com duração de 1 segundos
	E se passarem 5 segundos
	Então o despachante deve ser encerrado

@stable @fast
Cenário: Despachante de trabalhos deve permanecer vivo pelo tempo máximo determinado, sem nenhum trabalho ser recebido
	Dado que um despachante de trabalhos tenha sido configurado para se manter ativo até que 5 segundos tenham se passado
	Quando o despachante de trabalhos for inicializado
	E se passarem 10 segundos
	Então o despachante deve ser encerrado

@stable @slow
Cenário: Despachante de trabalhos deve permanecer vivo pelo tempo máximo determinado, com um trabalho em execução
	Dado que um despachante de trabalhos tenha sido configurado para se manter ativo até que 5 segundos tenham se passado
	Quando o despachante de trabalhos for inicializado
	E que o despachante de trabalhos receba uma solicitação de trabalho com duração de 30 segundos
	E se passarem 10 segundos
	Então o despachante não deve ser encerrado

@stable @fast
Cenário: Despachante de trabalhos deve permanecer vivo pelo tempo máximo determinado, com um trabalho já concluído
	Dado que um despachante de trabalhos tenha sido configurado para se manter ativo até que 5 segundos tenham se passado
	Quando o despachante de trabalhos for inicializado
	E que o despachante de trabalhos receba uma solicitação de trabalho com duração de 1 segundos
	E se passarem 10 segundos
	Então o despachante deve ser encerrado