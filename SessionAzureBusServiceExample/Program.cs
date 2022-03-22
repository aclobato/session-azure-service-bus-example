using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SessionAzureBusServiceExample
{
    public static class Program
    {
        static async Task Main()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .BuildServiceProvider();

            var configurationInstance = serviceProvider.GetService<IConfiguration>();
            SendMessagesWithSessionId(configurationInstance);

            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<AppConfig>(configurationInstance.GetSection("AppConfig"));
                    services.AddHostedService<MessageReader>();
                });

            await builder.RunConsoleAsync();
        }

        static void SendMessagesWithSessionId(IConfiguration configurationInstance)
        {
            var client = new ServiceBusClient(configurationInstance.GetValue<string>("AppConfig:ServiceBusConnectionString"));

            ServiceBusSender sender = client.CreateSender(configurationInstance.GetValue<string>("AppConfig:QueueName"));
            SendMessage(sender, "message 1.1", "1");
            SendMessage(sender, "message 1.2", "1");
            SendMessage(sender, "message 1.3", "1");
            SendMessage(sender, "message 1.4", "1");
            SendMessage(sender, "message 1.5", "1");
            SendMessage(sender, "message 1.6", "1");

            SendMessage(sender, "message 2.1", "2");

            SendMessage(sender, "message 3.1", "3");
            SendMessage(sender, "message 3.2", "3");
            SendMessage(sender, "message 3.3", "3");
            SendMessage(sender, "message 3.4", "3");
            SendMessage(sender, "message 3.5", "3");
            SendMessage(sender, "message 3.6", "3");

            SendMessage(sender, "message 4.1", "4");
            SendMessage(sender, "message 4.2", "4");

            SendMessage(sender, "message 5.1", "5");

            SendMessage(sender, "message 2.2", "2");
            SendMessage(sender, "message 2.3", "2");
            SendMessage(sender, "message 2.4", "2");
            SendMessage(sender, "message 2.5", "2");
        }

        private static void SendMessage(ServiceBusSender queueClient, string text, string sessionId)
        {
            ServiceBusMessage message = CreateMessage(text, sessionId);
            Console.WriteLine($"Sending : {text} from session : {sessionId}");
            queueClient.SendMessageAsync(message);
            Thread.Sleep(100);
        }

        private static ServiceBusMessage CreateMessage(string text, string sessionId)
        {
            return new ServiceBusMessage(text)
            {
                SessionId = sessionId
            };
        }
    }
}
