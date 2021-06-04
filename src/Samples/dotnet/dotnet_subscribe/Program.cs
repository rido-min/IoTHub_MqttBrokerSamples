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
using MQTTnet.Protocol;

namespace dotnet_subscribe
{
    class Program
    {
        static string IoTHubHostname = "broker3.azure-devices.net";
        static string deviceId = "client2";
        static string sasKey = "0AufpBHzfQi02Dgat92fTSBOFhbxe9NFyHeUZtcWqko=";

        static async Task Main(string[] args)
        {
            Console.WriteLine("IoT Hub MQTT broker publish sample");
            var mqttClient = await CreateMqttClient();

            await mqttClient.SubscribeAsync("/rido/one", MqttQualityOfServiceLevel.AtMostOnce);

            Console.WriteLine("Receiving messages. Press any key to complete");
            Console.ReadLine();
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
