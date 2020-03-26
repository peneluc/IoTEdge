namespace IotEdgeModuloCentral.Tipos
{

    class Content
    {
        //HwId":"GTI-Device","Data":[{"CorrelationId":"DefaultCorrelationId","SourceTimestamp":"2020-03-24 04:43:19","Values":[{"DisplayName":"MedidorDeNivel","Address":"100002","Value":"1"}]}]}]}

        public string HwId { get; set; } //GTI-Device
        public Data Data { get; set; } //":[{"CorrelationId":"DefaultCorrelationId","SourceTimestamp":"2020-03-24 04:43:19","Values":[{"DisplayName":"MedidorDeNivel","Address":"100002","Value":"1"}]}]}]}

    }
}