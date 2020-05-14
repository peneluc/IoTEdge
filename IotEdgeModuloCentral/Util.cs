using IotEdgeModuloCentral.Helpers;

namespace IotEdgeModuloCentral
{
    public static class Util
    {
        public static LogHelper Log = new LogHelper();
        public static DatabaseHelper Database = new DatabaseHelper();
        public static MessageHelper Message = new MessageHelper();
    }
}
