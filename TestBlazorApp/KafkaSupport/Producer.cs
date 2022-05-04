using MassTransit;
using TestBlazorApp.Infrastructure;

namespace TestKafkaRider
{
    public class Producer : IHostedService, IDisposable
    {
        private readonly ILogger<Producer> logger;
        private readonly ServiceProvider serviceProvider;
        private IBusControl busControl;
        private readonly Random random;
        private readonly LimitedConcurrencyLevelTaskScheduler lcts;
        private List<Task> tasks = new List<Task>();
        private readonly TaskFactory factory;

        public Producer(IServiceCollection serviceCollection, ILogger<Producer> logger)
        {
            serviceProvider = serviceCollection.BuildServiceProvider();
            busControl = serviceProvider.GetRequiredService<IBusControl>();
            this.logger = logger;
            random = new Random();
            lcts = new LimitedConcurrencyLevelTaskScheduler(2);
            factory = new TaskFactory(lcts);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {

            await busControl.StartAsync(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);
            try
            {
                var producer = serviceProvider.GetRequiredService<ITopicProducer<Message>>();
                if (producer != null)
                {
                    Task t = factory.StartNew(() => {
                            producer.Produce(new
                            {
                                Text = random.Next().ToString()
                            }, cancellationToken);
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
