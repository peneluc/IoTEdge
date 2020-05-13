namespace IotEdgeModuloCentral
{
    using IotEdgeModuloCentral.Helpers;
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

        #region Variaveis


        #endregion


        #region Eventos

        static void Main(string[] args)
        {
            Util.Log($"[Main] - Info: Inicio - {DateTime.Now}");

            Util.Log($"[Update] - Info: 20200407-1930");

            Init().Wait();

            // Aguarda at� o aplicativo descarregar ou ser cancelado
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();

            Util.Log($"[Main] - Info: Fim - {DateTime.Now}");
        }


        /// <summary>
        /// Inicializa o ModuleClient e configura o retorno de chamada para receber
        /// mensagens contendo informa��es de temperatura
        /// </summary>
        static async Task Init()
        {
            Util.LogFixo($"[Init] - Info: v.1.15 em 20200423-0130");
            Util.LogFixo($"[Init] - Info: Inicio - {DateTime.Now}");

            try
            {
                AmqpTransportSettings amqpSetting = new AmqpTransportSettings(TransportType.Amqp_Tcp_Only);
                ITransportSettings[] settings = { amqpSetting };

                Util.Log("[Init] - Info: Inicializando Cliente do modulo IoT Hub ***");

                // Abra uma conex�o com o tempo de execu��o do Edge
                ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
                await ioTHubModuleClient.OpenAsync();

                Util.Log("[Init] - Info: Cliente do modulo IoT Hub inicializado ***");

                // Obtem os valores das *propriedades desejadas* do m�dulo g�meo
                var moduleTwin = await ioTHubModuleClient.GetTwinAsync();

                // Atribui *propriedades desejadas* ao m�todo de tratamento
                await OnDesiredPropertiesUpdate(moduleTwin.Properties.Desired, ioTHubModuleClient);

                // Anexa m�todo para tratar as *propriedades desejadas* do m�dulo g�meo sempre que tiver atualiza��es.
                await ioTHubModuleClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, null);

                // Registra um m�todo respons�vel por tratar as mensagens recebidas pelo m�dulo (filtrar e encaminhar).
                await ioTHubModuleClient.SetInputMessageHandlerAsync(MessageHelper.CONST_MODCENTRAL_INPUT_MSG_FROM_MODBUS, FilterMessages, ioTHubModuleClient);

                // Registra um m�todo respons�vel por tratar as mensagens recebidas pelo m�dulo (filtrar e encaminhar).
                await ioTHubModuleClient
                    .SetMethodHandlerAsync("LigarMedidorDeNivel", MessageHelper.LigarMedidorDeNivel, ioTHubModuleClient)
                    .ConfigureAwait(false);
                await ioTHubModuleClient
                    .SetMethodHandlerAsync("DesligarMedidorDeNivel", MessageHelper.DesligarMedidorDeNivel, ioTHubModuleClient)
                    .ConfigureAwait(false);
                await ioTHubModuleClient
                    .SetMethodHandlerAsync("PiscarLED", MessageHelper.PiscarLED, ioTHubModuleClient)
                    .ConfigureAwait(false);

                //Console.WriteLine("Waiting 30 seconds for IoT Hub method calls ...");
                //await Task.Delay(TimeSpan.FromSeconds(30)).ConfigureAwait(false);

                Util.LogFixo($"[Init] - Info: Fim - {DateTime.Now}");
            }
            catch (AggregateException ex)
            {
                int cont = 0;
                foreach (Exception exception in ex.InnerExceptions)
                {
                    Util.LogErro($"Init :: Error{cont++}: {exception}");
                }
            }
            catch (Exception ex)
            {
                Util.LogErro($"Init :: Error: {ex.Message}");
            }

            Util.LogFixo($"[Init] - Info: Fim - {DateTime.Now}");
        }


        /// <summary>
        /// Lida com opera��es de limpeza quando o aplicativo � cancelado ou descarregado
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            Util.Log($"[WhenCancelled] - Info: Inicio - {DateTime.Now}");

            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);

            Util.Log($"[WhenCancelled] - Info: Fim - {DateTime.Now}");

            return tcs.Task;
        }


        /// Esse m�todo � chamado sempre que o m�dulo recebe uma mensagem do hub IoT Edge.
        /// Ele filtra as mensagens que relatam temperaturas abaixo do limite de temperatura 
        /// definido pelo m�dulo duplo.Ele tamb�m adiciona a propriedade MessageType � mensagem 
        /// com o valor definido como Alerta.
        static async Task<MessageResponse> FilterMessages(Message message, object userContext)
        {
            Util.Log($"[FilterMessages] - Info: Inicio - {DateTime.Now}");

            Util.Log($"[FilterMessages] - Info: 1 - Esse metodo eh chamado sempre que o modulo recebe uma mensagem do hub IoT Edge");
            try
            {
                Util.Log($"[FilterMessages] - Info: 2 - Inicializa instancia do ModuleClient");
                if (userContext != null)
                {
                    ModuleClient moduleClient = (ModuleClient)userContext;
                    if (moduleClient == null)
                    {
                        throw new InvalidOperationException("[FilterMessages] - Erro: 2.1 - Modulo cliente n�o instanciado");
                    }
                    Util.Log($"[FilterMessages] - Info: 2.1 - Modulo cliente instanciado");

                    Util.Log($"[FilterMessages] - Info: 3 - Captura corpo da mensagem [original] em formato de bytes array");
                    if (message != null)
                    {
                        var messageBytes = message.GetBytes();
                        if (messageBytes == null)
                        {
                            throw new InvalidOperationException("[FilterMessages] - Erro: 3.1 - messageBytes igual a null!");
                        }

                        Util.Log($"[FilterMessages] - Info: 4 - Converte corpo da mensagem em string");
                        var messageString = Encoding.UTF8.GetString(messageBytes);
                        if (messageString == null)
                        {
                            throw new InvalidOperationException("[FilterMessages] - Erro: 4.1 - messageString igual a null");
                        }

                        MessageBodyModbusOutput messageBodyModbus = null;
                        try
                        {
                            Util.LogErro($"[FilterMessages] - Info: 5 - Deserializa a mensagem em um objeto");
                            messageBodyModbus = JsonConvert.DeserializeObject<MessageBodyModbusOutput>(messageString);
                        }
                        catch (Exception ex)
                        {
                            Util.LogErro($"Erro na captura corpo da mensagem: {ex}");
                            if (string.IsNullOrEmpty(messageString))
                            {
                                throw new InvalidOperationException($"Falha ao deserializar messageString: {messageString}");
                            }
                            else
                            {
                                Util.LogErro($"messageString: {messageString}");
                            }
                        }

                        Util.Log($"[FilterMessages] - Info: 6 - Transforma objeto de msg do Modbus em objeto de msg para o IoT Central");

                        MessageBodyIoTCentral messageBodyIoTCentral = MessageHelper.CriandoMensagemIoTCentral(messageBodyModbus);

                        //Gravar dados no banco
                        Util.GravarDados(messageBodyIoTCentral);

                        //adiciona evento
                        Util.Log($"[FilterMessages] - Info: 7 - Cria Evento de Alerta para o hub IoT");
                        await MessageHelper.EnviarMensagemIoTCentral(messageBodyIoTCentral, message, moduleClient);

                        // Indica que o tratamento da mensagem FOI conclu�do.
                        Util.Log($"[FilterMessages] - Info: Fim - {DateTime.Now} OK");
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
                    Util.LogErro($"[FilterMessages] - Erro{cont++}: {exception}");
                }

                // Indica que o tratamento da mensagem N�O foi conclu�do.
                var moduleClient = (ModuleClient)userContext;
                return MessageResponse.Abandoned;
            }
            catch (Exception ex)
            {
                Util.LogErro($"[FilterMessages] - Erro: {ex}");

                // Indica que o tratamento da mensagem N�O foi conclu�do.
                ModuleClient moduleClient = (ModuleClient)userContext;
                return MessageResponse.Abandoned;
            }
        }

        /// Este m�todo recebe atualiza��es das propriedades desejadas do m�dulo twin e 
        /// atualiza a vari�vel temperatureThreshold para corresponder.Todos os m�dulos 
        /// possuem seu pr�prio m�dulo duplo, o que permite configurar o c�digo que est� 
        /// sendo executado dentro de um m�dulo diretamente da nuvem.
        static Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
        {
            Util.Log($"[OnDesiredPropertiesUpdate] - Info: Inicio - {DateTime.Now}");

            Util.Log($"[Este m�todo recebe atualiza��es das propriedades desejadas do m�dulo twin e atualiza a vari�vel temperatureThreshold para corresponder]");
            try
            {
                Util.Log("Desired property change:");
                Util.Log(JsonConvert.SerializeObject(desiredProperties));

                if (desiredProperties[MessageHelper.CONST_DEVICE_NAME_WRITE_PORT] != null)
                {
                    MessageHelper.MedidoDeNivel.PortWrite = desiredProperties[MessageHelper.CONST_DEVICE_NAME_WRITE_PORT];
                }
            }
            catch (AggregateException ex)
            {
                int cont = 0;
                foreach (Exception exception in ex.InnerExceptions)
                {
                    Util.LogErro($"OnDesiredPropertiesUpdate :: Error{cont++}: {exception}");
                }
            }
            catch (Exception ex)
            {
                Util.LogErro($"OnDesiredPropertiesUpdate :: Error: {ex.Message}");
            }

            Util.Log($"[OnDesiredPropertiesUpdate] - Info: Fim - {DateTime.Now} OK");
            return Task.CompletedTask;
        }

        #endregion

    }
}
