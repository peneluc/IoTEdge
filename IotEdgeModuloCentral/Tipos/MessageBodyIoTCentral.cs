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
            "VazaoSaida": 250,
            "NivelReservatorioInferior": 850,
            "VazaoEntrada": 250,
            "StatusBomba1": false,
            "StatusBomba2”: false
        }
    */

    public class MessageBodyIoTCentral
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
    }
}
