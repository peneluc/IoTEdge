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
    public class MessageHelper
    {

        #region Variaveis

        /// <summary>
        /// Propriedade que armazena o contador de mensagens *encaminhadas*
        /// </summary>
        private int _totalMensagensEncaminhadas;

        /// <summary>
        /// Propriedade que armazena o contador de mensagens *recebidas*
        /// </summary>
        private int _totalMensagensRecebidas;

        /// <summary>
        /// Propriedade com nome do módulo de *saída* das mensagens
        /// </summary>
        private Device medidoDeNivel = null;
        public Device MedidoDeNivel { get => medidoDeNivel; set => medidoDeNivel = value; }


        public void StartObjects()
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
        public string CONST_MODCENTRAL_INPUT_MSG_FROM_MODBUS => "input1";

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
        public string CONST_DEVICE_NAME_WRITE_PORT => "BombaPortaEscrita";

        #endregion


        #region Suporte

        public int ObterValorInteiroRealOuSimulado(Values value)
        {
            Random rnd = new Random();
            int retorno = 0;

            if ((value == null) ||
                (string.IsNullOrEmpty(value.Value)))
            {
                retorno = rnd.Next(1, 100);
                Util.Log.Error($"[ObterValorInteiroRealOuSimulado] - Valor simulado: {retorno}");
            }
            else
            {
                retorno = int.Parse(value.Value);
                Util.Log.Error($"[ObterValorInteiroRealOuSimulado] - Valor real: {retorno}");
            }

            return retorno;
        }

        public bool ObterValorBooleanoRealOuSimulado(Values value)
        {
            Random rnd = new Random();
            bool retorno = false;

            if ((value == null) ||
                (string.IsNullOrEmpty(value.Value)))
            {
                retorno = rnd.Next(1, 100) < 50;
                Util.Log.Error($"[ObterValorBooleanoRealOuSimulado] - Valor simulado: {retorno}");
            }
            else
            {
                retorno = bool.Parse(value.Value);
                Util.Log.Error($"[ObterValorInteiroRealOuSimulado] - Valor real: {retorno}");
            }

            return retorno;
        }

        private void MessageSendCount(string message)
        {
            var totalMensagensEncaminhadas = Interlocked.Increment(ref _totalMensagensEncaminhadas);
            Util.Log.Info($"[MessageSendCount] - Info: ***  Mensagem encaminhada! Total: {totalMensagensEncaminhadas}, Corpo da Mensagem: {message} ***");
        }

        private void MessageReceivedCount(string message)
        {
            var totalMensagensRecebidas = Interlocked.Increment(ref _totalMensagensRecebidas);
            Util.Log.Info($"[MessageReceivedCount] - Info: ***  Mensagem encaminhada! Total: {totalMensagensRecebidas}, Corpo da Mensagem: {message} ***");
        }

        public MessageBodyModbusOutput ObterMessageBodyModbusOutput(Message message)
        {
            var messageBytes = message.GetBytes();
            if (messageBytes == null)
            {
                throw new InvalidOperationException("[ObterMessageBodyModbusOutput] - Erro: messageBytes igual a null!");
            }

            var messageString = Encoding.UTF8.GetString(messageBytes);
            if (messageString == null)
            {
                throw new InvalidOperationException("[ObterMessageBodyModbusOutput] - Erro: messageString igual a null");
            }

            MessageReceivedCount(messageString);

            MessageBodyModbusOutput messageBodyModbus = null;
            try
            {
                Util.Log.Error($"[ObterMessageBodyModbusOutput] - Info: 5 - Deserializa a mensagem em um objeto");
                messageBodyModbus = JsonConvert.DeserializeObject<MessageBodyModbusOutput>(messageString);
            }
            catch (Exception ex)
            {
                Util.Log.Error($"[ObterMessageBodyModbusOutput] - Erro: {ex}");
                if (string.IsNullOrEmpty(messageString))
                {
                    throw new InvalidOperationException($"Falha ao deserializar messageString: {messageString}");
                }
                else
                {
                    Util.Log.Error($"[ObterMessageBodyModbusOutput] - Erro: {messageString}");
                }
            }

            return messageBodyModbus;
        }

        public MessageBodyIoTCentral ObterMessageBodyIoTCentral(MessageBodyModbusOutput messageBodyModbus)
        {
            MessageBodyIoTCentral messageBodyIoTCentral = new MessageBodyIoTCentral();
            try
            {
                #region modelo
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
                #endregion

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
                                            Util.Log.Info("[ObterMessageBodyIoTCentral] - NivelSuperior");
                                            break;
                                        case "NivelInferior":
                                            messageBodyIoTCentral.NivelReservatorioInferior = ObterValorInteiroRealOuSimulado(null);
                                            Util.Log.Info("[ObterMessageBodyIoTCentral] - NivelInferior");
                                            break;
                                        case "VazaoEntrada":
                                            messageBodyIoTCentral.VazaoEntrada = ObterValorInteiroRealOuSimulado(null);
                                            Util.Log.Info("[ObterMessageBodyIoTCentral] - VazaoEntrada");
                                            break;
                                        case "VazaoSaida":
                                            messageBodyIoTCentral.VazaoSaida = ObterValorInteiroRealOuSimulado(null);
                                            Util.Log.Info("[ObterMessageBodyIoTCentral] - VazaoSaida");
                                            break;
                                        case "StatusBomba1":
                                            messageBodyIoTCentral.StatusBomba1 = ObterValorBooleanoRealOuSimulado(null);
                                            Util.Log.Info("[ObterMessageBodyIoTCentral] - StatusBomba1");
                                            break;
                                        case "StatusBomba2":
                                            messageBodyIoTCentral.StatusBomba2 = ObterValorBooleanoRealOuSimulado(null);
                                            Util.Log.Info("[ObterMessageBodyIoTCentral] - StatusBomba2");
                                            break;
                                        case "FalhaBomba1":
                                            messageBodyIoTCentral.FalhaBomba1 = ObterValorBooleanoRealOuSimulado(null);
                                            Util.Log.Info("[ObterMessageBodyIoTCentral] - FalhaBomba1");
                                            break;
                                        case "FalhaBomba2":
                                            messageBodyIoTCentral.FalhaBomba2 = ObterValorBooleanoRealOuSimulado(null);
                                            Util.Log.Info("[ObterMessageBodyIoTCentral] - FalhaBomba2");
                                            break;
                                        default:
                                            Util.Log.Info($"[ObterMessageBodyIoTCentral] Nome de dispositivo nao reconhecido: {value}");
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
                Util.Log.Error($"Erro ao transformar objetos: {ex}");
            }

            //Salva mensagem no banco
            Util.Database.AddMessage(messageBodyIoTCentral);

            return messageBodyIoTCentral;
        }


        #endregion

        #region Enviar Comandos

        public async Task<MethodResponse> LigarMedidorDeNivel(MethodRequest methodRequest, object userContext)
        {
            Util.Log.Log($"[LigarMedidoDeNivel] - Info: Inicio - {DateTime.Now}");
            var retorno = await EnviarMensagemMedidorDeNivel("1", userContext);
            Util.Log.Log($"[LigarMedidoDeNivel] - Info: Fim - {DateTime.Now}");
            return retorno;
        }

        public async Task<MethodResponse> DesligarMedidorDeNivel(MethodRequest methodRequest, object userContext)
        {
            Util.Log.Log($"[DesligarMedidoDeNivel] - Info: Inicio - {DateTime.Now}");
            var retorno = await EnviarMensagemMedidorDeNivel("0", userContext);
            Util.Log.Log($"[DesligarMedidoDeNivel] - Info: Fim - {DateTime.Now}");
            return retorno;
        }

        public async Task<MethodResponse> PiscarLED(MethodRequest methodRequest, object userContext)
        {
            Util.Log.Log($"[PiscarLED] - Inicio");
            for (int i = 0; i < 10; i++)
            {
                await LigarMedidorDeNivel(null, userContext);
                await Task.Delay(1000);
                await DesligarMedidorDeNivel(null, userContext);
            }
            Util.Log.Log($"[PiscarLED] - Fim");
            return new MethodResponse(new byte[0], 200);
        }


        #endregion

        #region Enviar Mensagens

        public async Task<string> EnviarMensagemIoTCentral(MessageBodyIoTCentral messageBodyIoTCentral, Message message, ModuleClient moduleClient)
        {
            string messageString = string.Empty;
            byte[] messageBytes = new byte[0];
            Util.Log.Log($"[EnviarMensagemIoTCentral] - Info: 1 - Captura corpo de [messageBodyIoTCentral] em formato de bytes array");
            try
            {
                string json = JsonConvert.SerializeObject(messageBodyIoTCentral);
                messageBytes = Encoding.UTF8.GetBytes(json);
            }
            catch (Exception ex)
            {
                Util.Log.Error($"Erro ao capturar byteArray: {ex}");
            }

            Util.Log.Log($"[EnviarMensagemIoTCentral] - Info: 2 - Cria evento e envia msg");
            using (var filteredMessage = new Message(messageBytes))
            {
                foreach (KeyValuePair<string, string> prop in message.Properties)
                {
                    filteredMessage.Properties.Add(prop.Key, prop.Value);
                    Util.Log.Log($"***  filteredMessage.Properties.Add({prop.Key}, {prop.Value}) ***");
                }

                Util.Log.Log($"[EnviarMensagemIoTCentral] - Info: 3 - Envia evento de alerta");
                filteredMessage.Properties.Add("MessageType", "Alert");
                await moduleClient.SendEventAsync(CONST_MODCENTRAL_OUTPUT_MSG_TO_IOTCENTRAL, filteredMessage); // <---------- envia mensagem iot central <-----------

                Util.Log.Log($"[EnviarMensagemIoTCentral] - Info: 4 - Converte corpo da mensagem em string e registra envio");
                messageString = Encoding.UTF8.GetString(messageBytes);

                MessageSendCount(messageString);
            }
            Util.Log.Log($"[EnviarMensagemIoTCentral] - Info: 5 - Captura corpo de [messageBodyIoTCentral] em formato de bytes array");

            return messageString;
        }

        public async Task<MethodResponse> EnviarMensagemModbus(string HwId, string UId, string Address, string Value, object userContext)
        {
            Util.Log.Log($"[EnviarMensagemModbus] - Info: Inicio - {DateTime.Now}");

            Util.Log.Log($"[EnviarMensagemModbus] - Info: 1 - Inicializa instancia do ModuleClient");
            try
            {
                ModuleClient moduleClient = (ModuleClient)userContext;
                if (moduleClient == null)
                {
                    throw new InvalidOperationException("Módulo cliente não instanciado");
                }

                Util.Log.Log($"[EnviarMensagemModbus] - Info: 2 - Esse metodo eh chamado sempre que enviamos uma mensagem para o Modulo ModBus");

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

                Util.Log.Log($"[EnviarMensagemModbus] - Info: 3 - Serializa o objeto em json");
                var jsonMessage = JsonConvert.SerializeObject(modbusMessageBody);

                Util.Log.Log($"[EnviarMensagemModbus] - Info: 4 - Transforma o json em array de bytes");
                var bytes = System.Text.Encoding.UTF8.GetBytes(jsonMessage);

                Util.Log.Log($"[EnviarMensagemModbus] - Info: 5 - Cria mensagem de input do ModBus e adiciona o array de bytes no corpo");
                var pipeMessage = new Message(bytes);

                Util.Log.Log($"[EnviarMensagemModbus] - Info: 6 - Adiciona propriedade de escrita a mensagem do ModBus");
                pipeMessage.Properties.Add("command-type", "ModbusWrite");

                Util.Log.Log($"[EnviarMensagemModbus] - Info: 7 - Envia a mensagem para o Modulo ModBus");
                await moduleClient.SendEventAsync(CONST_MODCENTRAL_OUTPUT_MSG_TO_MODBUS, pipeMessage);

                MessageSendCount(jsonMessage);

                Util.Log.Log($"[EnviarMensagemModbus] - Info: Fim - {DateTime.Now}");
                return
                    //new MethodResponse(new byte[0], 200);
                    new MethodResponse(Encoding.UTF8.GetBytes("{\"EnviarMensagemModbus\":\"" + (Value) + "\"}"), 200);
            }
            catch (AggregateException ex)
            {
                int cont = 0;
                foreach (Exception exception in ex.InnerExceptions)
                {
                    Util.Log.Error($"[EnviarMensagemModbus] - Erro{cont++}: {exception}");
                }

                // Indica que o tratamento da mensagem NÃO foi concluído.
                var moduleClient = (ModuleClient)userContext;
                return new MethodResponse(new byte[0], 500);
            }
            catch (Exception ex)
            {
                Util.Log.Error($"[EnviarMensagemModbus] - Erro: {ex}");

                // Indica que o tratamento da mensagem NÃO foi concluído.
                ModuleClient moduleClient = (ModuleClient)userContext;
                return new MethodResponse(new byte[0], 500);
            }
        }


        public async Task<MethodResponse> EnviarMensagemMedidorDeNivel(string value, object userContext)
        {
            Util.Log.Log($"[EnviarMensagemMedidoDeNivel] - Info: Inicio - {DateTime.Now}");
            var retorno = await EnviarMensagemMedidorDeNivel(MedidoDeNivel.PortWrite, value, userContext);
            Util.Log.Log($"[EnviarMensagemMedidoDeNivel] - Info: Fim - {DateTime.Now}");
            return retorno;
        }

        public async Task<MethodResponse> EnviarMensagemMedidorDeNivel(string porta, string value, object userContext)
        {
            Util.Log.Log($"[EnviarMensagemMedidoDeNivel] - Info: Inicio - {DateTime.Now}");
            var retorno = await EnviarMensagemModbus(MedidoDeNivel.HwId, MedidoDeNivel.UId, porta, value, userContext);
            Util.Log.Log($"[EnviarMensagemMedidoDeNivel] - Info: Fim - {DateTime.Now}");
            return retorno;
        }

        #endregion
    }
}