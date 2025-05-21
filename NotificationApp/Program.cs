using MassTransit;
using MongoDB.Driver;
using NotificationApp;
using NotificationApp.Consumers;
using NotificationApp.Events;
using NotificationApp.Models;
using NotificationApp.Repositories;
using NotificationApp.Services;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB Configuration
var mongoSettings = builder.Configuration.GetSection("MongoDb");
var mongoClient = new MongoClient(mongoSettings["ConnectionString"]);
var mongoDatabase = mongoClient.GetDatabase(mongoSettings["DatabaseName"]);
builder.Services.AddSingleton<IMongoDatabase>(mongoDatabase);
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Configure Quartz
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    // Create a "notification-schedule" group
    var jobKey = new JobKey("notification-scheduler");

    q.AddJob<NotificationScheduleJob>(opts => opts.WithIdentity(jobKey));

    // Create a trigger with a simple schedule
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("notification-scheduler-trigger")
        .WithSimpleSchedule(s => s
            .WithIntervalInSeconds(30)  // Check for scheduled notifications every 30 seconds
            .RepeatForever()));
});

// Add the Quartz hosted service
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// Define the scheduler endpoint URI
Uri schedulerEndpoint = new Uri("queue:scheduler");

builder.Services.AddMassTransit(busConfig => {
    // Add the message scheduler and point it to the scheduler endpoint
    busConfig.AddMessageScheduler(schedulerEndpoint);

    // Register consumers
    busConfig.AddConsumer<NotificationCreatedConsumer>();
    busConfig.AddConsumer<EmailNotificationSentConsumer>();
    busConfig.AddConsumer<PushNotificationSentConsumer>();
    busConfig.AddConsumer<ScheduledNotificationConsumer>();

    busConfig.UsingRabbitMq((context, config) => {
        config.Host(builder.Configuration["RabbitMQ:Host"], host => {
            host.Username(builder.Configuration["RabbitMQ:Username"]);
            host.Password(builder.Configuration["RabbitMQ:Password"]);
        });

        // Use the Quartz message scheduler
        config.UseMessageScheduler(schedulerEndpoint);

        // Configure endpoints
        config.ReceiveEndpoint("scheduler", e => {
            // This endpoint receives scheduling requests
            e.ConfigureConsumer<ScheduledNotificationConsumer>(context);
        });

        config.ReceiveEndpoint("notification-created", e => {
            e.ConfigureConsumer<NotificationCreatedConsumer>(context);
            e.PrefetchCount = 1;
        });

        config.ReceiveEndpoint("email-notification", e => {
            e.ConfigureConsumer<EmailNotificationSentConsumer>(context);
            e.PrefetchCount = 1;
            e.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5)));
        });

        config.ReceiveEndpoint("push-notification", e => {
            e.ConfigureConsumer<PushNotificationSentConsumer>(context);
            e.PrefetchCount = 1;
            e.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5)));
        });

        config.ConfigureEndpoints(context);
    });
});

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