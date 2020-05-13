using IotEdgeModuloCentral.Tipos;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IotEdgeModuloCentral.Helpers
{
    public static class MessageHelper
    {

        #region Variaveis

        /// <summary>
        /// Propriedade que armazena o contador de mensagens *encaminhadas*
        /// </summary>
        private static int _totalMensagensEncaminhadas;

        /// <summary>
        /// Propriedade que armazena o contador de mensagens *recebidas*
        /// </summary>
        private static int _totalMensagensRecebidas;

        /// <summary>
        /// Propriedade com nome do módulo de *saída* das mensagens
        /// </summary>
        private static Device medidoDeNivel = null;
        public static Device MedidoDeNivel { get => medidoDeNivel; set => medidoDeNivel = value; }
        public static void StartObjects()
        {
            MedidoDeNivel = new Device();
            MedidoDeNivel.Name = "Rasp001";
            MedidoDeNivel.HwId = "GTI-Device";
            MedidoDeNivel.UId = "1";
            MedidoDeNivel.PortRead = "10002";
            MedidoDeNivel.PortWrite = "00009";
        }

        #endregion

        #region Constantes

        /// <summary>
        /// Propriedade com nome do módulo de *entrada* das mensagens
        /// "modbusParaModuloCentral": 
        /// "FROM /messages/modules/modbusToIoTHub/outputs/* 
        /// INTO BrokeredEndpoint(\"/modules/IotEdgeModuloCentral/inputs/input1\")",
        /// </summary>
        public const string CONST_MODCENTRAL_INPUT_MSG_FROM_MODBUS = "input1";

        /// <summary>
        /// Propriedade com nome do módulo de *entrada* das mensagens
        /// </summary>
        public const string CONST_MODCENTRAL_INPUT_MSG_FROM_SQLITE = "input3";

        /// <summary>
        /// Propriedade com nome do módulo de *saída* das mensagens
        /// "moduloCentralParaIotCentral": 
        /// "FROM /messages/modules/IotEdgeModuloCentral/outputs/output1/* 
        /// INTO $upstream",
        /// </summary>
        public const string CONST_MODCENTRAL_OUTPUT_MSG_TO_IOTCENTRAL = "output1";

        /// <summary>
        /// Propriedade com nome do módulo de *saída* das mensagens
        /// "moduloCentralParaModbus": 
        /// "FROM /messages/modules/IotEdgeModuloCentral/outputs/output2/* 
        /// INTO BrokeredEndpoint(\"/modules/modbusToIoTHub/inputs/input1\")",
        /// </summary>
        public const string CONST_MODCENTRAL_OUTPUT_MSG_TO_MODBUS = "output2";

        /// <summary>
        /// Propriedade com nome do módulo de *saída* das mensagens
        /// </summary>
        public const string CONST_MODCENTRAL_OUTPUT_MSG_TO_SQLITE = "output3";

        /// <summary>
        /// Propriedade com nome do módulo de *saída* das mensagens
        /// </summary>
        public const string CONST_DEVICE_NAME_WRITE_PORT = "BombaPortaEscrita";

        #endregion


        #region Suporte

        public static int ObterValorInteiroRealOuSimulado(Values value)
        {
            Random rnd = new Random();
            int retorno = 0;

            if (string.IsNullOrEmpty(value.Value))
                retorno = rnd.Next(1, 100);
            else
                retorno = int.Parse(value.Value);

            return retorno;
        }

        public static bool ObterValorBooleanoRealOuSimulado(Values value)
        {
            Random rnd = new Random();
            bool retorno = false;

            if (string.IsNullOrEmpty(value.Value))
                retorno = rnd.Next(1, 100) < 50;
            else
                retorno = bool.Parse(value.Value);

            return retorno;
        }

        private static void MessageSendCount(string message)
        {
            var totalMensagensEncaminhadas = Interlocked.Increment(ref _totalMensagensEncaminhadas);
            Util.LogFixo($"[MessageSendCount] - Info: ***  Mensagem encaminhada! Total: {totalMensagensEncaminhadas}, Corpo da Mensagem: {message} ***");
        }

        private static void MessageReceivedCount(string message)
        {
            var totalMensagensRecebidas = Interlocked.Increment(ref _totalMensagensRecebidas);
            Util.LogFixo($"[MessageSendCount] - Info: ***  Mensagem encaminhada! Total: {totalMensagensRecebidas}, Corpo da Mensagem: {message} ***");
        }

        #endregion

        #region Enviar Comandos

        public static async Task<MethodResponse> LigarMedidorDeNivel(MethodRequest methodRequest, object userContext)
        {
            Util.Log($"[LigarMedidoDeNivel] - Info: Inicio - {DateTime.Now}");
            var retorno = await EnviarMensagemMedidorDeNivel("1", userContext);
            Util.Log($"[LigarMedidoDeNivel] - Info: Fim - {DateTime.Now}");
            return retorno;
        }

        public static async Task<MethodResponse> DesligarMedidorDeNivel(MethodRequest methodRequest, object userContext)
        {
            Util.Log($"[DesligarMedidoDeNivel] - Info: Inicio - {DateTime.Now}");
            var retorno = await EnviarMensagemMedidorDeNivel("0", userContext);
            Util.Log($"[DesligarMedidoDeNivel] - Info: Fim - {DateTime.Now}");
            return retorno;
        }

        public static async Task<MethodResponse> PiscarLED(MethodRequest methodRequest, object userContext)
        {
            Util.Log($"[PiscarLED] - Inicio");
            for (int i = 0; i < 10; i++)
            {
                await LigarMedidorDeNivel(null, userContext);
                await Task.Delay(1000);
                await DesligarMedidorDeNivel(null, userContext);
            }
            Util.Log($"[PiscarLED] - Fim");
            return new MethodResponse(new byte[0], 200);
        }

        public static MessageBodyIoTCentral CriandoMensagemIoTCentral(MessageBodyModbusOutput messageBodyModbus)
        {
            MessageBodyIoTCentral messageBodyIoTCentral = new MessageBodyIoTCentral();
            try
            {
                /*
                 * ENTRADA
                    {
                      "PublishTimestamp": "2020-04-19 03:19:08",
                      "Content": [
                        {
                          "HwId": "GTI-Device",
                          "Data": [
                            {
                              "CorrelationId": "DefaultCorrelationId",
                              "SourceTimestamp": "2020-04-19 03:19:04",
                              "Values": [
                                {
                                  "DisplayName": "StatusBomba",
                                  "Address": "00009",
                                  "Value": "0"
                                }
                              ]
                            },
                            {
                              "CorrelationId": "DefaultCorrelationId",
                              "SourceTimestamp": "2020-04-19 03:19:05",
                              "Values": [
                                {
                                  "DisplayName": "MedidorDeNivel",
                                  "Address": "10002",
                                  "Value": "1"
                                }
                              ]
                            }
                          ]
                        }
                      ]
                    }

                SAIDA

                    {
                        "HwId": "1",
                        "PublicacaoCLP": "2020-04-06 00:52:03",
                        "PublicacaoModBus": "2020-04-06 00:52:03",
                        "PublicacaoCentral": "2020-04-06 00:52:03",
                        "NivelReservatorioSuperior": 850,
                        "VazaoSaida": 250,
                        "NivelReservatorioInferior": 850,
                        "VazaoEntrada": 250,
                        "StatusBomba1": false,
                        "StatusBomba2”: false,
                        "ValvulaCorte": true
                    }
                */
                if (messageBodyModbus != null)
                {
                    //PublishTimestamp
                    messageBodyIoTCentral.PublicacaoModBus = messageBodyModbus.PublishTimestamp;
                    messageBodyIoTCentral.PublicacaoCentral = DateTime.Now;
                    if (messageBodyModbus.Content != null && messageBodyModbus.Content.Count > 0)
                    {
                        //HwId
                        messageBodyIoTCentral.HwId = messageBodyModbus.Content[0].HwId;

                        //throw new Exception("mensagem alterada, confirmar novo padrão");

                        //Trata corpo da mensagem
                        if (messageBodyModbus.Content[0].Data != null && messageBodyModbus.Content[0].Data.Count > 0)
                        {
                            foreach (Data data in messageBodyModbus.Content[0].Data)
                            {
                                foreach (Values value in data.Values)
                                {
                                    switch (value.DisplayName)
                                    {
                                        case "NivelSuperior":
                                            messageBodyIoTCentral.NivelReservatorioSuperior = ObterValorInteiroRealOuSimulado(null);
                                            break;
                                        case "NivelInferior":
                                            messageBodyIoTCentral.NivelReservatorioInferior = ObterValorInteiroRealOuSimulado(null);
                                            break;
                                        case "VazaoEntrada":
                                            messageBodyIoTCentral.VazaoEntrada = ObterValorInteiroRealOuSimulado(null);
                                            break;
                                        case "VazaoSaida":
                                            messageBodyIoTCentral.VazaoSaida = ObterValorInteiroRealOuSimulado(null);
                                            break;
                                        case "StatusBomba1":
                                            messageBodyIoTCentral.StatusBomba1 = ObterValorBooleanoRealOuSimulado(null);
                                            break;
                                        case "StatusBomba2":
                                            messageBodyIoTCentral.StatusBomba2 = ObterValorBooleanoRealOuSimulado(null);
                                            break;
                                        case "FalhaBomba1":
                                            messageBodyIoTCentral.FalhaBomba1 = ObterValorBooleanoRealOuSimulado(null);
                                            break;
                                        case "FalhaBomba2":
                                            messageBodyIoTCentral.FalhaBomba2 = ObterValorBooleanoRealOuSimulado(null);
                                            break;
                                        default:
                                            Util.Log($"Nome de dispositivo nao reconhecido: {value}");
                                            break;
                                    }
                                }
                            }
                        }

                        //calculando Indicadores secundarios
                        Indicator.CalcularIndicadores(messageBodyIoTCentral);

                    }
                }

            }
            catch (Exception ex)
            {
                Util.LogErro($"Erro ao transformar objetos: {ex}");
            }
            Util.Database.AddMessage(messageBodyIoTCentral);
            return messageBodyIoTCentral;
        }

        #endregion

        #region Enviar Mensagens

        public static async Task<string> EnviarMensagemIoTCentral(MessageBodyIoTCentral messageBodyIoTCentral, Message message, ModuleClient moduleClient)
        {
            string messageString = string.Empty;
            byte[] messageBytes = new byte[0];
            Util.Log($"[EnviarMensagemIoTCentral] - Info: 1 - Captura corpo de [messageBodyIoTCentral] em formato de bytes array");
            try
            {
                string json = JsonConvert.SerializeObject(messageBodyIoTCentral);
                messageBytes = Encoding.UTF8.GetBytes(json);
            }
            catch (Exception ex)
            {
                Util.LogErro($"Erro ao capturar byteArray: {ex}");
            }

            Util.Log($"[EnviarMensagemIoTCentral] - Info: 2 - Cria evento e envia msg");
            using (var filteredMessage = new Message(messageBytes))
            {
                foreach (KeyValuePair<string, string> prop in message.Properties)
                {
                    filteredMessage.Properties.Add(prop.Key, prop.Value);
                    Util.Log($"***  filteredMessage.Properties.Add({prop.Key}, {prop.Value}) ***");
                }

                Util.Log($"[FilterMessages] - Info: 3 - Envia evento de alerta");
                filteredMessage.Properties.Add("MessageType", "Alert");
                await moduleClient.SendEventAsync(CONST_MODCENTRAL_OUTPUT_MSG_TO_IOTCENTRAL, filteredMessage); // <---------- envia mensagem iot central <-----------

                Util.Log($"[FilterMessages] - Info: 4 - Converte corpo da mensagem em string e registra envio");
                messageString = Encoding.UTF8.GetString(messageBytes);
                MessageSendCount(messageString);
            }
            Util.Log($"[EnviarMensagemIoTCentral] - Info: 5 - Captura corpo de [messageBodyIoTCentral] em formato de bytes array");

            return messageString;
        }

        public static async Task<MethodResponse> EnviarMensagemModbus(string HwId, string UId, string Address, string Value, object userContext)
        {
            Util.Log($"[EnviarMensagemModbus] - Info: Inicio - {DateTime.Now}");

            Util.Log($"[EnviarMensagemModbus] - Info: 1 - Inicializa instancia do ModuleClient");
            try
            {
                ModuleClient moduleClient = (ModuleClient)userContext;
                if (moduleClient == null)
                {
                    throw new InvalidOperationException("Módulo cliente não instanciado");
                }

                Util.Log($"[EnviarMensagemModbus] - Info: 2 - Esse metodo eh chamado sempre que enviamos uma mensagem para o Modulo ModBus");

                /*
                    {
                      "PublishTimestamp": "2020-04-06 00:52:03",
                      "HwId": "AAAAA555555",
                      "SourceTimestamp": "2020-04-06 00:52:03",
                      "DisplayName": "AAAAA555555",
                      "Value": false
                    }
                */
                var modbusMessageBody = new MessageBodyModbusInput
                {
                    HwId = HwId,
                    UId = UId,
                    Address = Address,
                    Value = Value,
                };

                Util.Log($"[EnviarMensagemModbus] - Info: 3 - Serializa o objeto em json");
                var jsonMessage = JsonConvert.SerializeObject(modbusMessageBody);

                Util.Log($"[EnviarMensagemModbus] - Info: 4 - Transforma o json em array de bytes");
                var bytes = System.Text.Encoding.UTF8.GetBytes(jsonMessage);

                Util.Log($"[EnviarMensagemModbus] - Info: 5 - Cria mensagem de input do ModBus e adiciona o array de bytes no corpo");
                var pipeMessage = new Message(bytes);

                Util.Log($"[EnviarMensagemModbus] - Info: 6 - Adiciona propriedade de escrita a mensagem do ModBus");
                pipeMessage.Properties.Add("command-type", "ModbusWrite");

                Util.Log($"[EnviarMensagemModbus] - Info: 7 - Envia a mensagem para o Modulo ModBus");
                await moduleClient.SendEventAsync(MessageHelper.CONST_MODCENTRAL_OUTPUT_MSG_TO_MODBUS, pipeMessage);

                MessageSendCount(jsonMessage);

                Util.Log($"[EnviarMensagemModbus] - Info: Fim - {DateTime.Now}");
                return
                    //new MethodResponse(new byte[0], 200);
                    new MethodResponse(Encoding.UTF8.GetBytes("{\"EnviarMensagemModbus\":\"" + (Value) + "\"}"), 200);
            }
            catch (AggregateException ex)
            {
                int cont = 0;
                foreach (Exception exception in ex.InnerExceptions)
                {
                    Util.LogErro($"[FilterMessages] - Erro{cont++}: {exception}");
                }

                // Indica que o tratamento da mensagem NÃO foi concluído.
                var moduleClient = (ModuleClient)userContext;
                return new MethodResponse(new byte[0], 500);
            }
            catch (Exception ex)
            {
                Util.LogErro($"[FilterMessages] - Erro: {ex}");

                // Indica que o tratamento da mensagem NÃO foi concluído.
                ModuleClient moduleClient = (ModuleClient)userContext;
                return new MethodResponse(new byte[0], 500);
            }
        }


        public static async Task<MethodResponse> EnviarMensagemMedidorDeNivel(string value, object userContext)
        {
            Util.Log($"[EnviarMensagemMedidoDeNivel] - Info: Inicio - {DateTime.Now}");
            var retorno = await EnviarMensagemMedidorDeNivel(MedidoDeNivel.PortWrite, value, userContext);
            Util.Log($"[EnviarMensagemMedidoDeNivel] - Info: Fim - {DateTime.Now}");
            return retorno;
        }

        public static async Task<MethodResponse> EnviarMensagemMedidorDeNivel(string porta, string value, object userContext)
        {
            Util.Log($"[EnviarMensagemMedidoDeNivel] - Info: Inicio - {DateTime.Now}");
            var retorno = await EnviarMensagemModbus(MedidoDeNivel.HwId, MedidoDeNivel.UId, porta, value, userContext);
            Util.Log($"[EnviarMensagemMedidoDeNivel] - Info: Fim - {DateTime.Now}");
            return retorno;
        }

        #endregion
    }
}