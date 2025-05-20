using MassTransit;
using MongoDB.Driver;
using NotificationApp.Consumers;
using NotificationApp.Events;
using NotificationApp.Models;
using NotificationApp.Repositories;
using NotificationApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var mongoSettings = builder.Configuration.GetSection("MongoDb");
var mongoClient = new MongoClient(mongoSettings["ConnectionString"]);
var mongoDatabase = mongoClient.GetDatabase(mongoSettings["DatabaseName"]);
builder.Services.AddSingleton<IMongoDatabase>(mongoDatabase);
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddMassTransit(busConfig => {
    // Register consumers
    busConfig.AddConsumer<NotificationCreatedConsumer>();
    busConfig.AddConsumer<EmailNotificationSentConsumer>();
    busConfig.AddConsumer<PushNotificationSentConsumer>();

    busConfig.UsingRabbitMq((context, config) => {
        config.Host(builder.Configuration["RabbitMQ:Host"], host => {
            host.Username(builder.Configuration["RabbitMQ:Username"]);
            host.Password(builder.Configuration["RabbitMQ:Password"]);
        });

        // Use the delayed exchange for scheduled messages
        config.UseDelayedMessageScheduler();

        // Configure consumer endpoints with retry policies and concurrency limits
        config.ReceiveEndpoint("notification-created", e => {
            e.ConfigureConsumer<NotificationCreatedConsumer>(context);
            e.PrefetchCount = 1; // Process only one message at a time
        });

        config.ReceiveEndpoint("email-notification", e => {
            e.ConfigureConsumer<EmailNotificationSentConsumer>(context);
            e.PrefetchCount = 1; // Process only one message at a time

            // Configure retry policy (up to 3 attempts with increasing delays)
            e.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5)));
        });

        config.ReceiveEndpoint("push-notification", e => {
            e.ConfigureConsumer<PushNotificationSentConsumer>(context);
            e.PrefetchCount = 1; // Process only one message at a time

            // Configure retry policy (up to 3 attempts with increasing delays)
            e.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5)));
        });
    });
});

// Properly register the scheduler
builder.Services.AddScoped<IMessageScheduler>(provider => provider.GetRequiredService<IBus>().CreateMessageScheduler());

builder.Services.AddMassTransitHostedService();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();