using IotEdgeModuloCentral.Tipos;
using System;

namespace IotEdgeModuloCentral
{
    public static class Indicator
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

        private static MessageBodyIoTCentral messageBodyIoTCentral;

        #endregion


        public static void CalcularIndicadores(MessageBodyIoTCentral message)
        {
            messageBodyIoTCentral = message;

            //ESCOPO FINAL DE INDICADORES

            //GERENCIAMENTO DOS RESERVATÓRIOS
            ReservatorioSuperiorNivelPercentualAtual();
            ReservatorioInferiorNivelPercentualAtual();

            //Volume total dos reservatórios em m³
            ReservatoriosVolumeTotalAtual();

            //Autonomia em Horas baseada no consumo das últimas 24h
            AutonomiaBaseadaEm24horasDeConsumo();

            //Autonomia em Horas baseada no consumo instantâneo
            AutonomiaBaseadaEm1HoraDeConsumo();


            //GERENCIAMENTO DE BOMBAS

            //STATUS DE FUNCIONAMENTO
            BombaFuncionamentoTempo();

            //QUANTIDADE DE ACIONAMENTO (QT DE X QUE BOTÃO LIGAR/DESLIGAR É ATIVADO)
            BombaQuantidadeAcionamentoEm24Horas();
            BombaQuantidadeAcionamentoEm30Dias();

            //TEMPO DE FUNCIONAMENTO DA BOMBA(ACUMULA TEMPO ENTRE LIGAR E DESLIGAR BOMBA)
            BombaFuncionamentoTempo();


            //GERENCIAMENTO DE VAZÃO

            //MEDIÇÃO DE CONSUMO ENTRADA/SAÍDA
            MedidorVazaoConsumo30dias();
            MedidorVazaoConsumo1Dia();
            MedidorVazaoConsumo1Hora();

            //Fórmula sugerida para cálculo de média/meta mensal
            MetaConsumoMensal();

            //ALARME PARA VAZÃO DOS RESERVATÓRIOS 
            AlarmeReservatorioSuperiorAtingiuNivelMaximoAceitavel();
            AlarmeReservatorioInferiorAtingiuNivelMinimoAceitavel();

            //ALARME DE VAZAMENTO
            AlarmeReservatorioVazamento();
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
        public static int ReservatorioSuperiorNivelPercentualAtual()
        {
            int resultado = 0;

            try
            {
                resultado = (messageBodyIoTCentral.NivelReservatorioSuperior * 100) / CONST_NIVEL_SUPERIOR_VALOR_MAXIMO;
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [ReservatorioSuperiorNivelPercentual] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }
        public static int ReservatorioInferiorNivelPercentualAtual()
        {
            int resultado = 0;

            try
            {
                resultado = (messageBodyIoTCentral.NivelReservatorioInferior * 100) / CONST_NIVEL_INFERIOR_VALOR_MAXIMO;
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
        public static int ReservatorioSuperiorVolumeTotalAtual()
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
        public static int ReservatorioInferiorVolumeTotalAtual()
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
        public static int ReservatoriosVolumeTotalAtual()
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
        public static float AutonomiaBaseadaEm24horasDeConsumo()
        {
            float resultado = 0;

            try
            {
                var volumeTotal = ReservatoriosVolumeTotalAtual();
                var consumo24horas = ObterConsumoTotal24Horas();

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

        private static float ObterConsumoTotal24Horas()
        {
            //obter valor do banco
            var valor = new Random().Next(5000, 6000);
            Util.Log.MetodoNaoImplementado("ObterConsumoTotal24Horas", valor.ToString());

            ///Util.Log.Database.Indicadores.ObterConsumoTotal24Horas();


            return valor;
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
        public static float AutonomiaBaseadaEm1HoraDeConsumo()
        {
            float resultado = 0;

            try
            {
                var volumeTotal = ReservatoriosVolumeTotalAtual();
                var consumoTotalUltimaHora = ObterConsumoTotalUltimaHora();

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

        private static float ObterConsumoTotalUltimaHora()
        {
            var valor = new Random().Next(5000, 6000);
            Util.Log.MetodoNaoImplementado("ObterConsumoTotalUltimaHora", valor.ToString());
            return valor;
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
        public static int BombaQuantidadeAcionamentoEm24Horas()
        {
            int resultado = 0;

            try
            {
                resultado = ObterQuantidadeAcionamentoEm24Horas();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [BombaQuantidadeAcionamento] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }
        private static int ObterQuantidadeAcionamentoEm24Horas()
        {
            var valor = new Random().Next(5000, 6000);
            Util.Log.MetodoNaoImplementado("ObterQuantidadeAcionamentoEm24Horas", valor.ToString());
            return valor;
        }

        public static int BombaQuantidadeAcionamentoEm30Dias()
        {
            int resultado = 0;

            try
            {
                resultado = ObterQuantidadeAcionamentoEm30Dias();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [BombaQuantidadeAcionamento] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }
        private static int ObterQuantidadeAcionamentoEm30Dias()
        {
            var valor = new Random().Next(10, 100);
            Util.Log.MetodoNaoImplementado("ObterQuantidadeAcionamentoEm30Dias", valor.ToString());
            return valor;
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
        public static int BombaFuncionamentoTempo()
        {
            int resultado = 0;

            try
            {
                resultado = ObterBombaFuncionamentoTempo();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [BombaFuncionamentoTempo] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }
        private static int ObterBombaFuncionamentoTempo()
        {
            var valor = new Random().Next(10, 100);
            Util.Log.MetodoNaoImplementado("ObterBombaFuncionamentoTempo", valor.ToString());
            return valor;
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
        public static int MedidorVazaoConsumo30dias()
        {
            int resultado = 0;

            try
            {
                resultado = ObterMedidorVazaoConsumo30dias();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [MedidorVazaoConsumo30dias] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }
        private static int ObterMedidorVazaoConsumo30dias()
        {
            var valor = new Random().Next(5000, 50000);
            Util.Log.MetodoNaoImplementado("ObterMedidorVazaoConsumo30dias", valor.ToString());
            return valor;
        }

        public static int MedidorVazaoConsumo1Dia()
        {
            int resultado = 0;

            try
            {
                resultado = ObterMedidorVazaoConsumo1Dia();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [MedidorVazaoConsumo1Dia] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }
        private static int ObterMedidorVazaoConsumo1Dia()
        {
            var valor = new Random().Next(100, 1000);
            Util.Log.MetodoNaoImplementado("ObterMedidorVazaoConsumo1Dia", valor.ToString());
            return valor;
        }

        public static int MedidorVazaoConsumo1Hora()
        {
            int resultado = 0;

            try
            {
                resultado = ObterMedidorVazaoConsumo1Hora();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [MedidorVazaoConsumo1Hora] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }
        private static int ObterMedidorVazaoConsumo1Hora()
        {
            var valor = new Random().Next(10, 100);
            Util.Log.MetodoNaoImplementado("ObterMedidorVazaoConsumo1Hora", valor.ToString());
            return valor;
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
        public static int MetaConsumoMensal()
        {
            int resultado = 0;

            try
            {
                resultado = ObterMetaConsumoMensal();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [MetaConsumoMensal] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }
        private static int ObterMetaConsumoMensal()
        {
            var valor = new Random().Next(10, 100);
            Util.Log.MetodoNaoImplementado("ObterMetaConsumoMensal", valor.ToString());
            return valor;
        }


        //ValorMédioMensal = Totalizador_Consumo / Qtd_dias_decorridonomês
        //ValorMetaMensal (pré-estabelecido) = Meta_Consumo/ 30

        //A partir desses valores realizar a comparação, se ele está próximo do valor de meta, já ultrapassou o valor da meta, se está longe do valor.

        //ALARME PARA VAZÃO DOS RESERVATÓRIOS 
        //>= 10m3/h
        //<=1m3/h
        public static bool AlarmeReservatorioSuperiorAtingiuNivelMaximoAceitavel()
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
        public static bool AlarmeReservatorioInferiorAtingiuNivelMinimoAceitavel()
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
        public static int AlarmeReservatorioVazamento()
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
