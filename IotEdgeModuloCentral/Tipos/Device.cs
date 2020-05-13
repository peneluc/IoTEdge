using System;
using System.Collections.Generic;
using System.Text;

namespace IotEdgeModuloCentral.Tipos
{
    public class Device
    {
        public string Name { get; set; }
        public string HwId { get; set; }
        public string UId { get; set; }
        public string PortRead { get; set; }
        public string PortWrite { get; set; }
    }
}
