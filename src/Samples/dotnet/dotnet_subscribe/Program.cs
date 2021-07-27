using AzureIoTHubDevices;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_subscribe
{
    class Program
    {
        static string cs = "HostName=testshared02.azure-devices-int.net;DeviceId=cosa2;SharedAccessKey=OTgyZWUyYzVjMzY5NGMwZWFiZjBjODQ1MjUwZGEwZjE=";
        static async Task Main(string[] args)
        {
            IMqttClient mqttClient = await  HubClient.CreateFromConnectionString(cs);
            await mqttClient.SubscribeAsync("rido/one", MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce);
            //mqttClient.UseApplicationMessageReceivedHandler(MessageReceived);
            Console.WriteLine("Subscribed");
            Console.ReadLine();
        }

        private static void MessageReceived(MqttApplicationMessageReceivedEventArgs arg)
        {
            Console.WriteLine(arg.ApplicationMessage.Payload);
        }
    }
}
