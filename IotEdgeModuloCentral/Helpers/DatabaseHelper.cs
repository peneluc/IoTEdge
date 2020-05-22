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
                                                "NivelReservatorioSuperior Varchar(50), " +
                                                "VazaoSaida Varchar(50), " +
                                                "NivelReservatorioInferior Varchar(50), " +
                                                "VazaoEntrada Varchar(50), " +
                                                "StatusBomba1 Varchar(50), " +
                                                "StatusBomba2 Varchar(50)," +
                                                "ReservatorioSuperiorNivelPercentualAtual Varchar(50), " +
                                                "ReservatorioInferiorNivelPercentualAtual Varchar(50), " +
                                                "ReservatoriosVolumeTotalAtual Varchar(50), " +
                                                "AutonomiaBaseadaEm24horasDeConsumo Varchar(50), " +
                                                "AutonomiaBaseadaEm1HoraDeConsumo Varchar(50), " +
                                                "BombaQuantidadeAcionamentoEm24Horas Varchar(50), " +
                                                "BombaQuantidadeAcionamentoEm30Dias Varchar(50), " +
                                                "BombaFuncionamentoTempo Varchar(50), " +
                                                "MedidorVazaoConsumo30dias Varchar(50), " +
                                                "MedidorVazaoConsumo1Dia Varchar(50), " +
                                                "MedidorVazaoConsumo1Hora Varchar(50), " +
                                                "MetaConsumoMensal Varchar(50), " +
                                                "AlarmeReservatorioSuperiorAtingiuNivelMaximoAceitavel Varchar(50), " +
                                                "AlarmeReservatorioInferiorAtingiuNivelMinimoAceitavel Varchar(50), " +
                                                "AlarmeReservatorioVazamento Varchar(50))";

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
                Util.Log.Log($"[DatabaseHelper.OpenCreateDatabase] - Erro: {ex}");
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
                                              "NivelReservatorioSuperior, " +
                                              "VazaoSaida, " +
                                              "NivelReservatorioInferior, " +
                                              "VazaoEntrada, " +
                                              "StatusBomba1, " +
                                              "StatusBomba2, " +

                                              "ReservatorioSuperiorNivelPercentualAtual, " +
                                              "ReservatorioInferiorNivelPercentualAtual, " +
                                              "ReservatoriosVolumeTotalAtual, " +
                                              "AutonomiaBaseadaEm24horasDeConsumo, " +
                                              "AutonomiaBaseadaEm1HoraDeConsumo, " +
                                              "BombaQuantidadeAcionamentoEm24Horas, " +
                                              "BombaQuantidadeAcionamentoEm30Dias, " +
                                              "BombaFuncionamentoTempo, " +
                                              "MedidorVazaoConsumo30dias, " +
                                              "MedidorVazaoConsumo1Dia, " +
                                              "MedidorVazaoConsumo1Hora, " +
                                              "MetaConsumoMensal, " +
                                              "AlarmeReservatorioSuperiorAtingiuNivelMaximoAceitavel, " +
                                              "AlarmeReservatorioInferiorAtingiuNivelMinimoAceitavel, " +
                                              "AlarmeReservatorioVazamento) " +
                                      "VALUES (@HwId, " +
                                              "@PublicacaoCLP, " +
                                              "@PublicacaoModBus, " +
                                              "@PublicacaoCentral, " +
                                              "@NivelReservatorioSuperior, " +
                                              "@VazaoSaida, " +
                                              "@NivelReservatorioInferior, " +
                                              "@VazaoEntrada, " +
                                              "@StatusBomba1, " +
                                              "@StatusBomba2, " +

                                              "@ReservatorioSuperiorNivelPercentualAtual," +
                                              "@ReservatorioInferiorNivelPercentualAtual," +
                                              "@ReservatoriosVolumeTotalAtual," +
                                              "@AutonomiaBaseadaEm24horasDeConsumo," +
                                              "@AutonomiaBaseadaEm1HoraDeConsumo," +
                                              "@BombaQuantidadeAcionamentoEm24Horas," +
                                              "@BombaQuantidadeAcionamentoEm30Dias," +
                                              "@BombaFuncionamentoTempo," +
                                              "@MedidorVazaoConsumo30dias," +
                                              "@MedidorVazaoConsumo1Dia," +
                                              "@MedidorVazaoConsumo1Hora," +
                                              "@MetaConsumoMensal," +
                                              "@AlarmeReservatorioSuperiorAtingiuNivelMaximoAceitavel," +
                                              "@AlarmeReservatorioInferiorAtingiuNivelMinimoAceitavel," +
                                              "@AlarmeReservatorioVazamento)";

                    cmd.Parameters.AddWithValue("@HwId", msg.HwId);
                    cmd.Parameters.AddWithValue("@PublicacaoCLP", msg.PublicacaoCLP);
                    cmd.Parameters.AddWithValue("@PublicacaoModBus", msg.PublicacaoModBus);
                    cmd.Parameters.AddWithValue("@PublicacaoCentral", msg.PublicacaoCentral);

                    cmd.Parameters.AddWithValue("@AcionamentoBomba1", msg.AcionamentoBomba1);
                    cmd.Parameters.AddWithValue("@AcionamentoBomba2", msg.AcionamentoBomba2);
                    cmd.Parameters.AddWithValue("@HidrometroEntrada", msg.HidrometroEntrada);
                    cmd.Parameters.AddWithValue("@HidrometroSaida", msg.HidrometroSaida);
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
                        cmd.CommandText = "SELECT vazaoSaida - vazaoEntrada FROM Central " +
                                          "WHERE PublicacaoModBus >= datetime('now', '-100 day')";

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
                        cmd.CommandText = "SELECT vazaoSaida - vazaoEntrada FROM central " +
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
            public static int ObterQuantidadeAcionamentoEm24Horas()
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
            public static int ObterQuantidadeAcionamentoEm30Dias()
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
            public static int ObterMedidorVazaoConsumo1Dia()
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
            public static int ObterMedidorVazaoConsumo1Hora()
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
            public static int ObterMetaConsumoMensal()
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
        }


    }

}
