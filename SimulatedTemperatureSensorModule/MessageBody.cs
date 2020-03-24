using System;
using System.Collections.Generic;
using System.Text;

namespace SimulatedTemperatureSensorModule
{
    class MessageBody
    {
        public Machine machine { get; set; }
        public Ambient ambient { get; set; }
        public string timeCreated { get; set; }
    }
}
