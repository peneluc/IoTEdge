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

        private static SQLiteConnection conn;
        private static SQLiteConnection GetConnection()
        {
            try
            {
                if (conn != null)
                {
                    conn.Close();
                    conn = null;
                }
                string cs = @"URI=file:cim.db;" +
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
                                      "Central (" +
                                                "id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                                "HwId Varchar(50), " +
                                                "PublicacaoCLP Varchar(50), " +
                                                "PublicacaoModBus Varchar(50), " +
                                                "PublicacaoCentral Varchar(50), " +
                                                "AcionamentoBomba1 Varchar(50), " +
                                                "AcionamentoBomba2 Varchar(50), " +
                                                "LeituraMedidorInferior Varchar(50), " +
                                                "LeituraMedidorSuperior Varchar(50), " +
                                                "StatusBomba1 Varchar(50), " +
                                                "StatusBomba2 Varchar(50), " +
                                                "StatusFalhaBomba1 Varchar(50), " +
                                                "StatusFalhaBomba2 Varchar(50), " +
                                                "SondaDeNivelInferior Varchar(50), " +
                                                "SondaDeNivelSuperior Varchar(50), " +
                                                "VolumeReservatorioSuperior Varchar(50), " +
                                                "VolumeReservatorioInferior Varchar(50), " +
                                                "VolumeTotalReservatorios Varchar(50), " +
                                                "Autonomia24h Varchar(50), " +
                                                "AutonomiaInstantanea Varchar(50), " +
                                                "QtAcionamentBomba1 Varchar(50), " +
                                                "QtAcionamentBomba2 Varchar(50), " +
                                                "PercentualAcionamentBomba1 Varchar(50), " +
                                                "PercentualAcionamentBomba2 Varchar(50), " +
                                                "TempoAcionamentoBomba1 Varchar(50), " +
                                                "TempoAcionamentoBomba2 Varchar(50), " +
                                                "PercentualTempoAcionamentoBomba1 Varchar(50), " +
                                                "PercentualTempoAcionamentoBomba2 Varchar(50), " +
                                                "ConsumoHora Varchar(50), " +
                                                "ConsumoDia Varchar(50), " +
                                                "ConsumoMes Varchar(50)) ";

                    Util.Log.Log($"[DatabaseHelper.OpenOrCreateDatabase] cmd.CommandText = {cmd.CommandText} ");
                    ExecuteNonQuery(cmd);
                }
            }
            catch (Exception ex)
            {
                Util.Log.Log($"[DatabaseHelper.OpenCreateDatabase] - Erro: {ex}");
            }
        }

        public static int ExecuteNonQuery(SQLiteCommand cmd)
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
                Util.Log.Log($"[DatabaseHelper.ExecuteNonQuery] - Erro: {ex}");
                Util.Log.Log($"[DatabaseHelper.ExecuteNonQuery] - Erro cmd: {cmd.CommandText}");
            }
            return 0;
        }

        public static DataTable ExecuteCommand(SQLiteCommand cmd)
        {
            DataTable dt = new DataTable();
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
                Util.Log.Log($"[DatabaseHelper.ExecuteDataTable] - Erro: {ex}");
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
                    dt = ExecuteCommand(cmd);
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
                using (var cmd = new SQLiteCommand())
                {
                    cmd.CommandText = "SELECT * FROM Message ORDER BY @ordenacao LIMIT @limite";
                    cmd.Parameters.AddWithValue("@ordenacao", ordenacao);
                    cmd.Parameters.AddWithValue("@limite", limite);
                    dt = ExecuteCommand(cmd);
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
                    dt = ExecuteCommand(cmd);
                    Util.Log.Log($"[DatabaseHelper.GetMessage] cmd.CommandText = {cmd.CommandText} ");
                }
            }
            catch (Exception ex)
            {
                Util.Log.Log($"[DatabaseHelper.GetMessage] - Erro: {ex}");
            }
            return dt;
        }

        public void AddMessage(MessageBodyIoTCentral msg)
        {
            try
            {

                OpenOrCreateDatabase();

                //calculando Indicadores secundarios
                var message = new Indicators().GetMessage(msg);

                using (var cmd = new SQLiteCommand())
                {
                    cmd.CommandText = "INSERT INTO Central" +
                                              "(HwId, " +
                                              "PublicacaoCLP, " +
                                              "PublicacaoModBus, " +
                                              "PublicacaoCentral, " +
                                              "AcionamentoBomba1, " +
                                              "AcionamentoBomba2, " +
                                              "LeituraMedidorInferior, " +
                                              "LeituraMedidorSuperior, " +
                                              "StatusBomba1, " +
                                              "StatusBomba2, " +
                                              "StatusFalhaBomba1, " +
                                              "StatusFalhaBomba2, " +
                                              "SondaDeNivelInferior, " +
                                              "SondaDeNivelSuperior, " +
                                              "VolumeReservatorioSuperior, " +
                                              "VolumeReservatorioInferior, " +
                                              "VolumeTotalReservatorios, " +
                                              "Autonomia24h, " +
                                              "AutonomiaInstantanea, " +
                                              "QtAcionamentBomba1, " +
                                              "QtAcionamentBomba2, " +
                                              "PercentualAcionamentBomba1, " +
                                              "PercentualAcionamentBomba2, " +
                                              "TempoAcionamentoBomba1, " +
                                              "TempoAcionamentoBomba2, " +
                                              "PercentualTempoAcionamentoBomba1, " +
                                              "PercentualTempoAcionamentoBomba2, " +
                                              "ConsumoHora, " +
                                              "ConsumoDia, " +
                                              "ConsumoMes) " +
                                              "VALUES (" +
                                              "@HwId, " +
                                              "@PublicacaoCLP, " +
                                              "@PublicacaoModBus, " +
                                              "@PublicacaoCentral, " +
                                              "@AcionamentoBomba1, " +
                                              "@AcionamentoBomba2, " +
                                              "@LeituraMedidorInferior, " +
                                              "@LeituraMedidorSuperior, " +
                                              "@StatusBomba1, " +
                                              "@StatusBomba2, " +
                                              "@StatusFalhaBomba1, " +
                                              "@StatusFalhaBomba2, " +
                                              "@SondaDeNivelInferior, " +
                                              "@SondaDeNivelSuperior, " +
                                              "@VolumeReservatorioSuperior, " +
                                              "@VolumeReservatorioInferior, " +
                                              "@VolumeTotalReservatorios, " +
                                              "@Autonomia24h, " +
                                              "@AutonomiaInstantanea, " +
                                              "@QtAcionamentBomba1, " +
                                              "@QtAcionamentBomba2, " +
                                              "@PercentualAcionamentBomba1, " +
                                              "@PercentualAcionamentBomba2, " +
                                              "@TempoAcionamentoBomba1, " +
                                              "@TempoAcionamentoBomba2, " +
                                              "@PercentualTempoAcionamentoBomba1, " +
                                              "@PercentualTempoAcionamentoBomba2, " +
                                              "@ConsumoHora, " +
                                              "@ConsumoDia, " +
                                              "@ConsumoMes)";

                    cmd.Parameters.AddWithValue("@HwId", msg.HwId);
                    cmd.Parameters.AddWithValue("@PublicacaoCLP", msg.PublicacaoCLP);
                    cmd.Parameters.AddWithValue("@PublicacaoModBus", msg.PublicacaoModBus);
                    cmd.Parameters.AddWithValue("@PublicacaoCentral", msg.PublicacaoCentral);

                    cmd.Parameters.AddWithValue("@AcionamentoBomba1", msg.AcionamentoBomba1);
                    cmd.Parameters.AddWithValue("@AcionamentoBomba2", msg.AcionamentoBomba2);
                    cmd.Parameters.AddWithValue("@LeituraMedidorInferior", msg.LeituraMedidorInferior);
                    cmd.Parameters.AddWithValue("@LeituraMedidorSuperior", msg.LeituraMedidorSuperior);
                    cmd.Parameters.AddWithValue("@StatusBomba1", msg.StatusBomba1);
                    cmd.Parameters.AddWithValue("@StatusBomba2", msg.StatusBomba2);
                    cmd.Parameters.AddWithValue("@StatusFalhaBomba1", msg.StatusFalhaBomba1);
                    cmd.Parameters.AddWithValue("@StatusFalhaBomba2", msg.StatusFalhaBomba2);
                    cmd.Parameters.AddWithValue("@SondaDeNivelInferior", msg.SondaDeNivelInferior);
                    cmd.Parameters.AddWithValue("@SondaDeNivelSuperior", msg.SondaDeNivelSuperior);

                    cmd.Parameters.AddWithValue("@VolumeReservatorioSuperior", message.VolumeReservatorioSuperior);
                    cmd.Parameters.AddWithValue("@VolumeReservatorioInferior", message.VolumeReservatorioInferior);
                    cmd.Parameters.AddWithValue("@VolumeTotalReservatorios", message.VolumeTotalReservatorios);
                    cmd.Parameters.AddWithValue("@Autonomia24h", message.Autonomia24h);
                    cmd.Parameters.AddWithValue("@AutonomiaInstantanea", message.AutonomiaInstantanea);
                    cmd.Parameters.AddWithValue("@QtAcionamentBomba1", message.QtAcionamentBomba1);
                    cmd.Parameters.AddWithValue("@QtAcionamentBomba2", message.QtAcionamentBomba2);
                    cmd.Parameters.AddWithValue("@PercentualAcionamentBomba1", message.PercentualAcionamentBomba1);
                    cmd.Parameters.AddWithValue("@PercentualAcionamentBomba2", message.PercentualAcionamentBomba2);
                    cmd.Parameters.AddWithValue("@TempoAcionamentoBomba1", message.TempoAcionamentoBomba1);
                    cmd.Parameters.AddWithValue("@TempoAcionamentoBomba2", message.TempoAcionamentoBomba2);
                    cmd.Parameters.AddWithValue("@PercentualTempoAcionamentoBomba1", message.PercentualTempoAcionamentoBomba1);
                    cmd.Parameters.AddWithValue("@PercentualTempoAcionamentoBomba2", message.PercentualTempoAcionamentoBomba2);
                    cmd.Parameters.AddWithValue("@ConsumoHora", message.ConsumoHora);
                    cmd.Parameters.AddWithValue("@ConsumoDia", message.ConsumoDia);
                    cmd.Parameters.AddWithValue("@ConsumoMes", message.ConsumoMes);

                    Util.Log.Log($"[DatabaseHelper.AddMessage] cmd.CommandText = {cmd.CommandText} ");

                    var result = ExecuteNonQuery(cmd);
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

        public class Indicator
        {
            public static int ObterConsumoTotal24Horas()
            {
                //obter valor do banco
                int valor = 0;
                DataTable dt = new DataTable();
                try
                {
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.CommandText = "SELECT (LeituraMedidorSuperior - LeituraMedidorInferior) vazao24horas  " +
                                          "  FROM Central " +
                                          " WHERE PublicacaoModBus >= datetime('now', '-1 day')";

                        dt = ExecuteCommand(cmd);
                        if(dt!=null)
                            valor = int.Parse(dt.Rows[0][0].ToString());

                        Util.Log.Log($"[DatabaseHelper.ObterConsumoTotal24Horas] cmd.CommandText = {cmd.CommandText} ");
                    }
                }
                catch (Exception ex)
                {
                    Util.Log.Log($"[DatabaseHelper.ObterConsumoTotal24Horas] - Erro: {ex}");
                }
                return valor;
            }

            public static int ObterConsumoTotalUltimaHora()
            {
                int valor = 0;
                DataTable dt = new DataTable();
                try
                {
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.CommandText = "SELECT (LeituraMedidorSuperior - LeituraMedidorInferior) vazao24horas " +
                                          "  FROM Central " +
                                          " WHERE PublicacaoModBus >= datetime('now', '-1 hour')";

                        dt = ExecuteCommand(cmd);
                        if (dt != null)
                            valor = int.Parse(dt.Rows[0][0].ToString());

                        Util.Log.Log($"[DatabaseHelper.ObterConsumoTotalUltimaHora] cmd.CommandText = {cmd.CommandText} ");
                    }
                }
                catch (Exception ex)
                {
                    Util.Log.Log($"[DatabaseHelper.ObterConsumoTotalUltimaHora] - Erro: {ex}");
                }
                return valor;
            }
            
            public static int ObterQuantidadeAcionamentoEm24Horas()
            {
                int valor = 0;
                DataTable dt = new DataTable();
                try
                {
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.CommandText = "SELECT count(*) total " +
                                          "  FROM Central " +
                                          " WHERE PublicacaoModBus >= datetime('now', '-1 day')";

                        dt = ExecuteCommand(cmd);
                        if (dt != null)
                            valor = int.Parse(dt.Rows[0][0].ToString());

                        Util.Log.Log($"[DatabaseHelper.ObterQuantidadeAcionamentoEm24Horas] cmd.CommandText = {cmd.CommandText} ");
                    }
                }
                catch (Exception ex)
                {
                    Util.Log.Log($"[DatabaseHelper.ObterQuantidadeAcionamentoEm24Horas] - Erro: {ex}");
                }
                return valor;
            }
            public static int ObterQuantidadeAcionamentoEm30Dias()
            {
                int valor = 0;
                DataTable dt = new DataTable();
                try
                {
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.CommandText = "SELECT count(*) total " +
                                          "  FROM Central " +
                                          " WHERE PublicacaoModBus >= datetime('now', '-30 day')";

                        dt = ExecuteCommand(cmd);
                        if (dt != null)
                            valor = int.Parse(dt.Rows[0][0].ToString());

                        Util.Log.Log($"[DatabaseHelper.ObterConsumoTotal24Horas] cmd.CommandText = {cmd.CommandText} ");
                    }
                }
                catch (Exception ex)
                {
                    Util.Log.Log($"[DatabaseHelper.ObterConsumoTotal24Horas] - Erro: {ex}");
                }
                return valor;
            }
            public static int ObterBombaFuncionamentoTempo()
            {
                int valor = 0;
                DataTable dt = new DataTable();
                try
                {
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.CommandText = "SELECT PublicacaoModBus FROM central " +
                                          "WHERE PublicacaoModBus >= datetime('now', '-100 day')";

                        dt = ExecuteCommand(cmd);
                        if (dt != null)
                            valor = int.Parse(dt.Rows[0][0].ToString());

                        Util.Log.Log($"[DatabaseHelper.ObterConsumoTotal24Horas] cmd.CommandText = {cmd.CommandText} ");
                    }
                }
                catch (Exception ex)
                {
                    Util.Log.Log($"[DatabaseHelper.ObterConsumoTotal24Horas] - Erro: {ex}");
                }
                return valor;
            }
            public static int ObterMedidorVazaoConsumo30dias()
            {
                int valor = 0;
                DataTable dt = new DataTable();
                try
                {
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.CommandText = "SELECT (LeituraMedidorSuperior - LeituraMedidorInferior) vazao30dias " +
                                          "  FROM Central " +
                                          " WHERE PublicacaoModBus >= datetime('now', '-30 day')";

                        dt = ExecuteCommand(cmd);
                        if (dt != null)
                            valor = int.Parse(dt.Rows[0][0].ToString());

                        Util.Log.Log($"[DatabaseHelper.ObterConsumoTotal24Horas] cmd.CommandText = {cmd.CommandText} ");
                    }
                }
                catch (Exception ex)
                {
                    Util.Log.Log($"[DatabaseHelper.ObterConsumoTotal24Horas] - Erro: {ex}");
                }
                return valor;
            }
            public static int ObterMedidorVazaoConsumo1Dia()
            {
                int valor = 0;
                DataTable dt = new DataTable();
                try
                {
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.CommandText = "SELECT (LeituraMedidorSuperior - LeituraMedidorInferior) vazao24horas " +
                                          "  FROM Central " +
                                          " WHERE PublicacaoModBus >= datetime('now', '-1 day')";

                        dt = ExecuteCommand(cmd);
                        if (dt != null)
                            valor = int.Parse(dt.Rows[0][0].ToString());

                        Util.Log.Log($"[DatabaseHelper.ObterConsumoTotal24Horas] cmd.CommandText = {cmd.CommandText} ");
                    }
                }
                catch (Exception ex)
                {
                    Util.Log.Log($"[DatabaseHelper.ObterConsumoTotal24Horas] - Erro: {ex}");
                }
                return valor;
            }
            public static int ObterMedidorVazaoConsumo1Hora()
            {
                int valor = 0;
                DataTable dt = new DataTable();
                try
                {
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.CommandText = "SELECT (LeituraMedidorSuperior - LeituraMedidorInferior) vazao1hora " +
                                          "  FROM Central " +
                                          " WHERE PublicacaoModBus >= datetime('now', '-1 hour')";

                        dt = ExecuteCommand(cmd);
                        if (dt != null)
                            valor = int.Parse(dt.Rows[0][0].ToString());

                        Util.Log.Log($"[DatabaseHelper.ObterConsumoTotal24Horas] cmd.CommandText = {cmd.CommandText} ");
                    }
                }
                catch (Exception ex)
                {
                    Util.Log.Log($"[DatabaseHelper.ObterConsumoTotal24Horas] - Erro: {ex}");
                }
                return valor;
            }

            public static DataTable ObterParametros()
            {
                DataTable dt = null;
                try
                {
                    using (var cmd = new SQLiteCommand("SELECT * FROM Parametros"))
                    {
                        dt = ExecuteCommand(cmd);
                        Util.Log.Log($"[DatabaseHelper.GetAllMessage] cmd.CommandText = {cmd.CommandText} ");
                    }
                }
                catch (Exception ex)
                {
                    Util.Log.Log($"[DatabaseHelper.GetAllMessage] - Erro: {ex}");
                }
                return dt;
            }
        }


    }

}
