using TestBlazorApp.Chat;
using TestBlazorApp.Data;
using TestKafkaRider;
using MassTransit;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.UseSerilog((ctx, lc) =>
{
    lc.ReadFrom.Configuration(ctx.Configuration);
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

// Register consumer and custom connection handler to tranc connected clients outside of bus
builder.Services.AddScoped<MessageConsumer>();
builder.Services.AddSingleton<IConnectionHandler, ConnectionHandler>();

builder.Services.AddMassTransit(x =>
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
                // e.Consumer<MessageConsumer>();
            });
        });
        rider.AddConsumer<MessageConsumer>();
    });
});

builder.Services.AddOptions<MassTransitHostOptions>()
                .Configure(options =>
                {
                    // if specified, waits until the bus is started before
                    // returning from IHostedService.StartAsync
                    // default is false
                    options.WaitUntilStarted = true;

                    // if specified, limits the wait time when starting the bus
                    options.StartTimeout = TimeSpan.FromSeconds(10);

                    // if specified, limits the wait time when stopping the bus
                    options.StopTimeout = TimeSpan.FromSeconds(30);
                });

builder.Services.AddHostedService<Producer>();

var app = builder.Build();

app.UseSerilogRequestLogging(options =>
{
    // Customize the message template
    options.MessageTemplate = "Handled {RequestPath}";

    // Emit debug-level events instead of the defaults
    options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;

    // Attach additional properties to the request completion event
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
    };
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapHub<ChatResource>(ChatResource.ChatURL);
app.MapFallbackToPage("/_Host");

app.Run();
