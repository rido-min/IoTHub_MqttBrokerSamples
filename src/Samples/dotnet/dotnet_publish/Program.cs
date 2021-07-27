using System;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;

namespace dotnet_publish
{ 
    class Program
    {
        // HostName=ridohub2.azure-devices.net;DeviceId=mapDemo;SharedAccessKey=Vyhrl80mH1z3mRd5GKtUojV3X8adq6CibaGHwuW719Q=
        static string cs = "HostName=testshared02.azure-devices-int.net;DeviceId=cosa1;SharedAccessKey=WCg0jYaADOfFx1aFw69YiT2j+65Hm+GxddawGXZcHJQ=";
        // HostName=broker3.azure-devices.net;DeviceId=client1;SharedAccessKey=ZmAcXR1qdIxScl5JZXztt638MC6i8gtIEVB98IkooZU=
        static string IoTHubHostname = "testshared02.azure-devices-int.net";
        static string deviceId = "cosa1";
        static string sasKey = "WCg0jYaADOfFx1aFw69YiT2j+65Hm+GxddawGXZcHJQ=";

        //static string IoTHubHostname = "ridohub2.azure-devices.net";
        //static string deviceId = "mapDemo";
        //static string sasKey = "Vyhrl80mH1z3mRd5GKtUojV3X8adq6CibaGHwuW719Q=";



        static async Task Main(string[] args)
        {
            Console.WriteLine("IoT Hub MQTT broker publish sample");
            var mqttClient = await CreateMqttClient();

            var mqttMessage = new MqttApplicationMessage()
            {
                Topic = "/rido/one",
                Payload = Encoding.ASCII.GetBytes("<message-payload>"),
                QualityOfServiceLevel = 0
            };

            await mqttClient.PublishAsync(mqttMessage);
            Console.WriteLine("Message published");

            await mqttClient.DisconnectAsync();
        }

        static async Task<IMqttClient> CreateMqttClient()
        {
            var mqttFactory = new MqttFactory();
            IMqttClient mqttClient = mqttFactory.CreateMqttClient();

            mqttClient.UseConnectedHandler(HandleConnected);
            mqttClient.UseDisconnectedHandler(HandleDisconnect);
            mqttClient.UseApplicationMessageReceivedHandler(HandleApplicationMessageReceivedAsync);

            MqttClientOptionsBuilderTlsParameters tlsParameters = new MqttClientOptionsBuilderTlsParameters
            {
                UseTls = true,
                IgnoreCertificateChainErrors = true,
                IgnoreCertificateRevocationErrors = true,
                AllowUntrustedCertificates = true,
                CertificateValidationHandler = (x) => { return true; },
                SslProtocol = SslProtocols.Tls12
            };

            var expiry = DateTimeOffset.UtcNow.AddDays(1);
            var expiryString = expiry.ToUnixTimeMilliseconds().ToString();

            //string username = $"av=2021-06-30-preview&" +
            //                $"h={IoTHubHostname}&" +
            //                $"did={deviceId}&" +
            //                $"am=SAS&" +
            //                $"se={expiryString}&" +
            //                $"ca=mqttbrokerE2Etests";

            var auth = new HubV2Auth(cs);

            string username = auth.GetUserName();

            var password = auth.BuildSasToken();

            var mqttClientOptionsBuilder = new MqttClientOptionsBuilder()
                .WithClientId(deviceId)
                .WithTcpServer(IoTHubHostname, 8883)
                .WithCredentials(username, password)
                .WithTls(tlsParameters)
                .WithCleanSession(true)
                .Build();

            Console.WriteLine($"Connecting to Mqtt Broker {IoTHubHostname} with device Id: {deviceId}");
            var connAck = await mqttClient.ConnectAsync(mqttClientOptionsBuilder, CancellationToken.None);
            Console.WriteLine("Connection successful");

            return mqttClient;
        }

        private static Task HandleDisconnect(MqttClientDisconnectedEventArgs eventArgs)
        {
            Console.WriteLine($"Client disconnected: {eventArgs.AuthenticateResult}");
            return Task.FromResult(0);
        }

        private static Task HandleConnected(MqttClientConnectedEventArgs eventArgs)
        {
            Console.WriteLine($"Client connected: {eventArgs.AuthenticateResult}");
            return Task.FromResult(0);
        }

        private static Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            Console.WriteLine($"Message received\nTopic: {eventArgs.ApplicationMessage.Topic}.\nContent: {Encoding.ASCII.GetString(eventArgs.ApplicationMessage.Payload)}");
            return Task.FromResult(0);
        }

        //public static byte[] BuildSasToken(string did, string sasKey, string expiryString)
        //{
        //    var algorithm = new HMACSHA256(Convert.FromBase64String(sasKey));
        //    string toSign = $"{IoTHubHostname}\n{did}\n\n\n{expiryString}\n";
        //    return algorithm.ComputeHash(Encoding.UTF8.GetBytes(toSign));
        //}
    }
}
