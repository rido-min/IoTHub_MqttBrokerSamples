﻿using System;
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
        // HostName=broker3.azure-devices.net;DeviceId=client1;SharedAccessKey=ZmAcXR1qdIxScl5JZXztt638MC6i8gtIEVB98IkooZU=
        static string IoTHubHostname = "broker3.azure-devices.net";
        static string deviceId = "client1";
        static string sasKey = "ZmAcXR1qdIxScl5JZXztt638MC6i8gtIEVB98IkooZU=";

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
                CertificateValidationHandler = (x) => { return true; }
            };

            string username = $"{IoTHubHostname}/{deviceId}/api-version=2019-06-30";
            var sasBuilder = new SharedAccessSignatureBuilder()
            {
                Key = sasKey,
                KeyName = string.Empty,
                Target = IoTHubHostname,
                TimeToLive = TimeSpan.FromSeconds(60)
            };
            var token = sasBuilder.ToSignature();

            var mqttClientOptionsBuilder = new MqttClientOptionsBuilder()
                .WithClientId(deviceId)
                .WithTcpServer(IoTHubHostname, 8883)
                .WithCredentials(username, token)
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
    }
}
