#language: pt-BR

Funcionalidade: Carregar configuracoes de partida de sistema

@stable @fast
Cenário: Carregar configuração para uma única execução de uma solicitação de trabalhos do tipo A, em um único despachante de trabalhos
	Dado que as configurações solicitem a execução de uma instância do despachante de trabalhos a ser identificada como despachante 'SettingsTest1'
	E que as configurações informem que o despachante 'SettingsTest1' será capaz de processar até 1 solicitações de trabalho por vez
	E que as configurações informem que o despachante 'SettingsTest1' será capaz de processar solicitações de trabalho do tipo 'A'
	E que as configurações solicitem a execução de 1 solicitações de trabalho do tipo 'A' na partida do sistema
	Quando as configurações forem carregadas no gerenciador de partida
	Então o gerenciador de partida deve solicitar a execução de uma instância do despachante de trabalhos a ser identificada como despachante 'SettingsTest1'
	E o despachante 'SettingsTest1' deve ser capaz de processar até 1 solicitações de trabalho por vez
	E o despachante 'SettingsTest1' deve ser capaz de processar solicitações de trabalho do tipo 'A'
	E o gerenciador de partida deve solicitar a execução de 1 solicitações de trabalho do tipo 'A' na partida do sistema

@stable @fast
Esquema do Cenário: Carregar configuração para n execuções de uma solicitação de trabalhos do tipo A, em um único despachante de trabalhos
	Dado que as configurações solicitem a execução de uma instância do despachante de trabalhos a ser identificada como despachante 'SettingsTest2_<n>'
	E que as configurações informem que o despachante 'SettingsTest2_<n>' será capaz de processar até <n> solicitações de trabalho por vez
	E que as configurações informem que o despachante 'SettingsTest2_<n>' será capaz de processar solicitações de trabalho do tipo 'A'
	E que as configurações solicitem a execução de <n> solicitações de trabalho do tipo 'A' na partida do sistema
	Quando as configurações forem carregadas no gerenciador de partida
	Então o gerenciador de partida deve solicitar a execução de uma instância do despachante de trabalhos a ser identificada como despachante 'SettingsTest2_<n>'
	E o despachante 'SettingsTest2_<n>' deve ser capaz de processar até <n> solicitações de trabalho por vez
	E o despachante 'SettingsTest2_<n>' deve ser capaz de processar solicitações de trabalho do tipo 'A'
	E o gerenciador de partida deve solicitar a execução de <n> solicitações de trabalho do tipo 'A' na partida do sistema
	Exemplos: 
	|	n	|
	|	2	|
	|	10	|
	|	20	|

@stable @fast
Esquema do Cenário: Carregar configuração para n execuções de uma solicitação de trabalhos do tipo A, em dois despachantes de trabalhos
	Dado que as configurações solicitem a execução de uma instância do despachante de trabalhos a ser identificada como despachante 'SettingsTest3_Tipo1_<n><m><o>'
	E que as configurações solicitem a execução de uma instância do despachante de trabalhos a ser identificada como despachante 'SettingsTest3_Tipo2_<n><m><o>'
	E que as configurações informem que o despachante 'SettingsTest3_Tipo1_<n><m><o>' será capaz de processar até <m> solicitações de trabalho por vez
	E que as configurações informem que o despachante 'SettingsTest3_Tipo1_<n><m><o>' será capaz de processar solicitações de trabalho do tipo 'A'
	E que as configurações informem que o despachante 'SettingsTest3_Tipo2_<n><m><o>' será capaz de processar até <o> solicitações de trabalho por vez
	E que as configurações informem que o despachante 'SettingsTest3_Tipo2_<n><m><o>' será capaz de processar solicitações de trabalho do tipo 'A'
	E que as configurações solicitem a execução de <n> solicitações de trabalho do tipo 'A' na partida do sistema
	Quando as configurações forem carregadas no gerenciador de partida
	Então o gerenciador de partida deve solicitar a execução de uma instância do despachante de trabalhos a ser identificada como despachante 'SettingsTest3_Tipo1_<n><m><o>'
	E o despachante 'SettingsTest3_Tipo1_<n><m><o>' deve ser capaz de processar até <m> solicitações de trabalho por vez
	E o despachante 'SettingsTest3_Tipo1_<n><m><o>' deve ser capaz de processar solicitações de trabalho do tipo 'A'
	E o despachante 'SettingsTest3_Tipo2_<n><m><o>' deve ser capaz de processar até <o> solicitações de trabalho por vez
	E o despachante 'SettingsTest3_Tipo2_<n><m><o>' deve ser capaz de processar solicitações de trabalho do tipo 'A'
	E o gerenciador de partida deve solicitar a execução de <n> solicitações de trabalho do tipo 'A' na partida do sistema
	Exemplos: 
	| n  | m  | o  |
	| 2  | 1  | 1  |
	| 11 | 6  | 5  |
	| 20 | 10 | 10 |

