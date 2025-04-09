using System.Text;
using System.Text.Json.Nodes;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;

// Часть определения подключаемого устройства
string addres = "192.168.100.4";

var options = new MqttClientOptionsBuilder()
.WithTcpServer(addres, 1883)
.Build();

await Task.Run(() => SubscribeToStatusZigbee(options));

var json = new JsonObject
{
    {"action","setTargetTemperature"},
    {"value",20}
};

await PublishSomeThink("i/command/manager", json, options);

await Task.Delay(60 * 1000);

async Task SubscribeToStatusZigbee(IMqttClientOptions mqttClientOptionsBuilder)
{
    MqttFactory mqttClientFactory = new();

    using var mqttClient = mqttClientFactory.CreateMqttClient();
    await mqttClient.ConnectAsync(options);

    // Бизнес логика
    await mqttClient.SubscribeAsync("i/status/zigbee");

    mqttClient.ApplicationMessageReceivedHandler = new Handler();
}

async Task PublishSomeThink(string Topic, JsonObject message, IMqttClientOptions mqttClientOptionsBuilder)
{
    MqttFactory mqttClientFactory = new();
    using var mqttClient = mqttClientFactory.CreateMqttClient();
    await mqttClient.ConnectAsync(options);

    // Console.WriteLine(message.ToString());
    await mqttClient.PublishAsync(Topic, message.ToString());
}

class Handler : IMqttApplicationMessageReceivedHandler
{
    public Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
    {
        Console.WriteLine($"{eventArgs.ApplicationMessage.Topic}: {Encoding.UTF8.GetString(eventArgs.ApplicationMessage.Payload)}");
        return Task.CompletedTask;
    }
}



// Устройство => MQTT брокер => Роутер => Client-server app
// Устройство <= MQTT брокер <= Роутер <= Client-server app