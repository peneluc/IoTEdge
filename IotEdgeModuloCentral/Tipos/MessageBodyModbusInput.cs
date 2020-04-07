namespace IotEdgeModuloCentral.Tipos
{
    class MessageBodyModbusInput
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
        public string HwId { get; set; }
        public string UId { get; set; }
        public string Address { get; set; }
        public string Value { get; set; }
    }
}
