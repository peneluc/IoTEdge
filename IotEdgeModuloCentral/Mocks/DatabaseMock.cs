using IotEdgeModuloCentral.Helpers;
using IotEdgeModuloCentral.Tipos;
using System.Data;

namespace IotEdgeModuloCentral.Mocks
{
    public class DatabaseMock : IDatabaseHelper
    {
        public void OpenOrCreateDatabase()
        {
            Util.Log.Log("[OpenCreateDatabase] - Mock");
            Util.Log.Log("[OpenCreateDatabase] - Mock");
        }

        public DataTable GetAllMessage()
        {
            Util.Log.Log("[GetAllMessage] - Mock - Criando registro");
            Util.Log.Log("[GetAllMessage] - Mock - Criando registro");
            return new DataTable();
        }

        public DataTable GetMessage(int id)
        {
            Util.Log.Log("[GetMessage] - Mock - Criando registro");
            Util.Log.Log("[GetMessage] - Mock - Criando registro");
            return new DataTable();
        }

        public void AddMessage(MessageBodyIoTCentral message)
        {
            Util.Log.Log("[AddMessage] - Mock - Criando registro");
            Util.Log.Log("[AddMessage] - Mock - Criando registro");
        }

        public void Update(int id, MessageBodyIoTCentral message)
        {
            Util.Log.Log("[Update] - Mock - Criando registro");
            Util.Log.Log("[Update] - Mock - Criando registro");
        }

        public void Delete(int id)
        {
            Util.Log.Log("[Delete] - Mock - Criando registro");
            Util.Log.Log("[Delete] - Mock - Criando registro");
        }
    }
}
