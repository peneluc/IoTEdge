using IotEdgeModuloCentral.Tipos;
using System;
using System.Data;
using System.Data.SQLite;

namespace IotEdgeModuloCentral.Helpers
{
    public class DatabaseHelper : IDatabaseHelper
    {
        public DatabaseHelper()
        {
            Util.Log.Log($"[DatabaseHelper] Iniciando helper...");
        }

        private SQLiteConnection conn;
        private SQLiteConnection GetConnection()
        {
            try
            {
                if (conn != null)
                {
                    conn.Close();
                    conn = null;
                }
                string cs = @"URI=file:cim.db;"+
                            "Version=3;Mode=ReadWrite;New=False;Compress=True;Journal Mode=Off;";
                conn = new SQLiteConnection(cs);
                conn.Open();

                Util.Log.Log($"[DatabaseHelper.GetConnection] conn.State = {conn.State} " +
                             $"- conn.ConnectionString = {conn.ConnectionString} " +
                             $"- conn.FileName = {conn.FileName} ");
            }
            catch (Exception ex)
            {
                Util.Log.Error($"[DatabaseHelper.GetConnection] - Erro: {ex}");
            }
            return conn;
        }

        public void OpenOrCreateDatabase()
        {
            try
            {
                using (var cmd = new SQLiteCommand())
                {
                    cmd.CommandText = "CREATE TABLE IF NOT EXISTS " +
                                      "Message (" +
                                                "id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                                "HwId Varchar(50), " +
                                                "PublicacaoCLP Varchar(50), " +
                                                "PublicacaoModBus Varchar(50), " +
                                                "PublicacaoCentral Varchar(50), " +
                                                "NivelReservatorioSuperior Varchar(50), " +
                                                "VazaoSaida Varchar(50), " +
                                                "NivelReservatorioInferior Varchar(50), " +
                                                "VazaoEntrada Varchar(50), " +
                                                "StatusBomba1 Varchar(50), " +
                                                "StatusBomba2 Varchar(50))";

                    Util.Log.Log($"[DatabaseHelper.OpenOrCreateDatabase] cmd.CommandText = {cmd.CommandText} ");
                    ExecuteNonQuery(cmd);
                }
            }
            catch (Exception ex)
            {
                Util.Log.Log($"[DatabaseHelper.OpenCreateDatabase] - Erro: {ex}");
            }
        }

        public int ExecuteNonQuery(SQLiteCommand cmd)
        {
            try
            {
                if (cmd != null)
                {
                    cmd.Connection = GetConnection();
                    var rows = cmd.ExecuteNonQuery();
                    cmd = null;
                    return rows;
                }
            }
            catch (Exception ex)
            {
                Util.Log.Log($"[DatabaseHelper.OpenCreateDatabase] - Erro: {ex}");
            }
            return 0;
        }

        public DataTable ExecuteDataTable(SQLiteCommand cmd)
        {
            DataTable dt = null;
            try
            {
                if (cmd != null)
                {
                    SQLiteDataAdapter da = null;
                    cmd.Connection = GetConnection();
                    da = new SQLiteDataAdapter(cmd);
                    da.Fill(dt);
                    cmd = null;
                    da = null;
                }
            }
            catch (Exception ex)
            {
                Util.Log.Log($"[DatabaseHelper.OpenCreateDatabase] - Erro: {ex}");
            }
            return dt;
        }

        public DataTable GetAllMessage()
        {
            DataTable dt = null;
            try
            {
                using (var cmd = new SQLiteCommand("SELECT * FROM Message"))
                {
                    dt = ExecuteDataTable(cmd);
                    Util.Log.Log($"[DatabaseHelper.GetAllMessage] cmd.CommandText = {cmd.CommandText} ");
                }
            }
            catch (Exception ex)
            {
                Util.Log.Log($"[DatabaseHelper.GetAllMessage] - Erro: {ex}");
            }
            return dt;
        }
        public DataTable GetAllMessage(int limite, string ordenacao)
        {
            DataTable dt = new DataTable();
            try
            {
                using (var cmd = GetConnection().CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Message ORDER BY @ordenacao LIMIT @limite";
                    cmd.Parameters.AddWithValue("@ordenacao", ordenacao);
                    cmd.Parameters.AddWithValue("@limite", limite);
                    dt = ExecuteDataTable(cmd);
                    Util.Log.Log($"[DatabaseHelper.GetAllMessage] cmd.CommandText = {cmd.CommandText} ");
                }
            }
            catch (Exception ex)
            {
                Util.Log.Log($"[DatabaseHelper.GetAllMessage] - Erro: {ex}");
            }
            return dt;
        }

        public DataTable GetMessage(int id)
        {
            DataTable dt = null;
            try
            {
                using (var cmd = new SQLiteCommand())
                {
                    cmd.CommandText = "SELECT * FROM Message Where Id=@id";
                    var param = new SQLiteParameter("@id", SqlDbType.TinyInt) { Value = id };
                    cmd.Parameters.Add(param);
                    dt = ExecuteDataTable(cmd);
                    Util.Log.Log($"[DatabaseHelper.GetMessage] cmd.CommandText = {cmd.CommandText} ");
                }
            }
            catch (Exception ex)
            {
                Util.Log.Log($"[DatabaseHelper.GetMessage] - Erro: {ex}");
            }
            return dt;
        }

