using IotEdgeModuloCentral.Helpers;
using IotEdgeModuloCentral.Tipos;
using System;

namespace IotEdgeModuloCentral
{
    public class Indicators
    {
        //CONSTANTES

        #region Constantes

        //valor MAXIMO retornado pelo sensor de nivel (representa 100%)
        const int CONST_NIVEL_SUPERIOR_VALOR_MAXIMO = 11122;
        const int CONST_NIVEL_INFERIOR_VALOR_MAXIMO = 11122;

        //valor MINIMO retornado pelo sensor de nivel (representa 100%)
        const int CONST_NIVEL_SUPERIOR_VALOR_MINIMO = 100;
        const int CONST_NIVEL_INFERIOR_VALOR_MINIMO = 100;

        //capacidade maxima do reservatorio em m3
        const int CONST_RESERVATORIO_SUPERIOR_CAPACIDADE_MAXIMA = 5000;
        const int CONST_RESERVATORIO_INFERIOR_CAPACIDADE_MAXIMA = 5000;

        //valores percentuais dos LIMITES aceitaveis dos reservatorios (valor de alarme)
        const int CONST_NIVEL_SUPERIOR_LIMITE_MAXIMO = 10;
        const int CONST_NIVEL_SUPERIOR_LIMITE_MINIMO = 10;
        const int CONST_NIVEL_INFERIOR_LIMITE_MAXIMO = 10;
        const int CONST_NIVEL_INFERIOR_LIMITE_MINIMO = 10;

        #endregion

        //VARIAVEL

        #region Variaveis

        MessageBodyIoTCentral message;

        #endregion

        public MessageBodyIoTCentral GetMessage(MessageBodyIoTCentral msg)
        {
            this.message = msg;

            message.VolumeReservatorioSuperior = ReservatorioSuperiorVolumeTotalAtual();
            message.VolumeReservatorioInferior = ReservatorioInferiorVolumeTotalAtual();
            message.VolumeTotalReservatorios = ReservatoriosVolumeTotalAtual();
            message.Autonomia24h = AutonomiaBaseadaEm24horasDeConsumo();

            message.AutonomiaInstantanea = AutonomiaBaseadaEm1HoraDeConsumo();
            message.QtAcionamentBomba1 = Bomba1QuantidadeAcionamentoEm24Horas();
            message.QtAcionamentBomba2 = Bomba2QuantidadeAcionamentoEm24Horas();
            int totalAcionamentosBombas = (message.QtAcionamentBomba1 + message.QtAcionamentBomba2);
            message.PercentualAcionamentBomba1 = (message.QtAcionamentBomba1 / totalAcionamentosBombas) * 100;
            message.PercentualAcionamentBomba2 = (message.QtAcionamentBomba2 / totalAcionamentosBombas) * 100;

            message.TempoAcionamentoBomba1 = Bomba1TempoAcionamentoEm30Dias();
            message.TempoAcionamentoBomba2 = Bomba2TempoAcionamentoEm30Dias();
            int tempoTotalAcionamentosBombas = (message.TempoAcionamentoBomba1 + message.TempoAcionamentoBomba2);
            message.PercentualTempoAcionamentoBomba1 = (message.TempoAcionamentoBomba1 / tempoTotalAcionamentosBombas) * 100;
            message.PercentualTempoAcionamentoBomba2 = (message.TempoAcionamentoBomba2 / tempoTotalAcionamentosBombas) * 100;

            message.ConsumoHora = MedidorVazaoConsumo1Hora();
            message.ConsumoDia = MedidorVazaoConsumo1Dia();
            message.ConsumoMes = MedidorVazaoConsumo30Dias();

            //result.MetaConsumo = ;
            //result.PercentualMetaConsumo = ;
            //result.TipoConsumidor = ;

            return message;
        }


        //ESCOPO FINAL DE INDICADORES

