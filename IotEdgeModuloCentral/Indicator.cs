using IotEdgeModuloCentral.Helpers;
using IotEdgeModuloCentral.Tipos;
using System;

namespace IotEdgeModuloCentral
{
    public class Indicators
    {

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


        #region Variaveis

        MessageBodyIoTCentral message;

        #endregion


        #region Modelo

            //v2.0 (MVP)
            //    {
            //    "HwId": "1",
            //    "PublicacaoCLP": "2020-04-06 00:52:03",
            //    "PublicacaoModBus": "2020-04-06 00:52:03",
            //    "PublicacaoCentral": "2020-04-06 00:52:03",
            //    "VolumeReservatorioSuperior": 100, //valor inteiro em m³ 
            //    "VolumeReservatorioInferior": 100, //valor inteiro em m³ 
            //    "VolumeTotalReservatorios": 100, //valor inteiro em m³
            //    "LeituraMedidorSuperior": 500.100, // três casas decimais após vírgula e variação de pulso 10 em 10.
            //    "LeituraMedidorInferior": 500.100, // três casas decimais após vírgula e variação de pulso 10 em 10.
            //    "NivelReservatorioSuperior": 100%, // Valor percentual sem casas decimais
            //    "NivelReservatorioInferior": 100%, // Valor percentual sem casas decimais
            //    "Autonomia24h": 10, //valor inteiro em hora
            //    "AutonomiaInstantanea": 10, //valor inteiro em hora
            //    "StatusBomba1": false, // booleano 
            //    "StatusBomba2”: false, // booleano 
            //    "QtAcionamentBomba1": 20, //valor inteiro, quantidade 
            //    "QtAcionamentBomba2": 15, //valor inteiro, quantidade
            //    "PercentualAcionamentoBomba1": 100%, // Valor percentual sem casas decimais
            //    "PercentualAcionamentoBomba2": 100%, // Valor percentual sem casas decimais
            //    "TempoAcionamentoBomba1": 20, //valor inteiro em hora ou minuto
            //    "TempoAcionamentoBomba2": 40, //valor inteiro em hora ou minuto
            //    "PercentualTempoAcionamentoBomba1": 10, //valor inteiro em hora ou minuto
            //    "PercentualTempoAcionamentoBomba2":10, //valor inteiro em hora ou minuto
            //    "VazaoHoraReservatorioSuperior": 2, //valor inteiro sem casas decimais m³/h
            //    "VazaoHoraReservatorioInferior": 2, //valor inteiro sem casas decimais m³/h
            //    "ConsumoDiaReservatorioSuperior": 50, //valor inteiro sem casas
            //    “ConsumoDiaReservatorioInferior”: 150, //valor inteiro sem casas
            //    "ConsumoMesReservatorioSuperior": 500, //valor inteiro sem casas
            //    "ConsumoMesReservatorioInferior": 1500, //valor inteiro sem casas
            //    “MetaConsumo”: 1500, //valor inteiro sem casas
            //    “PercentualMetaConsumo”: 100%, // Valor percentual sem casas 
            //    “TipoConsumidor”: “A”,
            //    }

        #endregion


        public MessageBodyIoTCentral GetMessage(MessageBodyIoTCentral msg)
        {
            try
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

                message.NivelReservatorioSuperior = NivelReservatorioSuperior(); // Valor percentual sem casas decimais
                message.NivelReservatorioInferior = NivelReservatorioInferior(); // Valor percentual sem casas decimais

                message.VazaoHoraReservatorioSuperior = ; //valor inteiro sem casas decimais m³/h
                message.VazaoHoraReservatorioInferior = ; //valor inteiro sem casas decimais m³/h

                message.ConsumoDiaReservatorioSuperior = ; //valor inteiro sem casas
                message.ConsumoDiaReservatorioInferior = ; //valor inteiro sem casas

                message.ConsumoMesReservatorioSuperior = ; //valor inteiro sem casas
                message.ConsumoMesReservatorioInferior = ; //valor inteiro sem casas

                message.ConsumoHora = MedidorVazaoConsumo1Hora();
                message.ConsumoDia = MedidorVazaoConsumo1Dia();
                message.ConsumoMes = MedidorVazaoConsumo30Dias();

                //result.MetaConsumo = ;
                //result.PercentualMetaConsumo = ;
                //result.TipoConsumidor = ;
            }
            catch (Exception ex)
            {
                Util.Log.Log($"[Indicator.GetMessage] - Erro: {ex}");
            }

            return message;
        }


        private int NivelReservatorioSuperior()
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

        private int NivelReservatorioInferior()
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

