namespace IotEdgeModuloCentral.Tipos
{
    class ModbusMessage
    {
        public string HwId { get; set; }
        public string UId { get; set; }
        public string Address { get; set; }
        public string Value { get; set; }
    }
}
