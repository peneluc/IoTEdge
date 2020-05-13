using IotEdgeModuloCentral.Helpers;
using IotEdgeModuloCentral.Tipos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace IotEdgeModuloCentral.Mocks
{
    public class DatabaseMock : IDatabaseHelper
    {
        public void OpenOrCreateDatabase()
        {
            Util.Log("[OpenCreateDatabase] - Mock");
            Util.Log("[OpenCreateDatabase] - Mock");
        }

        public DataTable GetAllMessage()
        {
            Util.Log("[GetAllMessage] - Mock - Criando registro");
            Util.Log("[GetAllMessage] - Mock - Criando registro");
            return new DataTable();
        }

        public DataTable GetMessage(int id)
        {
            Util.Log("[GetMessage] - Mock - Criando registro");
            Util.Log("[GetMessage] - Mock - Criando registro");
            return new DataTable();
        }

        public void AddMessage(MessageBodyIoTCentral message)
        {
            Util.Log("[AddMessage] - Mock - Criando registro");
            Util.Log("[AddMessage] - Mock - Criando registro");
        }

        public void Update(int id, MessageBodyIoTCentral message)
        {
            Util.Log("[Update] - Mock - Criando registro");
            Util.Log("[Update] - Mock - Criando registro");
        }

        public void Delete(int id)
        {
            Util.Log("[Delete] - Mock - Criando registro");
            Util.Log("[Delete] - Mock - Criando registro");
        }
    }
}