        private int ReservatoriosPercentualTotalAtual()
        {
            int resultado = 0;

            try
            {
                var valorReservatorioSuperior = NivelReservatorioSuperior();
                var valorReservatorioInferior = NivelReservatorioInferior();
                resultado = valorReservatorioSuperior + valorReservatorioInferior;
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [ReservatoriosPercentualTotalAtual] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }

        private int ReservatorioSuperiorVolumeTotalAtual()
        {
            int resultado = 0;

            try
            {
                var valorReservatorio = NivelReservatorioSuperior();
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
                var valorReservatorio = NivelReservatorioInferior();
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
                var valorReservatorioSuperior = ReservatorioSuperiorVolumeTotalAtual();
                var valorReservatorioInferior = ReservatorioInferiorVolumeTotalAtual();
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

        private int AutonomiaBaseadaEm24horasDeConsumo()
        {
            int resultado = 0;

            try
            {
                var volumeTotal = ReservatoriosVolumeTotalAtual();
                var consumo24horas = DatabaseHelper.Indicator.ObterConsumoTotal24Horas();

                if (volumeTotal > 0)
                {
                    if (consumo24horas > 0)
                    {
                        resultado = volumeTotal / consumo24horas;
                    }
                    else
                    {
                        Util.Log.Error($"AutonomiaBaseadaEm24horasDeConsumo - Erro: Sem consumo nas 24 horas");
                    }
                }
                else
                {
                    Util.Log.Error($"AutonomiaBaseadaEm24horasDeConsumo - Erro: Volume atual zerado");
                }
            }
            catch (Exception ex)
            {
                var msg = $"AutonomiaBaseadaEm24horasDeConsumo - Erro: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }

        private int AutonomiaBaseadaEm1HoraDeConsumo()
        {
            int resultado = 0;

            try
            {
                var volumeTotal = ReservatoriosVolumeTotalAtual();
                var consumoTotalUltimaHora = DatabaseHelper.Indicator.ObterConsumoTotalUltimaHora();

                if (volumeTotal > 0)
                {
                    if (consumoTotalUltimaHora > 0)
                    {
                        resultado = volumeTotal / consumoTotalUltimaHora;
                    }
                    else
                    {
                        Util.Log.Error($"AutonomiaBaseadaEm1HoraDeConsumo - Erro: Sem consumo na ultima horas");
                    }
                }
                else
                {
                    Util.Log.Error($"AutonomiaBaseadaEm1HoraDeConsumo - Erro: Volume atual zerado");
                }
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [AutonomiaBaseadaEm1HoraDeConsumo] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }

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

        private int Medidor1VazaoConsumo30Dias()
        {
            int resultado = 0;

            try
            {
                resultado = DatabaseHelper.Indicator.ObterMedidor1VazaoConsumo30dias();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [MedidorVazaoConsumo30dias] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }

        private int Medidor2VazaoConsumo30Dias()
        {
            int resultado = 0;

            try
            {
                resultado = DatabaseHelper.Indicator.ObterMedidor2VazaoConsumo30dias();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [MedidorVazaoConsumo30dias] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }

        private int Medidor1VazaoConsumo1Dia()
        {
            int resultado = 0;

            try
            {
                resultado = DatabaseHelper.Indicator.ObterMedidor1VazaoConsumo1Dia();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [MedidorVazaoConsumo1Dia] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }

        private int Medidor2VazaoConsumo1Dia()
        {
            int resultado = 0;

            try
            {
                resultado = DatabaseHelper.Indicator.ObterMedidor2VazaoConsumo1Dia();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [MedidorVazaoConsumo1Dia] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }

        private int Medidor1VazaoConsumo1Hora()
        {
            int resultado = 0;

            try
            {
                resultado = DatabaseHelper.Indicator.ObterMedidor1VazaoConsumo1Hora();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [MedidorVazaoConsumo1Hora] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }

        private int Medidor2VazaoConsumo1Hora()
        {
            int resultado = 0;

            try
            {
                resultado = DatabaseHelper.Indicator.ObterMedidor2VazaoConsumo1Hora();
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [MedidorVazaoConsumo1Hora] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }

        private int MetaConsumoMensal()
        {
            int resultado = 0;

            try
            {
                //resultado = ParameterHelper.MetaConsumoMensal;
            }
            catch (Exception ex)
            {
                var msg = $"Erro ao calcular o Indicador [MetaConsumoMensal] - exc: {ex}";
                Util.Log.Error(msg);
                throw new Exception(msg, ex);
            }

            return resultado;
        }

        private bool AlarmeReservatorioSuperiorAtingiuNivelMaximoAceitavel()
        {
            bool resultado = false;

            try
            {
                var nivel = NivelReservatorioSuperior();
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
                var nivel = NivelReservatorioInferior();
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
