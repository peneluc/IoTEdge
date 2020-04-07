namespace IotEdgeModuloCentral.Tipos
{
    class Values
    {
        //{
        //    "PublishTimestamp": "2020-03-25 23:36:51",
        //    "Content": [
        //    {
        //        "HwId": "GTI-Device",
        //        "Data": [
        //        {
        //            "CorrelationId": "DefaultCorrelationId",
        //            "SourceTimestamp": "2020-03-25 23:36:51",
        //            "Values": [
        //            {
        //                "DisplayName": "MedidorDeNivel",
        //                "Address": "100002",
        //                "Value": "1"
        //            }
        //            ]
        //        }
        //        ]
        //    }
        //    ]
        //}
        public string DisplayName { get; set; }
        public string Address { get; set; }
        public string Value { get; set; }
    }
}
