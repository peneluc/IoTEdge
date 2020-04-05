using System;
using System.Collections.Generic;

namespace IotEdgeModuloCentral.Tipos
{
    class Data
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
        public string CorrelationId { get; set; }
        public DateTime SourceTimestamp { get; set; }
        public List<Values> Values { get; set; }
    }
}
