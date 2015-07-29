#language:pt-BR

#Funcionalidade: Realizar um teste completo do despachante de trabalhos
#	De modo a ser capaz de testar o despachante de trabalhos
#	Como um despachante de trabalhos
#	Eu quero receber e despachar as solicitações de trabalho para serem processadas pelo responsável e retornar os resultados das solicitações

#@EncaminharSolicitacoesMatrixParsingEMatrixReversing_Cenario1
#Cenário: Teste Integrado dos Despachantes de Trabalho em um mesmo processo
#	Dado que existam cinco arquivos contendo cada um uma matriz de números complexos de trinta por trinta serializados em formato JSON
#	E que exista um script de inicialização configurando três despachantes de trabalho para tratar mensagens do tipo MatrixParsing e MatrixReversing
#	E que haja uma assinatura para recebimento de eventos do tipo WorkCompleted do tipo MatrixReversed
#	Quando os três despachantes de trabalho forem iniciados em um mesmo processo
#	E as solicitações de MatrixParsing contendo os cinco arquivos forem publicadas
#	E o tempo necessário para término do processamento já tiver passado
#	Então cinco eventos do tipo WorkCompleted contendo o resultado das solicitações deverão ser recebidos no tópico MatrixReversed
#	E cada uma dessas WorkCompleted conterá uma MatrixReversing que fora processada por um despachante de trabalho (não necessariamente uma por despachante)
#	E cada uma dessas MatrixParsing conterá uma MatrixParsing terá sido processada por um despachante de trabalho (não necessariamente uma por despachante)
#	E o resultado contido no evento MatrixReversed resultante de cada MatrixParsing deve ser o resultado esperado

#@EncaminharSolicitacoesMatrixParsingEMatrixReversing_Cenario2
#Cenário: Teste Integrado dos Despachantes de Trabalho pelo bootstrapper
#	Dado que existam cinco arquivos contendo cada um uma matriz de números complexos de trinta por trinta serializados em formato JSON
#	E que exista um script de inicialização configurando três despachantes de trabalho para tratar mensagens do tipo MatrixParsing e MatrixReversing
#	E que haja uma assinatura para recebimento de eventos do tipo WorkCompleted do tipo MatrixReversed
#	Quando os três despachantes de trabalho forem iniciados pelo bootstrapper
#	E as solicitações de MatrixParsing contendo os cinco arquivos forem publicadas
#	E o tempo necessário para término do processamento já tiver passado
#	Então cinco eventos do tipo WorkCompleted contendo o resultado das solicitações deverão ser recebidos no tópico MatrixReversed
#	E cada uma dessas WorkCompleted conterá uma MatrixReversing que fora processada por um despachante de trabalho (não necessariamente uma por despachante)
#	E cada uma dessas MatrixParsing conterá uma MatrixParsing terá sido processada por um despachante de trabalho (não necessariamente uma por despachante)
#	E o resultado contido no evento MatrixReversed resultante de cada MatrixParsing deve ser o resultado esperado