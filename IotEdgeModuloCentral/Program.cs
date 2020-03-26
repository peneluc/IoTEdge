namespace IotEdgeModuloCentral
{
    using IotEdgeModuloCentral.Tipos;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Shared; // For TwinCollection
    using Newtonsoft.Json;                // For JsonConvert
    using System;
    using System.Collections.Generic;
    using System.Runtime.Loader;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    class Program
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
        /// Propriedade com nome do m�dulo de *entrada* das mensagens
        /// </summary>
        private static readonly string _inputName = "input1";

        /// <summary>
        /// Propriedade com nome do m�dulo de *sa�da* das mensagens
        /// </summary>
        private static readonly string _outputName = "output1";

        #endregion

        static void Main(string[] args)
        {
            Init().Wait();

            // Aguarda at� o aplicativo descarregar ou ser cancelado
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Lida com opera��es de limpeza quando o aplicativo � cancelado ou descarregado
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Inicializa o ModuleClient e configura o retorno de chamada para receber
        /// mensagens contendo informa��es de temperatura
        /// </summary>
        static async Task Init()
        {
            AmqpTransportSettings amqpSetting = new AmqpTransportSettings(TransportType.Amqp_Tcp_Only);
            ITransportSettings[] settings = { amqpSetting };

            // Abra uma conex�o com o tempo de execu��o do Edge
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("*** Cliente do m�dulo IoT Hub inicializado ***");

            // Obtem os valores das *propriedades desejadas* do m�dulo g�meo
            var moduleTwin = await ioTHubModuleClient.GetTwinAsync();

            // Atribui *propriedades desejadas* ao m�todo de tratamento
            await OnDesiredPropertiesUpdate(moduleTwin.Properties.Desired, ioTHubModuleClient);

            // Anexa m�todo para tratar as *propriedades desejadas* do m�dulo g�meo sempre que tiver atualiza��es.
            //await ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, null);

            // Registra um m�todo respons�vel por tratar as mensagens recebidas pelo m�dulo (filtrar e encaminhar).
            await ioTHubModuleClient.SetInputMessageHandlerAsync(_inputName, FilterMessages, ioTHubModuleClient);
        }

        /// Este m�todo recebe atualiza��es das propriedades desejadas do m�dulo twin e 
        /// atualiza a vari�vel temperatureThreshold para corresponder.Todos os m�dulos 
        /// possuem seu pr�prio m�dulo duplo, o que permite configurar o c�digo que est� 
        /// sendo executado dentro de um m�dulo diretamente da nuvem.
        static Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {
            Console.WriteLine($"[OnDesiredPropertiesUpdate]");
            Console.WriteLine($"[Este m�todo recebe atualiza��es das propriedades desejadas do m�dulo twin e atualiza a vari�vel temperatureThreshold para corresponder]");
            try
            {
                Console.WriteLine("Desired property change:");
                //Console.WriteLine(JsonConvert.SerializeObject(desiredProperties));

                //if (desiredProperties["TemperatureThreshold"] != null)
                //    _temperaturaLimite = desiredProperties["TemperatureThreshold"];

            }
            catch (AggregateException ex)
            {
                int cont = 0;
                foreach (Exception exception in ex.InnerExceptions)
                {
                    Console.WriteLine();
                    Console.WriteLine("OnDesiredPropertiesUpdate :: Error{0}: {1}", cont++, exception);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("OnDesiredPropertiesUpdate :: Error: {0}", ex.Message);
            }
            return Task.CompletedTask;
        }

        /// Esse m�todo � chamado sempre que o m�dulo recebe uma mensagem do hub IoT Edge.
        /// Ele filtra as mensagens que relatam temperaturas abaixo do limite de temperatura 
        /// definido pelo m�dulo duplo.Ele tamb�m adiciona a propriedade MessageType � mensagem 
        /// com o valor definido como Alerta.
        static async Task<MessageResponse> FilterMessages(Message message, object userContext)
        {
            Console.WriteLine($"[FilterMessages] - Inicio");
            Console.WriteLine($"1 - Esse metodo eh chamado sempre que o modulo recebe uma mensagem do hub IoT Edge");
            try
            {
                Console.WriteLine($"2 - Inicializa instancia do ModuleClient");
                ModuleClient moduleClient = (ModuleClient)userContext;
                if (moduleClient == null)
                {
                    throw new InvalidOperationException("M�dulo cliente n�o instanciado");
                }

                Console.WriteLine($"3 - Captura corpo da mensagem em formato de bytes array");
                var messageBytes = message.GetBytes();
                if (messageBytes == null)
                {
                    throw new InvalidOperationException("messageBytes igual a null!");
                }

                Console.WriteLine($"4 - Converte corpo da mensagem em string");
                var messageString = Encoding.UTF8.GetString(messageBytes);

                //incrementa contador
                var totalMensagensRecebidas = Interlocked.Increment(ref _totalMensagensRecebidas);
                Console.WriteLine($"*** Mensagem recebida! Total: {totalMensagensRecebidas}, Corpo da Mensagem: {messageString} ***");

                MessageBody messageBody = null;
                try
                {
                    Console.WriteLine($"5 - Esse metodo eh chamado sempre que o modulo recebe uma mensagem do hub IoT Edge");
                    messageBody = JsonConvert.DeserializeObject<MessageBody>(messageString);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro na captura corpo da mensagem: {ex.Message}");
                    if (string.IsNullOrEmpty(messageString))
                    {
                        throw new InvalidOperationException($"Falha ao deserializar messageString: {messageString}");
                    }
                    else
                    {
                        Console.WriteLine($"messageString: {messageString}");
                    }
                }


                //adiciona evento
                Console.WriteLine($"6 - Cria Evento de Alerta para o hub IoT");
                using (var filteredMessage = new Message(messageBytes))
                {
                    foreach (KeyValuePair<string, string> prop in message.Properties)
                    {
                        filteredMessage.Properties.Add(prop.Key, prop.Value);
                    }

                    Console.WriteLine($"7 - Envia evento de alerta");
                    filteredMessage.Properties.Add("MessageType", "Alert");
                    await moduleClient.SendEventAsync(_outputName, filteredMessage);

                    var totalMensagensEncaminhadas = Interlocked.Increment(ref _totalMensagensEncaminhadas);
                    Console.WriteLine($"***  Mensagem recebida e encaminhada para o Iot Hub! ***");
                    Console.WriteLine($"***  Total: {totalMensagensEncaminhadas}, Corpo da Mensagem: {_outputName} ***");
                }

                // Indica que o tratamento da mensagem FOI conclu�do.
                Console.WriteLine($"[FilterMessages] - Fim OK");
                return MessageResponse.Completed;
            }
            catch (AggregateException ex)
            {
                int cont = 0;
                foreach (Exception exception in ex.InnerExceptions)
                {
                    Console.WriteLine();
                    Console.WriteLine("[FilterMessages] - Erro{0}: {1}", cont++, exception);
                }

                // Indica que o tratamento da mensagem N�O foi conclu�do.
                var moduleClient = (ModuleClient)userContext;
                return MessageResponse.Abandoned;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("[FilterMessages] - Erro: {0}", ex.Message);

                // Indica que o tratamento da mensagem N�O foi conclu�do.
                ModuleClient moduleClient = (ModuleClient)userContext;
                return MessageResponse.Abandoned;
            }
        }

        private void enviarMensagemCLP(string device, Boolean ativar)
        {
            Console.WriteLine("Info: Construct message");

            //var modbusMessageBody = new ModbusMessage
            //{
            //    HwId = HwId,
            //    UId = UId,
            //    Address = Address,
            //    Value = ativar,
            //};

            //var jsonMessage = JsonConvert.SerializeObject(modbusMessageBody);
            //var bytes = System.Text.Encoding.UTF8.GetBytes(jsonMessage);
            //Console.WriteLine("Info: Byte array SwitchTwo");
            //var pipeMessage = new Message(bytes);
            //pipeMessage.Properties.Add("command-type", "ModbusWrite");

            //await deviceClient.SendEventAsync("output1", pipeMessage);

            //Console.WriteLine("Info: Piped out message SwitchTwo");

            //if (reportedProperties.Count > 0)
            //{
            //    await deviceClient.UpdateReportedPropertiesAsync(reportedProperties).ConfigureAwait(false);
            //}
        }

    }
}
