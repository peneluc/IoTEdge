using IotEdgeModuloCentral.Tipos;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IotEdgeModuloCentral.Helpers
{
    public class DatabaseHelper : IDatabaseHelper
    {
        private SQLiteConnection con()
        {
            SQLiteConnection con = null;
            try
            {
                if (con.State != ConnectionState.Open) 
                {
                    string cs = @"URI=file:cim.db";
                    con = new SQLiteConnection(cs);
                    con.Open();
                }
            }
            catch (Exception ex)
            {
                Util.Log($"[con] - Erro: {ex}");
            }
            return con;
        }

        public void OpenOrCreateDatabase()
        {
            try
            {
                using (var cmd = con().CreateCommand())
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
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Util.Log($"[OpenCreateDatabase] - Erro: {ex}");
            }
        }

        public DataTable GetAllMessage()
        {
            OpenOrCreateDatabase();
            SQLiteDataAdapter da = null;
            DataTable dt = new DataTable();
            try
            {
                using (var cmd = con().CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Message";
                    da = new SQLiteDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                Util.Log($"[GetAllMessage] - Erro: {ex}");
            }
            return dt;
        }

        public DataTable GetMessage(int id)
        {
            OpenOrCreateDatabase();
            SQLiteDataAdapter da = null;
            DataTable dt = new DataTable();
            try
            {
                using (var cmd = con().CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Message Where Id=@id";
                    var param = new SQLiteParameter("@id", SqlDbType.TinyInt) { Value = id };
                    cmd.Parameters.Add(param);
                    da = new SQLiteDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                Util.Log($"[GetMessage] - Erro: {ex}");
            }
            return dt;
        }

        public void AddMessage(MessageBodyIoTCentral message)
        {
            try
            {
                OpenOrCreateDatabase();
                using (var cmd = con().CreateCommand())
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

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Util.Log($"[AddMessage] - Erro: {ex}");
            }
        }

        public void Update(int id, MessageBodyIoTCentral message)
        {
            try
            {
                OpenOrCreateDatabase();

                using (var cmd = new SQLiteCommand(con()))
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

                        cmd.ExecuteNonQuery();
                    }
                };
            }
            catch (Exception ex)
            {
                Util.Log($"[Update] - Erro: {ex}");
            }
        }

        public void Delete(int id)
        {
            try
            {
                OpenOrCreateDatabase();
                using (var cmd = new SQLiteCommand(con()))
                {
                    cmd.CommandText = "DELETE FROM Message Where Id=@Id";
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Util.Log($"[Delete] - Erro: {ex}");
            }
        }

    }

}
