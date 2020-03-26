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
        private static readonly string _outputName = "output1";

        #endregion

        static void Main(string[] args)
        {
            Init().Wait();

            // Aguarda até o aplicativo descarregar ou ser cancelado
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Lida com operações de limpeza quando o aplicativo é cancelado ou descarregado
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Inicializa o ModuleClient e configura o retorno de chamada para receber
        /// mensagens contendo informações de temperatura
        /// </summary>
        static async Task Init()
        {
            AmqpTransportSettings amqpSetting = new AmqpTransportSettings(TransportType.Amqp_Tcp_Only);
            ITransportSettings[] settings = { amqpSetting };

            // Abra uma conexão com o tempo de execução do Edge
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Util.Log("*** Cliente do módulo IoT Hub inicializado ***");

            // Obtem os valores das *propriedades desejadas* do módulo gêmeo
            var moduleTwin = await ioTHubModuleClient.GetTwinAsync();

            // Atribui *propriedades desejadas* ao método de tratamento
            await OnDesiredPropertiesUpdate(moduleTwin.Properties.Desired, ioTHubModuleClient);

            // Anexa método para tratar as *propriedades desejadas* do módulo gêmeo sempre que tiver atualizações.
            //await ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, null);

            // Registra um método responsável por tratar as mensagens recebidas pelo módulo (filtrar e encaminhar).
            await ioTHubModuleClient.SetInputMessageHandlerAsync(_inputName, FilterMessages, ioTHubModuleClient);
        }

        /// Este método recebe atualizações das propriedades desejadas do módulo twin e 
        /// atualiza a variável temperatureThreshold para corresponder.Todos os módulos 
        /// possuem seu próprio módulo duplo, o que permite configurar o código que está 
        /// sendo executado dentro de um módulo diretamente da nuvem.
        static Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {
            Util.Log($"[OnDesiredPropertiesUpdate]");
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
            return Task.CompletedTask;
        }

        /// Esse método é chamado sempre que o módulo recebe uma mensagem do hub IoT Edge.
        /// Ele filtra as mensagens que relatam temperaturas abaixo do limite de temperatura 
        /// definido pelo módulo duplo.Ele também adiciona a propriedade MessageType à mensagem 
        /// com o valor definido como Alerta.
        static async Task<MessageResponse> FilterMessages(Message message, object userContext)
        {
            Util.Log($"[FilterMessages] - Info: Inicio");
            Util.Log($"[FilterMessages] - Info: 1 - Esse metodo eh chamado sempre que o modulo recebe uma mensagem do hub IoT Edge");
            try
            {
                Util.Log($"[FilterMessages] - Info: 2 - Inicializa instancia do ModuleClient");
                ModuleClient moduleClient = (ModuleClient)userContext;
                if (moduleClient == null)
                {
                    throw new InvalidOperationException("Módulo cliente não instanciado");
                }

                Util.Log($"[FilterMessages] - Info: 3 - Captura corpo da mensagem em formato de bytes array");
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

                MessageBody messageBody = null;
                try
                {
                    Util.Log($"[FilterMessages] - Info: 5 - Deserializa a mensagem em um objeto");
                    messageBody = JsonConvert.DeserializeObject<MessageBody>(messageString);
                }
                catch (Exception ex)
                {
                    Util.Log($"Erro na captura corpo da mensagem: {ex.Message}");
                    if (string.IsNullOrEmpty(messageString))
                    {
                        throw new InvalidOperationException($"Falha ao deserializar messageString: {messageString}");
                    }
                    else
                    {
                        Util.Log($"messageString: {messageString}");
                    }
                }

                //adiciona evento
                Util.Log($"[FilterMessages] - Info: 6 - Cria Evento de Alerta para o hub IoT");
                using (var filteredMessage = new Message(messageBytes))
                {
                    foreach (KeyValuePair<string, string> prop in message.Properties)
                    {
                        filteredMessage.Properties.Add(prop.Key, prop.Value);
                    }

                    Util.Log($"[FilterMessages] - Info: 7 - Envia evento de alerta");
                    filteredMessage.Properties.Add("MessageType", "Alert");
                    await moduleClient.SendEventAsync(_outputName, filteredMessage);

                    var totalMensagensEncaminhadas = Interlocked.Increment(ref _totalMensagensEncaminhadas);
                    Util.Log($"***  Mensagem recebida e encaminhada para o Iot Hub! ***");
                    Util.Log($"***  Total: {totalMensagensEncaminhadas}, Corpo da Mensagem: {_outputName} ***");
                }

                // Indica que o tratamento da mensagem FOI concluído.
                Util.Log($"[FilterMessages] - Info: Fim OK");
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
                Util.Log($"[FilterMessages] - Erro: {ex.Message}");

                // Indica que o tratamento da mensagem NÃO foi concluído.
                ModuleClient moduleClient = (ModuleClient)userContext;
                return MessageResponse.Abandoned;
            }
        }

        private async void EnviarMensagemModbus(object userContext, string HwId, string UId, string Address, string Value)
        {
            Util.Log($"[EnviarMensagemModbus] - Info: Inicio");
            Util.Log($"[EnviarMensagemModbus] - Info: 1 - Inicializa instancia do ModuleClient");
            ModuleClient moduleClient = (ModuleClient)userContext;
            if (moduleClient == null)
            {
                throw new InvalidOperationException("Módulo cliente não instanciado");
            }

            Util.Log($"[EnviarMensagemModbus] - Info: 2 - Esse metodo eh chamado sempre que enviamos uma mensagem para o Modulo ModBus");

            var modbusMessageBody = new ModbusMessage
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
            await moduleClient.SendEventAsync("output1", pipeMessage);

            Util.Log($"[EnviarMensagemModbus] - Info: Fim");
        }

    }
}
