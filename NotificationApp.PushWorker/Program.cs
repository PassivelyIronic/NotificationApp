// NotificationApp.PushWorker/Program.cs
using MassTransit;
using MongoDB.Driver;
using NotificationApp.PushWorker.Consumers;
using NotificationApp.Shared.Repositories;

var builder = Host.CreateApplicationBuilder(args);

// MongoDB Configuration
var mongoSettings = builder.Configuration.GetSection("MongoDB");
var mongoClient = new MongoClient(mongoSettings["ConnectionString"]);
var mongoDatabase = mongoClient.GetDatabase(mongoSettings["DatabaseName"]);
builder.Services.AddSingleton<IMongoDatabase>(mongoDatabase);
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Add MassTransit
builder.Services.AddMassTransit(busConfig =>
{
    // Add consumers
    busConfig.AddConsumer<PushNotificationSentConsumer>();

    // Configure RabbitMQ
    busConfig.UsingRabbitMq((context, config) =>
    {
        config.Host(builder.Configuration["RabbitMQ:Host"], host =>
        {
            host.Username(builder.Configuration["RabbitMQ:Username"]);
            host.Password(builder.Configuration["RabbitMQ:Password"]);
        });

        // Configure the push notification endpoint
        config.ReceiveEndpoint("push-notification", e =>
        {
            e.ConfigureConsumer<PushNotificationSentConsumer>(context);
            e.PrefetchCount = 1;
            e.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5)));
        });
    });
});

var host = builder.Build();
host.Run();