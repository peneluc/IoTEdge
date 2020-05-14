namespace IotEdgeModuloCentral
{
    using IotEdgeModuloCentral.Tipos;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Shared; // For TwinCollection
    using Newtonsoft.Json;                // For JsonConvert
    using System;
    using System.Runtime.Loader;
    using System.Threading;
    using System.Threading.Tasks;

    partial class Program
    {

        #region Variaveis


        #endregion


        #region Eventos

        static void Main(string[] args)
        {
            Util.Log.Info($"[Main] Inicio - {DateTime.Now}");

            Util.Log.Info($"[Update] 20200407-1930");

            Init().Wait();

            // Aguarda até o aplicativo descarregar ou ser cancelado
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();

            Util.Log.Info($"[Main] Fim - {DateTime.Now}");
        }


        /// <summary>
        /// Inicializa o ModuleClient e configura o retorno de chamada para receber
        /// mensagens contendo informações de temperatura
        /// </summary>
        static async Task Init()
        {
            Util.Log.Info($"[Init] v.1.15 em 20200423-0130");
            Util.Log.Info($"[Init] Inicio - {DateTime.Now}");

            try
            {
                AmqpTransportSettings amqpSetting = new AmqpTransportSettings(TransportType.Amqp_Tcp_Only);
                ITransportSettings[] settings = { amqpSetting };

                Util.Log.Info("[Init] Inicializando Cliente do modulo IoT Hub ***");

                // Abra uma conexão com o tempo de execução do Edge
                ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
                await ioTHubModuleClient.OpenAsync();

                Util.Log.Info("[Init] Cliente do modulo IoT Hub inicializado ***");

                // Obtem os valores das *propriedades desejadas* do módulo gêmeo
                var moduleTwin = await ioTHubModuleClient.GetTwinAsync();

                // Atribui *propriedades desejadas* ao método de tratamento
                await OnDesiredPropertiesUpdate(moduleTwin.Properties.Desired, ioTHubModuleClient);

                // Anexa método para tratar as *propriedades desejadas* do módulo gêmeo sempre que tiver atualizações.
                await ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, null);

                // Registra um método responsável por tratar as mensagens recebidas pelo módulo (filtrar e encaminhar).
                await ioTHubModuleClient.SetInputMessageHandlerAsync(Util.Message.CONST_MODCENTRAL_INPUT_MSG_FROM_MODBUS, FilterMessages, ioTHubModuleClient);

                // Registra um método responsável por tratar as mensagens recebidas pelo módulo (filtrar e encaminhar).
                await ioTHubModuleClient
                    .SetMethodHandlerAsync("LigarMedidorDeNivel", Util.Message.LigarMedidorDeNivel, ioTHubModuleClient)
                    .ConfigureAwait(false);
                await ioTHubModuleClient
                    .SetMethodHandlerAsync("DesligarMedidorDeNivel", Util.Message.DesligarMedidorDeNivel, ioTHubModuleClient)
                    .ConfigureAwait(false);
                await ioTHubModuleClient
                    .SetMethodHandlerAsync("PiscarLED", Util.Message.PiscarLED, ioTHubModuleClient)
                    .ConfigureAwait(false);

                //Console.WriteLine("Waiting 30 seconds for IoT Hub method calls ...");
                //await Task.Delay(TimeSpan.FromSeconds(30)).ConfigureAwait(false);

                Util.Log.Info($"[Init] Fim - {DateTime.Now}");
            }
            catch (AggregateException ex)
            {
                int cont = 0;
                foreach (Exception exception in ex.InnerExceptions)
                {
                    Util.Log.Error($"Init: Error{cont++}: {exception}");
                }
            }
            catch (Exception ex)
            {
                Util.Log.Error($"Init: Error: {ex.Message}");
            }

            Util.Log.Info($"[Init] Fim - {DateTime.Now}");
        }


        /// <summary>
        /// Lida com operações de limpeza quando o aplicativo é cancelado ou descarregado
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            Util.Log.Info($"[WhenCancelled] Inicio - {DateTime.Now}");

            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);

            Util.Log.Info($"[WhenCancelled] Fim - {DateTime.Now}");

            return tcs.Task;
        }


        /// Esse método é chamado sempre que o módulo recebe uma mensagem do hub IoT Edge.
        /// Ele filtra as mensagens que relatam temperaturas abaixo do limite de temperatura 
        /// definido pelo módulo duplo.Ele também adiciona a propriedade MessageType à mensagem 
        /// com o valor definido como Alerta.
        static async Task<MessageResponse> FilterMessages(Message message, object userContext)
        {
            Util.Log.Info($"[FilterMessages] Inicio - {DateTime.Now}");

            Util.Log.Info($"[FilterMessages] 1 - Esse metodo eh chamado sempre que o modulo recebe uma mensagem do hub IoT Edge");
            try
            {
                Util.Log.Info($"[FilterMessages] 2 - Inicializa instancia do ModuleClient");
                if (userContext != null)
                {
                    ModuleClient moduleClient = (ModuleClient)userContext;
                    if (moduleClient == null)
                    {
                        throw new InvalidOperationException("[FilterMessages] - Erro: 2.1 - Modulo cliente não instanciado");
                    }
                    Util.Log.Info($"[FilterMessages] 2.1 - Modulo cliente instanciado");

                    Util.Log.Info($"[FilterMessages] 3 - Captura corpo da mensagem [original] em formato de bytes array");
                    if (message != null)
                    {

                        Util.Log.Info($"[FilterMessages] 4 - Converte corpo da mensagem em string");
                        MessageBodyModbusOutput messageBodyModbus = Util.Message.ObterMessageBodyModbusOutput(message);

                        Util.Log.Info($"[FilterMessages] 5 - Transforma objeto de msg do Modbus em objeto de msg para o IoT Central");

                        MessageBodyIoTCentral messageBodyIoTCentral = Util.Message.ObterMessageBodyIoTCentral(messageBodyModbus);

                        //adiciona evento
                        Util.Log.Info($"[FilterMessages] 6 - Cria Evento de Alerta para o hub IoT");
                        await Util.Message.EnviarMensagemIoTCentral(messageBodyIoTCentral, message, moduleClient);

                        // Indica que o tratamento da mensagem FOI concluído.
                        Util.Log.Info($"[FilterMessages] Fim - {DateTime.Now} OK");
                        return MessageResponse.Completed;

                    }
                }
                return MessageResponse.Abandoned;
            }
            catch (AggregateException ex)
            {
                int cont = 0;
                foreach (Exception exception in ex.InnerExceptions)
                {
                    Util.Log.Error($"[FilterMessages] - Erro{cont++}: {exception}");
                }

                // Indica que o tratamento da mensagem NÃO foi concluído.
                var moduleClient = (ModuleClient)userContext;
                return MessageResponse.Abandoned;
            }
            catch (Exception ex)
            {
                Util.Log.Error($"[FilterMessages] - Erro: {ex}");

                // Indica que o tratamento da mensagem NÃO foi concluído.
                ModuleClient moduleClient = (ModuleClient)userContext;
                return MessageResponse.Abandoned;
            }
        }

        /// Este método recebe atualizações das propriedades desejadas do módulo twin e 
        /// atualiza a variável temperatureThreshold para corresponder.Todos os módulos 
        /// possuem seu próprio módulo duplo, o que permite configurar o código que está 
        /// sendo executado dentro de um módulo diretamente da nuvem.
        static Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {
            Util.Log.Info($"[OnDesiredPropertiesUpdate] Inicio - {DateTime.Now}");

            Util.Log.Info($"[Este método recebe atualizações das propriedades desejadas do módulo twin e atualiza a variável temperatureThreshold para corresponder]");
            try
            {
                Util.Log.Info("Desired property change:");
                Util.Log.Info(JsonConvert.SerializeObject(desiredProperties));

                if (desiredProperties[Util.Message.CONST_DEVICE_NAME_WRITE_PORT] != null)
                {
                    Util.Message.MedidoDeNivel.PortWrite = desiredProperties[Util.Message.CONST_DEVICE_NAME_WRITE_PORT];
                }
            }
            catch (AggregateException ex)
            {
                int cont = 0;
                foreach (Exception exception in ex.InnerExceptions)
                {
                    Util.Log.Error($"OnDesiredPropertiesUpdate: Error{cont++}: {exception}");
                }
            }
            catch (Exception ex)
            {
                Util.Log.Error($"OnDesiredPropertiesUpdate: Error: {ex.Message}");
            }

            Util.Log.Info($"[OnDesiredPropertiesUpdate] Fim - {DateTime.Now} OK");
            return Task.CompletedTask;
        }

        #endregion

    }
}
