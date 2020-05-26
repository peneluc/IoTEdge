using System;

namespace IotEdgeModuloCentral.Tipos
{
    /*
        {
          "HwId": "1",
          "PublicacaoCLP": "2020-04-06 00:52:03",
          "PublicacaoModBus": "2020-04-06 00:52:03",
          "PublicacaoCentral": "2020-04-06 00:52:03",
          "NivelReservatorioSuperior": 850,
          "NivelReservatorioInferior": 850,
          "MedidorSaida": 850,
          "MedidorEntrada": 250,
          "StatusBomba1": false,
          "StatusBomba2”: false,
            "VolumeReservatorioSuperior": 850,
            "VolumeReservatorioInferior": 850,
            "VolumeTotalReservatorios": 850,
            "Autonomia24h": 850,
            "AutonomiaInstantanea": 850,
            "QtAcionamentBomba1": 850,
            "QtAcionamentBomba2": 850,
            "PercentualAcionamentBomba1": 850,
            "PercentualAcionamentBomba2": 850,
            "TempoAcionamentoBomba1": 850,
            "TempoAcionamentoBomba2": 850,
            "PercentualTempoAcionamentoBomba1": 850,
            "PercentualTempoAcionamentoBomba2": 850,
            "ConsumoHora": 850,
            "ConsumoDia": 850,
            "ConsumoMes": 850,
            “MetaConsumo”: 850,
            “PercentualMetaConsumo”: 850,
            “TipoConsumidor”: “A”,
        }
    */

    public class MessageBodyIoTCentral
    {
        public string HwId { get; set; }
        public DateTime PublicacaoCLP { get; set; }
        public DateTime PublicacaoModBus { get; set; }
        public DateTime PublicacaoCentral { get; set; }

        //dados sensores
        public bool AcionamentoBomba1 { get; set; }
        public bool AcionamentoBomba2 { get; set; }
        public int LeituraMedidorInferior { get; set; }
        public int LeituraMedidorSuperior { get; set; }
        public bool StatusBomba1 { get; set; }
        public bool StatusBomba2 { get; set; }
        public bool StatusFalhaBomba1 { get; set; }
        public bool StatusFalhaBomba2 { get; set; }
        public int SondaDeNivelInferior { get; set; }   
        public int SondaDeNivelSuperior { get; set; }


        //indicadores
        public int VolumeReservatorioSuperior { get; set; }
        public int VolumeReservatorioInferior { get; set; }
        public int VolumeTotalReservatorios { get; set; }
        public int Autonomia24h { get; set; }
        public int AutonomiaInstantanea { get; set; }
        public int QtAcionamentBomba1 { get; set; }
        public int QtAcionamentBomba2 { get; set; }
        public int PercentualAcionamentBomba1 { get; set; }
        public int PercentualAcionamentBomba2 { get; set; }
        public int TempoAcionamentoBomba1 { get; set; }
        public int TempoAcionamentoBomba2 { get; set; }
        public int PercentualTempoAcionamentoBomba1 { get; set; }
        public int PercentualTempoAcionamentoBomba2 { get; set; }
        public int ConsumoHora { get; set; }
        public int ConsumoDia { get; set; }
        public int ConsumoMes { get; set; }
        public int MetaConsumo { get; set; }
        public int PercentualMetaConsumo { get; set; }
        public int TipoConsumidor { get; set; }
        public int NivelReservatorioSuperior { get; internal set; }
        public int NivelReservatorioInferior { get; internal set; }
        public object VazaoHoraReservatorioSuperior { get; internal set; }
        public int VazaoHoraReservatorioInferior { get; internal set; }
        public int ConsumoDiaReservatorioSuperior { get; internal set; }
        public int ConsumoDiaReservatorioInferior { get; internal set; }
        public int ConsumoMesReservatorioSuperior { get; internal set; }
        public int ConsumoMesReservatorioInferior { get; internal set; }
    }
}