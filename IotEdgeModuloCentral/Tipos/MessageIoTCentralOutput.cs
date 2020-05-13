using System;

namespace IotEdgeModuloCentral.Tipos
{
    /*
        {
            "PublishTimestamp": "2020-04-06 00:52:03",
            "HwId": "AAAAA555555",
            "SourceTimestamp": "2020-04-06 00:52:03",
            "DisplayName": "AAAAA555555",
            "Value": false
        }
    */

    public class MessageIoTCentralOutput
    {
        public DateTime PublishTimestamp { get; set; }
        public string HwId { get; set; }
        public DateTime SourceTimestamp { get; set; }
        public string DisplayName { get; set; }
        public int Value { get; set; }
    }
}
