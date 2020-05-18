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
                var message = IndicatorHelper.CalcularIndicadores(msg);

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
                    cmd.Parameters.AddWithValue("@NivelReservatorioSuperior", msg.NivelReservatorioSuperior);
                    cmd.Parameters.AddWithValue("@VazaoSaida", msg.VazaoSaida);
                    cmd.Parameters.AddWithValue("@NivelReservatorioInferior", msg.NivelReservatorioInferior);
                    cmd.Parameters.AddWithValue("@VazaoEntrada", msg.VazaoEntrada);
                    cmd.Parameters.AddWithValue("@StatusBomba1", msg.StatusBomba1);
                    cmd.Parameters.AddWithValue("@StatusBomba2", msg.StatusBomba2);

                    cmd.Parameters.AddWithValue("@ReservatorioSuperiorNivelPercentualAtual", message.ReservatorioSuperiorNivelPercentualAtual);
                    cmd.Parameters.AddWithValue("@ReservatorioInferiorNivelPercentualAtual", message.ReservatorioInferiorNivelPercentualAtual);
                    cmd.Parameters.AddWithValue("@ReservatoriosVolumeTotalAtual", message.ReservatoriosVolumeTotalAtual);
                    cmd.Parameters.AddWithValue("@AutonomiaBaseadaEm24horasDeConsumo", message.AutonomiaBaseadaEm24horasDeConsumo);
                    cmd.Parameters.AddWithValue("@AutonomiaBaseadaEm1HoraDeConsumo", message.AutonomiaBaseadaEm1HoraDeConsumo);
                    cmd.Parameters.AddWithValue("@BombaQuantidadeAcionamentoEm24Horas", message.BombaQuantidadeAcionamentoEm24Horas);
                    cmd.Parameters.AddWithValue("@BombaQuantidadeAcionamentoEm30Dias", message.BombaQuantidadeAcionamentoEm30Dias);
                    cmd.Parameters.AddWithValue("@BombaFuncionamentoTempo", message.BombaFuncionamentoTempo);
                    cmd.Parameters.AddWithValue("@MedidorVazaoConsumo30dias", message.MedidorVazaoConsumo30dias);
                    cmd.Parameters.AddWithValue("@MedidorVazaoConsumo1Dia", message.MedidorVazaoConsumo1Dia);
                    cmd.Parameters.AddWithValue("@MedidorVazaoConsumo1Hora", message.MedidorVazaoConsumo1Hora);
                    cmd.Parameters.AddWithValue("@MetaConsumoMensal", message.MetaConsumoMensal);
                    cmd.Parameters.AddWithValue("@AlarmeReservatorioSuperiorAtingiuNivelMaximoAceitavel", message.AlarmeReservatorioSuperiorAtingiuNivelMaximoAceitavel);
                    cmd.Parameters.AddWithValue("@AlarmeReservatorioInferiorAtingiuNivelMinimoAceitavel", message.AlarmeReservatorioInferiorAtingiuNivelMinimoAceitavel);
                    cmd.Parameters.AddWithValue("@AlarmeReservatorioVazamento", message.AlarmeReservatorioVazamento);

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

        public class Indicator
        {
            public static float ObterConsumoTotal24Horas()
            {
                //obter valor do banco
                float valor = 0f;
                DataTable dt = new DataTable();
                try
                {
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.CommandText = "SELECT vazaoSaida - vazaoEntrada FROM Central " +
                                          "WHERE PublicacaoModBus >= datetime('now', '-100 day')";

                        dt = ExecuteCommand(cmd);
                        if(dt!=null)
                            valor = float.Parse(dt.Rows[0][0].ToString());

                        Util.Log.Log($"[DatabaseHelper.ObterConsumoTotal24Horas] cmd.CommandText = {cmd.CommandText} ");
                    }
                }
                catch (Exception ex)
                {
                    Util.Log.Log($"[DatabaseHelper.ObterConsumoTotal24Horas] - Erro: {ex}");
                }
                return valor;
            }
            public static float ObterConsumoTotalUltimaHora()
            {
                float valor = 0f;
                DataTable dt = new DataTable();
                try
                {
                    using (var cmd = new SQLiteCommand())
                    {
                        cmd.CommandText = "SELECT vazaoSaida - vazaoEntrada FROM central " +
                                          "WHERE PublicacaoModBus >= datetime('now', '-100 day')";

                        dt = ExecuteCommand(cmd);
                        if (dt != null)
                            valor = float.Parse(dt.Rows[0][0].ToString());

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
