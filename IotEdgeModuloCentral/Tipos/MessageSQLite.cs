namespace IotEdgeModuloCentral.Tipos
{
    internal class MessageSQLite
    {
        public object RequestId { get; set; }
        public string RequestModule { get; set; }
        public string DbName { get; set; }
        public string Command { get; set; }
    }
}