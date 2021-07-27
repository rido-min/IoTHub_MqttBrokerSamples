using AzureIoTHubDevices;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_publish
{
    class Program
    {
        static string cs = "HostName=testshared02.azure-devices-int.net;DeviceId=cosa1;SharedAccessKey=WCg0jYaADOfFx1aFw69YiT2j+65Hm+GxddawGXZcHJQ=";
        
        static async Task Main(string[] args)
        {
            var mqttClient = await HubClient.CreateFromConnectionString(cs);

            var mqttMessage = new MqttApplicationMessage()
            {
                Topic = "rido/one",
                Payload = Encoding.UTF8.GetBytes("<message-payload>"),
                QualityOfServiceLevel = 0
            };

            await mqttClient.PublishAsync(mqttMessage);

            Console.WriteLine("Message Published");
            Console.ReadLine();
        }
    }
}
