using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SessionAzureBusServiceExample
{
    public class MessageReader : IHostedService
    { 
    
        private readonly ServiceBusSessionProcessor processor;

        public MessageReader(IOptions<AppConfig> configuration)
        {
            var client = new ServiceBusClient(configuration.Value.ServiceBusConnectionString);

            var sessionOptions = new ServiceBusSessionProcessorOptions()
            {
                AutoCompleteMessages = true,
                MaxConcurrentSessions = 2,
                SessionIdleTimeout = TimeSpan.FromSeconds(5)
            };

            processor = client.CreateSessionProcessor(configuration.Value.QueueName, sessionOptions);

            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return processor.StartProcessingAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return processor.StopProcessingAsync();
        }

        private static Task ErrorHandler(ProcessErrorEventArgs arg)
        {
            Console.WriteLine($"Error : {arg.Exception.Message}");
            return Task.CompletedTask;
        }

        private static Task MessageHandler(ProcessSessionMessageEventArgs arg)
        {
            var msg = arg.Message.Body.ToString();
            Console.WriteLine($"Received : {msg} from session : {arg.Message.SessionId} timestamp: {DateTime.Now} ");
            Thread.Sleep(1000);
            return Task.CompletedTask;
        }
    }
}
