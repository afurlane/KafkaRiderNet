// See https://aka.ms/new-console-template for more information
// https://masstransit-project.com/usage/riders/kafka.html
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using TestKafkaRider;

namespace KafkaTest
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddMassTransit(x =>
            {
                x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));

                x.AddRider(rider =>
                {
                    rider.AddConsumer<MessageConsumer>();

                    rider.AddProducer<Message>("topic-name");

                    rider.UsingKafka((context, k) =>
                    {
                        k.Host("localhost:9092");

                        k.TopicEndpoint<Message>("topic-name", "consumer-group-name", e =>
                        {
                            e.CreateIfMissing(t =>
                            {
                                t.NumPartitions = 2; //number of partitions
                                t.ReplicationFactor = 1; //number of replicas
                            });
                            e.ConfigureConsumer<MessageConsumer>(context);
                        });
                    });
                });
            });
            Producer testProducer = new Producer();
            await testProducer.StartProducer(services); 
        }
    }
}