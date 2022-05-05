using MassTransit;
using TestBlazorApp.Infrastructure;

namespace TestKafkaRider
{
    public class Producer : IHostedService, IDisposable
    {
        private readonly ILogger<Producer> logger;
        private readonly IServiceScopeFactory serviceFactory;
        private readonly IBusControl busControl;
        // private readonly ITopicProducer<Message> producer;
        // private readonly ServiceProvider provider;
        private readonly Random random;
        private readonly LimitedConcurrencyLevelTaskScheduler lcts;
        private readonly TaskFactory factory;
        private List<Task> tasks = new List<Task>();

        public Producer(IBusControl busControl, ILogger<Producer> logger, IServiceScopeFactory serviceFactory)
        {
            // this.provider = new ServiceCollection().BuildServiceProvider();
            this.busControl = busControl;
            this.logger = logger;
            this.serviceFactory = serviceFactory;
            random = new Random();
            lcts = new LimitedConcurrencyLevelTaskScheduler(2);
            factory = new TaskFactory(lcts);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Wait until the bus is started!
            while (busControl.CheckHealth().Status != BusHealthStatus.Healthy && !cancellationToken.IsCancellationRequested)
            {
                Thread.Sleep(1000);
            }
            if(!cancellationToken.IsCancellationRequested)
            {

                // ITopicProducer<Message> producer = provider.GetRequiredService<ITopicProducer<Message>>();
                ITopicProducer<Message> producer = serviceFactory.CreateScope().ServiceProvider.GetRequiredService<ITopicProducer<Message>>();
                try
                {
                    if (producer != null)
                    {
                        Task t = factory.StartNew(() =>
                        {
                            while (!cancellationToken.IsCancellationRequested)
                            {
                                logger.LogInformation("Sending a message");
                                var randomValue = random.Next(10000);
                                producer.Produce(new
                                {
                                    Text = randomValue.ToString()
                                }, cancellationToken);
                                Thread.Sleep(randomValue);
                            }
                        }, cancellationToken);
                        tasks.Add(t);
                    }

                }
                catch (Exception ex)
                {
                    logger.LogError("Auch!", ex);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return factory.StartNew(() =>
            {
                logger.LogInformation(cancellationToken.IsCancellationRequested ? "Requested cancellation" : "Cancellation not requested!");
            });
        }

        public void Dispose()
        {
            logger.LogInformation("Disposing content");
        }
    }
}
