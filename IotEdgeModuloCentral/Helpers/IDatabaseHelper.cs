using IotEdgeModuloCentral.Tipos;
using System.Data;

namespace IotEdgeModuloCentral.Helpers
{
    public interface IDatabaseHelper
    {
        void OpenOrCreateDatabase();

        DataTable GetAllMessage();

        DataTable GetMessage(int id);

        void AddMessage(MessageBodyIoTCentral message);

        void Delete(int id);
    }
}