        //GERENCIAMENTO DOS RESERVATÓRIOS
        //Nível do reservatório 
        //0 - 100 %  
        //Alarme de transbordamento
        //Nível de reservatório(integer) maior que 100%
        //Alarme de segurança de nível
        //50% (metade)
        //25% (baixo)
        //20% (vazio) -> Constante até solucionar
        //Para alarme constante
        //Criar subalarmes a partir de 20%
        private int ReservatorioSuperiorNivelPercentualAtual()
        {
            int resultado = 0;

            try
            {
                resultado = (message.SondaDeNivelSuperior * 100) / CONST_NIVEL_SUPERIOR_VALOR_MAXIMO;
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [ReservatorioSuperiorNivelPercentual] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }
        private int ReservatorioInferiorNivelPercentualAtual()
        {
            int resultado = 0;

            try
            {
                resultado = (message.SondaDeNivelInferior * 100) / CONST_NIVEL_INFERIOR_VALOR_MAXIMO;
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [ReservatorioInferiorNivelPercentual] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }


        //Volume total dos reservatórios em m³
        //Para cálculo do volume total do reservatório deveremos considerar as seguintes variáveis na fórmula: 
        //Volume total do reservatório(valor informado pelo cliente no formulário)
        //Nível do reservatório já transformado para percentual 0 - 100%.

        //VOL_RES1 = (NÍVEL RES1% * VOLUME TOTAL 1 M³) / 100%
        //VOL_RES2 = (NÍVEL RES2 % * VOLUME TOTAL 2 M³) / 100%
        //VOL_TOTAL = VOL_RES1  + VOL_RES2
        private int ReservatorioSuperiorVolumeTotalAtual()
        {
            int resultado = 0;

            try
            {
                var valorReservatorio = ReservatorioSuperiorNivelPercentualAtual();
                resultado = (valorReservatorio * CONST_RESERVATORIO_SUPERIOR_CAPACIDADE_MAXIMA) / 100;
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [ReservatorioVolumeTotal] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }
        private int ReservatorioInferiorVolumeTotalAtual()
        {
            int resultado = 0;

            try
            {
                var valorReservatorio = ReservatorioInferiorNivelPercentualAtual();
                resultado = (valorReservatorio * CONST_RESERVATORIO_INFERIOR_CAPACIDADE_MAXIMA) / 100;
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [ReservatorioVolumeTotal] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }
        private int ReservatoriosVolumeTotalAtual()
        {
            int resultado = 0;

            try
            {
                var valorReservatorioSuperior = ReservatorioSuperiorNivelPercentualAtual();
                var valorReservatorioInferior = ReservatorioInferiorNivelPercentualAtual();
                resultado = valorReservatorioSuperior + valorReservatorioInferior;
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [ReservatorioVolumeTotal] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }


        //Autonomia em Horas baseada no consumo das últimas 24h
        //Para inferir a autonomia do reservatório devemos utilizar o dado da sonda de nível do reservatório(0-100%), combinado com o valor de consumo nas últimas 24h junto ao volume máximo do reservatório que é um valor fixo(5000l).
        //nivelReservatorio[0..100 %]
        //vazao24horas = valorHidrometroTempoX - valorHidrometroTempoY

        //Ou então logicamente pode ser definido: 

        //relogio_dia
        //If relogio_dia == 24 Then
        //vazão24horas:= (valorHidrometroTempoX/1000 - valorHidrometroTempoY);
        //valorHidrometroTempoY:=valorHidrometroTempoX/1000;
        //End IF

        //X>Y
        //quantidadeDeLitrosAtual = (nívelReservatorioSuperior * volumeMaximoReservatorio) / 100->EQUAÇÃO NO ITEM 2.
        //autonomiaEmHoras = quantidadeDeLitrosAtual/vazao24horas
        private int AutonomiaBaseadaEm24horasDeConsumo()
        {
            int resultado = 0;

            try
            {
                var volumeTotal = ReservatoriosVolumeTotalAtual();
                var consumo24horas = DatabaseHelper.Indicator.ObterConsumoTotal24Horas();

                resultado = volumeTotal / consumo24horas;
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [AutonomiaBaseadaEm24horasDeConsumo] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }


        //Autonomia em Horas baseada no consumo instantâneo
        //Para inferir a autonomia do reservatório devemos utilizar o dado da sonda de nível do reservatório, combinado com o valor de consumo por hora junto ao volume máximo do reservatório que é um valor fixo.
        //nivelReservatorio [0..100%]
        //vazao1Hora = valorHidrometroTempoX - valorHidrometroTempoY
        //Ou então logicamente pode ser definido: 

        //relogio_Hora
        //If relogio_Hora == 1 Then
        //vazão1Hora:= (valorHidrometroTempoX/1000 - valorHidrometroTempoY);
        //        valorHidrometroTempoY:=valorHidrometroTempoX/1000;
        //End IF

        //X>Y
        //quantidadeDeLitrosAtual = (nívelReservatorioSuperior * volumeMaximoReservatorio) / 100
        //autonomiaEmHoras = quantidadeDeLitrosAtual/vazão1Hora
        private int AutonomiaBaseadaEm1HoraDeConsumo()
        {
            int resultado = 0;

            try
            {
                var volumeTotal = ReservatoriosVolumeTotalAtual();
                var consumoTotalUltimaHora = DatabaseHelper.Indicator.ObterConsumoTotalUltimaHora();

                resultado = volumeTotal / consumoTotalUltimaHora;
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [AutonomiaBaseadaEm1HoraDeConsumo] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }


        //GERENCIAMENTO DE BOMBAS

        //QUANTIDADE DE ACIONAMENTO (QT DE X QUE BOTÃO LIGAR/DESLIGAR É ATIVADO)

        //Fórmula Geral
        //Toda vez que o STATUS for verdadeiro ou seja for acionado executar a fórmula abaixo: 

        //AC_BOMBA1 = 0
        //AC_BOMBA1 = AC_BOMBA1 + 1

        //por dia;
        //        Alocar o valor da variável totalizadora AC_BOMBA1 em uma memória no banco de dados a cada 24 h; 

        //acumulado mês;
        //        Alocar o valor da variável totalizadora AC_BOMBA1 em uma memória no banco de dados a cada 30 dias(720 h);

        //        Sem preset de alarmes, apenas as informações de quantidade.
        private int Bomba1QuantidadeAcionamentoEm24Horas()
        {
            int resultado = 0;

            try
            {
                resultado = DatabaseHelper.Indicator.ObterQuantidadeAcionamentoEm24Horas();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [BombaQuantidadeAcionamento] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }

        private int Bomba1QuantidadeAcionamentoEm30Dias()
        {
            int resultado = 0;

            try
            {
                resultado = DatabaseHelper.Indicator.ObterQuantidadeAcionamentoEm30Dias();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [BombaQuantidadeAcionamento] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }
        private int Bomba1TempoAcionamentoEm30Dias()
        {
            int resultado = 0;

            try
            {
                resultado = DatabaseHelper.Indicator.ObterQuantidadeAcionamentoEm30Dias();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [BombaQuantidadeAcionamento] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }


        //TEMPO DE FUNCIONAMENTO DA BOMBA(ACUMULA TEMPO ENTRE LIGAR E DESLIGAR BOMBA)

        //Fórmula Geral
        //Toda vez que o STATUS for verdadeiro ou seja for acionado executar a fórmula abaixo:

        //X = Tempo_Atual_Ligado

        //Exemplo: 
        //X = (hh:mm);
        //X = hh*60;
        //Res1= hh*60 + mm ; 

        //Quando a variável STATUS for falsa, ou seja for desativado

        //Y = Tempo_Atual_Desligado

        //Exemplo: 
        //Y= (hh:mm);
        //Y= hh*60;
        //Res2= hh*60 + mm ; 

        //Temp_FUN = Res2 - Res1(Tempo para um acionamento)


        //Obs : para realizar o cálculo da fórmula descrita acima, necessita-se multiplicar a hora por 60 para que o número esteja todo transformado em minuto, ou seja, o resultado do tempo será dado em minuto.

        //obs : para realizar a lógica de contagem de tempo é melhor utilizar um cronômetro.
        private int Bomba1FuncionamentoTempo()
        {
            int resultado = 0;

            try
            {
                resultado = DatabaseHelper.Indicator.ObterBombaFuncionamentoTempo();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [BombaFuncionamentoTempo] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }


        //GERENCIAMENTO DE BOMBAS

        //QUANTIDADE DE ACIONAMENTO (QT DE X QUE BOTÃO LIGAR/DESLIGAR É ATIVADO)

        //Fórmula Geral
        //Toda vez que o STATUS for verdadeiro ou seja for acionado executar a fórmula abaixo: 

        //AC_BOMBA1 = 0
        //AC_BOMBA1 = AC_BOMBA1 + 1

        //por dia;
        //        Alocar o valor da variável totalizadora AC_BOMBA1 em uma memória no banco de dados a cada 24 h; 

        //acumulado mês;
        //        Alocar o valor da variável totalizadora AC_BOMBA1 em uma memória no banco de dados a cada 30 dias(720 h);

        //        Sem preset de alarmes, apenas as informações de quantidade.
        private int Bomba2QuantidadeAcionamentoEm24Horas()
        {
            int resultado = 0;

            try
            {
                resultado = DatabaseHelper.Indicator.ObterQuantidadeAcionamentoEm24Horas();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [BombaQuantidadeAcionamento] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }

        private int Bomba2QuantidadeAcionamentoEm30Dias()
        {
            int resultado = 0;

            try
            {
                resultado = DatabaseHelper.Indicator.ObterQuantidadeAcionamentoEm30Dias();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [BombaQuantidadeAcionamento] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }
        private int Bomba2TempoAcionamentoEm30Dias()
        {
            int resultado = 0;

            try
            {
                resultado = DatabaseHelper.Indicator.ObterQuantidadeAcionamentoEm30Dias();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [BombaQuantidadeAcionamento] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }


        //TEMPO DE FUNCIONAMENTO DA BOMBA(ACUMULA TEMPO ENTRE LIGAR E DESLIGAR BOMBA)

        //Fórmula Geral
        //Toda vez que o STATUS for verdadeiro ou seja for acionado executar a fórmula abaixo:

        //X = Tempo_Atual_Ligado

        //Exemplo: 
        //X = (hh:mm);
        //X = hh*60;
        //Res1= hh*60 + mm ; 

        //Quando a variável STATUS for falsa, ou seja for desativado

        //Y = Tempo_Atual_Desligado

        //Exemplo: 
        //Y= (hh:mm);
        //Y= hh*60;
        //Res2= hh*60 + mm ; 

        //Temp_FUN = Res2 - Res1(Tempo para um acionamento)


        //Obs : para realizar o cálculo da fórmula descrita acima, necessita-se multiplicar a hora por 60 para que o número esteja todo transformado em minuto, ou seja, o resultado do tempo será dado em minuto.

        //obs : para realizar a lógica de contagem de tempo é melhor utilizar um cronômetro.
        private int Bomba2FuncionamentoTempo()
        {
            int resultado = 0;

            try
            {
                resultado = DatabaseHelper.Indicator.ObterBombaFuncionamentoTempo();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [BombaFuncionamentoTempo] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }


        //GERENCIAMENTO DE VAZÃO

        //MEDIÇÃO DE CONSUMO ENTRADA/SAÍDA
        //        Hora
        //vazao1Hora = valorHidrometroTempoX - valorHidrometroTempoY
        //X>Y
        //Dia
        //vazao24horas = valorHidrometroTempoX - valorHidrometroTempoY
        //X>Y
        //30 dias
        //vazao1mes = valorHidrometroTempoX - valorHidrometroTempoY
        //X>Y
        private int MedidorVazaoConsumo30Dias()
        {
            int resultado = 0;

            try
            {
                resultado = DatabaseHelper.Indicator.ObterMedidorVazaoConsumo30dias();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [MedidorVazaoConsumo30dias] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }

        private int MedidorVazaoConsumo1Dia()
        {
            int resultado = 0;

            try
            {
                resultado = DatabaseHelper.Indicator.ObterMedidorVazaoConsumo1Dia();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [MedidorVazaoConsumo1Dia] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }

        private int MedidorVazaoConsumo1Hora()
        {
            int resultado = 0;

            try
            {
                resultado = DatabaseHelper.Indicator.ObterMedidorVazaoConsumo1Hora();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [MedidorVazaoConsumo1Hora] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }


        //META DE CONSUMO
        //Inicialmente podemos criar uma regra enviando um SMS para o cliente baseado neste parâmetro?  
        //Definimos ou usuário pode adicionar ou mudar as regras de ativação baseado no valor de vazão acumulado do mês
        //RPM no momento não vai ser possível
        //No cadastro o cliente vai informar a média de consumo mensal da estrutura, a partir disso deveremos disponibilizar no sistema o percentual de redução estipulado com as seguintes opções, para que o cliente escolha:  

        //X = 0 - 5% de redução           Y= 5 - 15% de redução   Z = 15 - 30 %  de redução

        //Exemplo: No momento do cadastro o cliente informou que a sua média de consumo mensal é igual a 5000 m³ e escolheu um percentual de redução de 10% então agora ele tem uma meta de consumo 4500 m³.

        //A partir dessa informação e da medição do consumo acumulado em 30 dias, informar ao cliente se ele está próximo do valor de meta, já ultrapassou o valor da meta, se está longe do valor.Sugestão foi usar o velocímetro com as cores verde, amarelo e vermelho para fazer essa indicação.

        //Fórmula sugerida para cálculo de média/meta mensal
        private int MetaConsumoMensal()
        {
            int resultado = 0;

            try
            {
                resultado = DatabaseHelper.Indicator.ObterMetaConsumoMensal();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [MetaConsumoMensal] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }


        //ValorMédioMensal = Totalizador_Consumo / Qtd_dias_decorridonomês
        //ValorMetaMensal (pré-estabelecido) = Meta_Consumo/ 30

        //A partir desses valores realizar a comparação, se ele está próximo do valor de meta, já ultrapassou o valor da meta, se está longe do valor.

        //ALARME PARA VAZÃO DOS RESERVATÓRIOS 
        //>= 10m3/h
        //<=1m3/h
        private bool AlarmeReservatorioSuperiorAtingiuNivelMaximoAceitavel()
        {
            bool resultado = false;

            try
            {
                var nivel = ReservatorioSuperiorNivelPercentualAtual();
                var nivelMaximo = CONST_NIVEL_SUPERIOR_LIMITE_MAXIMO - ((CONST_RESERVATORIO_SUPERIOR_CAPACIDADE_MAXIMA * CONST_NIVEL_SUPERIOR_LIMITE_MAXIMO) / 100);
                if (nivel < nivelMaximo)
                {
                    resultado = true;
                }
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [AlarmeReservatorioSuperiorAtingiuNivelMaximoAceitavel] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }
        private bool AlarmeReservatorioInferiorAtingiuNivelMinimoAceitavel()
        {
            bool resultado = false;

            try
            {
                var nivel = ReservatorioInferiorNivelPercentualAtual();
                var nivelMinimo = (CONST_RESERVATORIO_SUPERIOR_CAPACIDADE_MAXIMA * CONST_NIVEL_SUPERIOR_LIMITE_MINIMO) / 100;
                if (nivel < nivelMinimo)
                {
                    resultado = true;
                }
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [AlarmeReservatorioInferiorAtingiuNivelMinimoAceitavel] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }

        //ALARME DE VAZAMENTO
        //Como identificar um vazamento? Quais parâmetros devem ser tra
        private int AlarmeReservatorioVazamento()
        {
            int resultado = 0;

            try
            {
                //resultado = messageBodyIoTCentral.AlarmeReservatorioVazamento;
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [AlarmeReservatorioVazamento] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }

    } //class

} //namespace
