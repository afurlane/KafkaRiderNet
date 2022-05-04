using MassTransit;
using TestBlazorApp.Infrastructure;

namespace TestKafkaRider
{
    public class Producer : IHostedService, IDisposable
    {
        private readonly ILogger<Producer> logger;
        private readonly IBusControl busControl;
        // private readonly ITopicProducer<Message> producer;
        private readonly ServiceProvider provider;
        private readonly Random random;
        private readonly LimitedConcurrencyLevelTaskScheduler lcts;
        private List<Task> tasks = new List<Task>();
        private readonly TaskFactory factory;

        public Producer(IBusControl busControl, ILogger<Producer> logger)
        {
            this.provider = new ServiceCollection().BuildServiceProvider();
            this.busControl = busControl;
            this.logger = logger;
            random = new Random();
            lcts = new LimitedConcurrencyLevelTaskScheduler(2);
            factory = new TaskFactory(lcts);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {

            await busControl.StartAsync(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);
            ITopicProducer<Message> producer = provider.GetRequiredService<ITopicProducer<Message>>();
            try
            {
                if (producer != null)
                {
                    Task t = factory.StartNew(() => {
                        producer.Produce(new
                        {
                            Text = random.Next().ToString()
                        }, cancellationToken);
                        Thread.Sleep(1000);
                    }, cancellationToken);
                    tasks.Add(t);
                }

            }
            catch (Exception ex)
            {
                logger.LogError("Auch!", ex);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {            
            if (busControl != null)
                await busControl.StopAsync();
        }

        public void Dispose()
        {
            // Dispose
        }
    }
}
