using System;

namespace IotEdgeModuloCentral.Tipos
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

    class MessageBody
    {
        public DateTime PublishTimestamp { get; set; } //2020-03-24 04:43:20
        public Content Content { get; set; } //HwId":"GTI-Device","Data":[{"CorrelationId":"DefaultCorrelationId","SourceTimestamp":"2020-03-24 04:43:19","Values":[{"DisplayName":"MedidorDeNivel","Address":"100002","Value":"1"}]}]}]}
    }
}