@stable @fast
Esquema do Cenário: Carregar configuração para n execuções de uma solicitação de trabalhos do tipo A e outra do tipo B, em dois despachantes de trabalhos
	Dado que as configurações solicitem a execução de uma instância do despachante de trabalhos a ser identificada como despachante 'SettingsTest3_Tipo1_<n1><n2><m><o>'
	E que as configurações solicitem a execução de uma instância do despachante de trabalhos a ser identificada como despachante 'SettingsTest3_Tipo2_<n1><n2><m><o>'
	E que as configurações informem que o despachante 'SettingsTest3_Tipo1_<n1><n2><m><o>' será capaz de processar até <m> solicitações de trabalho por vez
	E que as configurações informem que o despachante 'SettingsTest3_Tipo1_<n1><n2><m><o>' será capaz de processar solicitações de trabalho do tipo 'A'
	E que as configurações informem que o despachante 'SettingsTest3_Tipo2_<n1><n2><m><o>' será capaz de processar até <o> solicitações de trabalho por vez
	E que as configurações informem que o despachante 'SettingsTest3_Tipo2_<n1><n2><m><o>' será capaz de processar solicitações de trabalho do tipo 'B'
	E que as configurações solicitem a execução de <n1> solicitações de trabalho do tipo 'A' na partida do sistema
	E que as configurações solicitem a execução de <n2> solicitações de trabalho do tipo 'B' na partida do sistema
	Quando as configurações forem carregadas no gerenciador de partida
	Então o gerenciador de partida deve solicitar a execução de uma instância do despachante de trabalhos a ser identificada como despachante 'SettingsTest3_Tipo1_<n1><n2><m><o>'
	E o despachante 'SettingsTest3_Tipo1_<n1><n2><m><o>' deve ser capaz de processar até <m> solicitações de trabalho por vez
	E o despachante 'SettingsTest3_Tipo1_<n1><n2><m><o>' deve ser capaz de processar solicitações de trabalho do tipo 'A'
	E o despachante 'SettingsTest3_Tipo2_<n1><n2><m><o>' deve ser capaz de processar até <o> solicitações de trabalho por vez
	E o despachante 'SettingsTest3_Tipo2_<n1><n2><m><o>' deve ser capaz de processar solicitações de trabalho do tipo 'B'
	E o gerenciador de partida deve solicitar a execução de <n1> solicitações de trabalho do tipo 'A' na partida do sistema
	E o gerenciador de partida deve solicitar a execução de <n2> solicitações de trabalho do tipo 'B' na partida do sistema
	Exemplos: 
	| n1 | n2 | m  | o  |
	| 2  | 2  | 2  | 2  |
	| 11 | 11 | 11 | 11 |
	| 20 | 20 | 20 | 20 |

@stable @fast
Cenário: Salvar configurações em script e carregá-las no gerenciador de partida
	Dado que as configurações solicitem a execução de uma instância do despachante de trabalhos a ser identificada como despachante 'SettingsTest5'
	E que as configurações informem que o despachante 'SettingsTest5' será capaz de processar até 1 solicitações de trabalho por vez
	E que as configurações informem que o despachante 'SettingsTest5' será capaz de processar solicitações de trabalho do tipo 'A'
	E que as configurações solicitem a execução de 1 solicitações de trabalho do tipo 'A' na partida do sistema
	Quando as configurações forem salvas em um arquivo
	E o arquivo for carregado nas configurações do gerenciador de partida
	Então o gerenciador de partida deve solicitar a execução de uma instância do despachante de trabalhos a ser identificada como despachante 'SettingsTest5'
	E o despachante 'SettingsTest5' deve ser capaz de processar até 1 solicitações de trabalho por vez
	E o despachante 'SettingsTest5' deve ser capaz de processar solicitações de trabalho do tipo 'A'
	E o gerenciador de partida deve solicitar a execução de 1 solicitações de trabalho do tipo 'A' na partida do sistema
