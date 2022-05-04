using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestKafkaRider
{
    public class Producer
    {
        public async Task StartProducer(IServiceCollection services) {

            var provider = services.BuildServiceProvider();

            var busControl = provider.GetRequiredService<IBusControl>();

            await busControl.StartAsync(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);
            try
            {
                var producer = provider.GetRequiredService<ITopicProducer<Message>>();
                do
                {
                    string value = await Task.Run(() =>
                    {
                        Console.WriteLine("Enter text (or quit to exit)");
                        Console.Write("> ");
                        return Console.ReadLine();
                    });

                    if ("quit".Equals(value, StringComparison.OrdinalIgnoreCase))
                        break;

                    await producer.Produce(new
                    {
                        Text = value
                    });
                }
                while (true);
            }
            finally
            {
                await busControl.StopAsync();
            }
        }
    }
}
