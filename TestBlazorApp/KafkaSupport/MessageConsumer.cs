using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TestBlazorApp.Chat;

namespace TestKafkaRider
{
    public class MessageConsumer : IConsumer<Message>
    {
        private readonly ILogger<MessageConsumer> logger;
        private readonly IHubContext<ChatResource> hubContext;
        private readonly IConnectionHandler connectionHandler;


        public MessageConsumer(ILogger<MessageConsumer> logger, IHubContext<ChatResource> hubContext, IConnectionHandler connectionHandler)
        {
            this.logger = logger;
            this.hubContext = hubContext;
            this.connectionHandler = connectionHandler;
        }

        public Task Consume(ConsumeContext<Message> context)
        {
            logger.LogInformation(context.Message.Text);          
            if (hubContext.Clients != null && connectionHandler.GetConnectedClients() > 0)
                hubContext.Clients.All.SendAsync("ChatMessage", "ServiceBus", context.Message.Text);
            return Task.CompletedTask;
        }
    }
}
