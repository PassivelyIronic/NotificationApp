using MassTransit;
using MongoDB.Driver;
using NotificationApp.Consumers;
using NotificationApp.Models;
using NotificationApp.Repositories;
using NotificationApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

var mongoConnectionString = builder.Configuration["MongoDb:ConnectionString"];
var mongoDatabaseName = builder.Configuration["MongoDb:DatabaseName"];

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    return new MongoClient(mongoConnectionString);
});

builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDatabaseName);
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<NotificationConsumer>();
    x.AddConsumer<EmailNotificationConsumer>();
    x.AddConsumer<PushNotificationConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Konfiguracja exchange
        cfg.Message<Notification>(x => x.SetEntityName("notifications"));
        cfg.Send<Notification>(x => x.UseRoutingKeyFormatter(context => context.Message.Status.ToString()));

        // Konfiguracja g³ównej kolejki (notifications)
        cfg.ReceiveEndpoint("notifications", e =>
        {
            e.ConfigureConsumer<NotificationConsumer>(context);
        });

        // Konfiguracja kolejki email
        cfg.ReceiveEndpoint("notifications-email", e =>
        {
            e.ConfigureConsumer<EmailNotificationConsumer>(context);
        });

        // Konfiguracja kolejki push
        cfg.ReceiveEndpoint("notifications-push", e =>
        {
            e.ConfigureConsumer<PushNotificationConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
