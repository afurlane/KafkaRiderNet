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
        public MessageConsumer(ILogger<MessageConsumer> logger, IHubContext<ChatResource> hubContext)
        {
            this.logger = logger;
            this.hubContext = hubContext;
        }

        public Task Consume(ConsumeContext<Message> context)
        {
            logger.LogInformation(context.Message.Text);
            if (hubContext.Clients != null)
                hubContext.Clients.All.SendAsync("ChatMessage", "ServiceBus", context.Message.Text);
            return Task.CompletedTask;
        }
    }
}
