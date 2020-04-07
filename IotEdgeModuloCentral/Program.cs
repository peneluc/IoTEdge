namespace IotEdgeModuloCentral
{
    using IotEdgeModuloCentral.Tipos;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Shared; // For TwinCollection
    using Newtonsoft.Json;                // For JsonConvert
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Loader;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    partial class Program
    {
        #region Variaveis Locais

        /// <summary>
        /// Propriedade que armazena o contador de mensagens *encaminhadas*
        /// </summary>
        private static int _totalMensagensEncaminhadas;

        /// <summary>
        /// Propriedade que armazena o contador de mensagens *recebidas*
        /// </summary>
        private static int _totalMensagensRecebidas;

        /// <summary>
        /// Propriedade com nome do módulo de *entrada* das mensagens
        /// </summary>
        private static readonly string _inputName = "input1";

        /// <summary>
        /// Propriedade com nome do módulo de *saída* das mensagens
        /// </summary>
        private static readonly string _encaminharParaIoTCentral = "output1";

        /// <summary>
        /// Propriedade com nome do módulo de *saída* das mensagens
        /// </summary>
        private static readonly string _encaminharParaModbus = "output2";

        #endregion

        static void Main(string[] args)
        {
            Util.Log($"[Main] - Info: Inicio - {DateTime.Now}");

            Util.Log($"[Update] - Info: 20200407-0300");

            Init().Wait();

            // Aguarda até o aplicativo descarregar ou ser cancelado
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();

            Util.Log($"[Main] - Info: Fim - {DateTime.Now}");
        }

        /// <summary>
        /// Lida com operações de limpeza quando o aplicativo é cancelado ou descarregado
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            Util.Log($"[WhenCancelled] - Info: Inicio - {DateTime.Now}");

            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);

            Util.Log($"[WhenCancelled] - Info: Fim - {DateTime.Now}");

            return tcs.Task;
        }

        /// <summary>
        /// Inicializa o ModuleClient e configura o retorno de chamada para receber
        /// mensagens contendo informações de temperatura
        /// </summary>
        static async Task Init()
        {
            Util.Log($"[Init] - Info: Inicio - {DateTime.Now}");

            AmqpTransportSettings amqpSetting = new AmqpTransportSettings(TransportType.Amqp_Tcp_Only);
            ITransportSettings[] settings = { amqpSetting };

            // Abra uma conexão com o tempo de execução do Edge
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Util.Log("*** Cliente do modulo IoT Hub inicializado ***");

            // Obtem os valores das *propriedades desejadas* do módulo gêmeo
            // var moduleTwin = await ioTHubModuleClient.GetTwinAsync();

            // Atribui *propriedades desejadas* ao método de tratamento
            // await OnDesiredPropertiesUpdate(moduleTwin.Properties.Desired, ioTHubModuleClient);

            // Anexa método para tratar as *propriedades desejadas* do módulo gêmeo sempre que tiver atualizações.
            //await ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, null);

            // Registra um método responsável por tratar as mensagens recebidas pelo módulo (filtrar e encaminhar).
            Util.Log("*** Registrando SetInputMessageHandlerAsync FilterMessages ***");
            await ioTHubModuleClient.SetInputMessageHandlerAsync(_inputName, FilterMessages, ioTHubModuleClient);

            //Util.Log("*** Registrando SetInputMessageHandlerAsync LigarMedidorDeNivel ***");
            //await ioTHubModuleClient.SetInputMessageHandlerAsync(_inputName, LigarMedidorDeNivel, ioTHubModuleClient);

            //Util.Log("*** Registrando SetInputMessageHandlerAsync DesligarMedidorDeNivel ***");
            //await ioTHubModuleClient.SetInputMessageHandlerAsync(_inputName, DesligarMedidorDeNivel, ioTHubModuleClient);

            Util.Log($"[Init] - Info: Fim - {DateTime.Now}");
        }

        /// Este método recebe atualizações das propriedades desejadas do módulo twin e 
        /// atualiza a variável temperatureThreshold para corresponder.Todos os módulos 
        /// possuem seu próprio módulo duplo, o que permite configurar o código que está 
        /// sendo executado dentro de um módulo diretamente da nuvem.
        static Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {
            Util.Log($"[OnDesiredPropertiesUpdate] - Info: Inicio - {DateTime.Now}");

            Util.Log($"[Este método recebe atualizações das propriedades desejadas do módulo twin e atualiza a variável temperatureThreshold para corresponder]");
            try
            {
                Util.Log("Desired property change:");
                //Util.Log(JsonConvert.SerializeObject(desiredProperties));

                //if (desiredProperties["TemperatureThreshold"] != null)
                //    _temperaturaLimite = desiredProperties["TemperatureThreshold"];

            }
            catch (AggregateException ex)
            {
                int cont = 0;
                foreach (Exception exception in ex.InnerExceptions)
                {
                    Util.Log($"OnDesiredPropertiesUpdate :: Error{cont++}: {exception}");
                }
            }
            catch (Exception ex)
            {
                Util.Log($"OnDesiredPropertiesUpdate :: Error: {ex.Message}");
            }

            Util.Log($"[OnDesiredPropertiesUpdate] - Info: Fim - {DateTime.Now} OK");
            return Task.CompletedTask;
        }

        /// Esse método é chamado sempre que o módulo recebe uma mensagem do hub IoT Edge.
        /// Ele filtra as mensagens que relatam temperaturas abaixo do limite de temperatura 
        /// definido pelo módulo duplo.Ele também adiciona a propriedade MessageType à mensagem 
        /// com o valor definido como Alerta.
        static async Task<MessageResponse> FilterMessages(Message message, object userContext)
        {
            Util.Log($"[FilterMessages] - Info: Inicio - {DateTime.Now}");

            MessageBodyIoTCentral messageBodyIoTCentral = new MessageBodyIoTCentral();

            Util.Log($"[FilterMessages] - Info: 1 - Esse metodo eh chamado sempre que o modulo recebe uma mensagem do hub IoT Edge");
            try
            {


                Util.Log($"[FilterMessages] - Info: 2 - Inicializa instancia do ModuleClient");
                ModuleClient moduleClient = (ModuleClient)userContext;
                if (moduleClient == null)
                {
                    throw new InvalidOperationException("Modulo cliente não instanciado");
                }


                Util.Log($"[FilterMessages] - Extra: Pisca LED (liga/desliga)");
                for (int i = 0; i < 10; i++)
                {
                    await EnviarMensagemMedidorDeNivel("1", userContext);
                    await Task.Delay(1000);
                    await EnviarMensagemMedidorDeNivel("0", userContext);
                }


                Util.Log($"[FilterMessages] - Info: 3 - Captura corpo da mensagem [original] em formato de bytes array");
                var messageBytes = message.GetBytes();
                if (messageBytes == null)
                {
                    throw new InvalidOperationException("messageBytes igual a null!");
                }


                Util.Log($"[FilterMessages] - Info: 4 - Converte corpo da mensagem em string");
                var messageString = Encoding.UTF8.GetString(messageBytes);


                //incrementa contador
                var totalMensagensRecebidas = Interlocked.Increment(ref _totalMensagensRecebidas);
                Util.Log($"*** Mensagem recebida! Total: {totalMensagensRecebidas}, Corpo da Mensagem: {messageString} ***");


                MessageBodyModbusOutput messageBodyModbus = null;
                try
                {
                    Util.Log($"[FilterMessages] - Info: 5 - Deserializa a mensagem em um objeto");
                    messageBodyModbus = JsonConvert.DeserializeObject<MessageBodyModbusOutput>(messageString);
                }
                catch (Exception ex)
                {
                    Util.Log($"Erro na captura corpo da mensagem: {ex}");
                    if (string.IsNullOrEmpty(messageString))
                    {
                        throw new InvalidOperationException($"Falha ao deserializar messageString: {messageString}");
                    }
                    else
                    {
                        Util.Log($"messageString: {messageString}");
                    }
                }


                try
                {
                    Util.Log($"[FilterMessages] - Info: 6 - Transforma objeto de msg do Modbus em objeto de msg para o IoT Central");
                    /*
                        {
                          "PublishTimestamp": "2020-04-06 00:52:03",
                          "HwId": "AAAAA555555",
                          "SourceTimestamp": "2020-04-06 00:52:03",
                          "DisplayName": "AAAAA555555",
                          "Value": false
                        }
                    */
                    if (messageBodyModbus != null)
                    {
                        //PublishTimestamp
                        messageBodyIoTCentral.PublishTimestamp = messageBodyModbus.PublishTimestamp;
                        if (messageBodyModbus.Content != null && messageBodyModbus.Content.Count > 0)
                        {
                            //HwId
                            messageBodyIoTCentral.HwId = messageBodyModbus.Content[0].HwId;
                            if (messageBodyModbus.Content[0].Data != null && messageBodyModbus.Content[0].Data.Count > 0)
                            {
                                //SourceTimestamp
                                messageBodyIoTCentral.SourceTimestamp = messageBodyModbus.Content[0].Data[0].SourceTimestamp;
                                if (messageBodyModbus.Content[0].Data[0].Values != null && messageBodyModbus.Content[0].Data[0].Values.Count > 0)
                                {
                                    //DisplayName
                                    messageBodyIoTCentral.DisplayName = messageBodyModbus.Content[0].Data[0].Values[0].DisplayName;
                                    //Value
                                    messageBodyIoTCentral.Value = int.Parse(messageBodyModbus.Content[0].Data[0].Values[0].Value);
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Util.Log($"Erro ao transformar objetos: {ex}");
                }


                Util.Log($"[FilterMessages] - Info: 7 - Captura corpo de [messageBodyIoTCentral] em formato de bytes array");
                try
                {
                    string json = JsonConvert.SerializeObject(messageBodyIoTCentral);
                    messageBytes = Encoding.UTF8.GetBytes(json);
                }
                catch (Exception ex)
                {
                    Util.Log($"Erro ao capturar byteArray: {ex}");
                }


                //adiciona evento
                Util.Log($"[FilterMessages] - Info: 8 - Cria Evento de Alerta para o hub IoT");
                using (var filteredMessage = new Message(messageBytes))
                {
                    foreach (KeyValuePair<string, string> prop in message.Properties)
                    {
                        filteredMessage.Properties.Add(prop.Key, prop.Value);
                        Util.Log($"***  filteredMessage.Properties.Add({prop.Key}, {prop.Value}) ***");
                    }

                    Util.Log($"[FilterMessages] - Info: 9 - Envia evento de alerta");
                    filteredMessage.Properties.Add("MessageType", "Alert");

                    await moduleClient.SendEventAsync(_encaminharParaIoTCentral, filteredMessage); // <---------- envia mensagem iot central <-----------

                    var totalMensagensEncaminhadas = Interlocked.Increment(ref _totalMensagensEncaminhadas);
                    Util.Log($"***  Mensagem recebida e encaminhada para o Iot Hub! ***");
                    Util.Log($"***  Total: {totalMensagensEncaminhadas}, Corpo da Mensagem: {_encaminharParaIoTCentral} ***");
                }

                // Indica que o tratamento da mensagem FOI concluído.
                Util.Log($"[FilterMessages] - Info: Fim - {DateTime.Now} OK");
                return MessageResponse.Completed;
            }
            catch (AggregateException ex)
            {
                int cont = 0;
                foreach (Exception exception in ex.InnerExceptions)
                {
                    Util.Log($"[FilterMessages] - Erro{cont++}: {exception}");
                }

                // Indica que o tratamento da mensagem NÃO foi concluído.
                var moduleClient = (ModuleClient)userContext;
                return MessageResponse.Abandoned;
            }
            catch (Exception ex)
            {
                Util.Log($"[FilterMessages] - Erro: {ex}");

                // Indica que o tratamento da mensagem NÃO foi concluído.
                ModuleClient moduleClient = (ModuleClient)userContext;
                return MessageResponse.Abandoned;
            }
        }


        static async Task<MessageResponse> LigarMedidorDeNivel(Message message, object userContext)
        {
            Util.Log($"[LigarMedidoDeNivel] - Info: Inicio - {DateTime.Now}");
            var retorno = await EnviarMensagemMedidorDeNivel("1", userContext);
            Util.Log($"[LigarMedidoDeNivel] - Info: Fim - {DateTime.Now}");
            return retorno;
        }

        static async Task<MessageResponse> DesligarMedidorDeNivel(Message message, object userContext)
        {
            Util.Log($"[DesligarMedidoDeNivel] - Info: Inicio - {DateTime.Now}");
            var retorno = await EnviarMensagemMedidorDeNivel("0", userContext);
            Util.Log($"[DesligarMedidoDeNivel] - Info: Fim - {DateTime.Now}");
            return retorno;
        }

        static async Task<MessageResponse> EnviarMensagemMedidorDeNivel(string value, object userContext)
        {
            Util.Log($"[EnviarMensagemMedidoDeNivel] - Info: Inicio - {DateTime.Now}");
            var retorno = await EnviarMensagemModbus("GTI-Device", "1", "100003", value, userContext);
            Util.Log($"[EnviarMensagemMedidoDeNivel] - Info: Fim - {DateTime.Now}");
            return retorno;
        }

        static async Task<MessageResponse> EnviarMensagemModbus(string HwId, string UId, string Address, string Value, object userContext)
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
                await moduleClient.SendEventAsync(_encaminharParaModbus, pipeMessage);

                Util.Log($"[EnviarMensagemModbus] - Info: Fim - {DateTime.Now}");
                return MessageResponse.Completed;
            }
            catch (AggregateException ex)
            {
                int cont = 0;
                foreach (Exception exception in ex.InnerExceptions)
                {
                    Util.Log($"[FilterMessages] - Erro{cont++}: {exception}");
                }

                // Indica que o tratamento da mensagem NÃO foi concluído.
                var moduleClient = (ModuleClient)userContext;
                return MessageResponse.Abandoned;
            }
            catch (Exception ex)
            {
                Util.Log($"[FilterMessages] - Erro: {ex}");

                // Indica que o tratamento da mensagem NÃO foi concluído.
                ModuleClient moduleClient = (ModuleClient)userContext;
                return MessageResponse.Abandoned;
            }
        }

    }
}
