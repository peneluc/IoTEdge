using System;

namespace IotEdgeModuloCentral.Tipos
{
    public class MessageDatabase
    {
        public string HwId { get; set; }
        public DateTime PublicacaoCLP { get; set; }
        public DateTime PublicacaoModBus { get; set; }
        public DateTime PublicacaoCentral { get; set; }
        public int NivelReservatorioInferior { get; set; }
        public int NivelReservatorioSuperior { get; set; }
        public int VazaoSaida { get; set; }
        public int VazaoEntrada { get; set; }
        public bool StatusBomba1 { get; set; }
        public bool StatusBomba2 { get; set; }
        public bool FalhaBomba1 { get; internal set; }
        public bool FalhaBomba2 { get; internal set; }

        //ESCOPO FINAL DE INDICADORES
        public int ReservatorioSuperiorNivelPercentualAtual { get; set; }
        public int ReservatorioInferiorNivelPercentualAtual { get; set; }
        public int ReservatoriosVolumeTotalAtual { get; set; }
        public float AutonomiaBaseadaEm24horasDeConsumo { get; set; }
        public float AutonomiaBaseadaEm1HoraDeConsumo { get; set; }
        public int BombaQuantidadeAcionamentoEm24Horas { get; set; }
        public int BombaQuantidadeAcionamentoEm30Dias { get; set; }
        public int BombaFuncionamentoTempo { get; set; }
        public int MedidorVazaoConsumo30dias { get; set; }
        public int MedidorVazaoConsumo1Dia { get; set; }
        public int MedidorVazaoConsumo1Hora { get; set; }
        public int MetaConsumoMensal { get; set; }
        public bool AlarmeReservatorioSuperiorAtingiuNivelMaximoAceitavel { get; set; }
        public bool AlarmeReservatorioInferiorAtingiuNivelMinimoAceitavel { get; set; }
        public int AlarmeReservatorioVazamento { get; set; }
    }
}