        public void AddMessage(MessageBodyIoTCentral message)
        {
            try
            {
                using (var cmd = new SQLiteCommand())
                {
                    cmd.CommandText = "INSERT INTO " +
                                      "Message(HwId, " +
                                              "PublicacaoCLP, " +
                                              "PublicacaoModBus, " +
                                              "PublicacaoCentral, " +
                                              "NivelReservatorioSuperior, " +
                                              "VazaoSaida, " +
                                              "NivelReservatorioInferior, " +
                                              "VazaoEntrada, " +
                                              "StatusBomba1, " +
                                              "StatusBomba2) " +
                                      "VALUES (@HwId, " +
                                              "@PublicacaoCLP, " +
                                              "@PublicacaoModBus, " +
                                              "@PublicacaoCentral, " +
                                              "@NivelReservatorioSuperior, " +
                                              "@VazaoSaida, " +
                                              "@NivelReservatorioInferior, " +
                                              "@VazaoEntrada, " +
                                              "@StatusBomba1, " +
                                              "@StatusBomba2)";
                    cmd.Parameters.AddWithValue("@HwId", message.HwId);
                    cmd.Parameters.AddWithValue("@PublicacaoCLP", message.PublicacaoCLP);
                    cmd.Parameters.AddWithValue("@PublicacaoModBus", message.PublicacaoModBus);
                    cmd.Parameters.AddWithValue("@PublicacaoCentral", message.PublicacaoCentral);
                    cmd.Parameters.AddWithValue("@NivelReservatorioSuperior", message.NivelReservatorioSuperior);
                    cmd.Parameters.AddWithValue("@VazaoSaida", message.VazaoSaida);
                    cmd.Parameters.AddWithValue("@NivelReservatorioInferior", message.NivelReservatorioInferior);
                    cmd.Parameters.AddWithValue("@VazaoEntrada", message.VazaoEntrada);
                    cmd.Parameters.AddWithValue("@StatusBomba1", message.StatusBomba1);
                    cmd.Parameters.AddWithValue("@StatusBomba2", message.StatusBomba2);

                    Util.Log.Log($"[DatabaseHelper.AddMessage] cmd.CommandText = {cmd.CommandText} ");

                    ExecuteNonQuery(cmd);
                }
            }
            catch (Exception ex)
            {
                Util.Log.Log($"[DatabaseHelper.AddMessage] - Erro: {ex}");
            }
        }

        internal void PrintRows(int v1, string v2)
        {
            try
            {
                var cont = 0;
                var dt = GetAllMessage(v1, v2).CreateDataReader();
                while ((dt.Read()) && (cont < v1))
                {
                    for (int i = 0; i < dt.FieldCount; i++)
                    {
                        Console.WriteLine(dt.GetValue(i));
                    }
                    Console.WriteLine();
                    cont++;
                }
            }
            catch (Exception ex)
            {
                Util.Log.Log($"[DatabaseHelper.PrintRows] - Erro: {ex}");
            }
        }

        public void Update(int id, MessageBodyIoTCentral message)
        {
            try
            {
                using (var cmd = new SQLiteCommand())
                {
                    if ((id > 0) && (message != null))
                    {
                        cmd.CommandText = "UPDATE Message SET " +
                                                  "HwId = @HwId, " +
                                                  "PublicacaoCLP = @PublicacaoCLP, " +
                                                  "PublicacaoModBus = @PublicacaoModBus, " +
                                                  "PublicacaoCentral = @PublicacaoCentral, " +
                                                  "NivelReservatorioSuperior = @NivelReservatorioSuperior, " +
                                                  "VazaoSaida = @VazaoSaida, " +
                                                  "NivelReservatorioInferior = @NivelReservatorioInferior, " +
                                                  "VazaoEntrada = @VazaoEntrada, " +
                                                  "StatusBomba1 = @StatusBomba1, " +
                                                  "StatusBomba2 = @StatusBomba2 " +
                                          "WHERE Id=@Id";
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@HwId", message.HwId);
                        cmd.Parameters.AddWithValue("@PublicacaoCLP", message.PublicacaoCLP);
                        cmd.Parameters.AddWithValue("@PublicacaoModBus", message.PublicacaoModBus);
                        cmd.Parameters.AddWithValue("@PublicacaoCentral", message.PublicacaoCentral);
                        cmd.Parameters.AddWithValue("@NivelReservatorioSuperior", message.NivelReservatorioSuperior);
                        cmd.Parameters.AddWithValue("@VazaoSaida", message.VazaoSaida);
                        cmd.Parameters.AddWithValue("@NivelReservatorioInferior", message.NivelReservatorioInferior);
                        cmd.Parameters.AddWithValue("@VazaoEntrada", message.VazaoEntrada);
                        cmd.Parameters.AddWithValue("@StatusBomba1", message.StatusBomba1);
                        cmd.Parameters.AddWithValue("@StatusBomba2", message.StatusBomba2);

                        Util.Log.Log($"[DatabaseHelper.Update] cmd.CommandText = {cmd.CommandText} ");

                        ExecuteNonQuery(cmd);
                    }
                };
            }
            catch (Exception ex)
            {
                Util.Log.Log($"[DatabaseHelper.Update] - Erro: {ex}");
            }
        }

        public void Delete(int id)
        {
            try
            {
                using (var cmd = new SQLiteCommand())
                {
                    cmd.CommandText = "DELETE FROM Message Where Id=@Id";
                    cmd.Parameters.AddWithValue("@Id", id);
                    ExecuteNonQuery(cmd);

                    Util.Log.Log($"[DatabaseHelper.Delete] cmd.CommandText = {cmd.CommandText} ");
                }
            }
            catch (Exception ex)
            {
                Util.Log.Log($"[DatabaseHelper.Delete] - Erro: {ex}");
            }
        }

    }

}
