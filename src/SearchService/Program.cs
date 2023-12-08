using System.Net;
using MassTransit;
using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Models;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();

builder.Services.AddHttpClient<AuctionServiceHttpClient>().AddPolicyHandler(GetPolicy());

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddMassTransit(x => {
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search",false));

   x.UsingRabbitMq((context,cfg) =>{

    cfg.ReceiveEndpoint("search-auction-created", e=> {
        e.UseMessageRetry(r => r.Interval(5,5));

        e.ConfigureConsumer<AuctionCreatedConsumer>(context);
        e.ConfigureConsumer<AuctionDeletedConsumer>(context);
        e.ConfigureConsumer<AuctionUpdatedConsumer>(context);


    });

      cfg.ConfigureEndpoints(context);
   });
});


var app = builder.Build();

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () => {
try
{
    await DBInitializer.IntitDb(app);
}
catch ( Exception e)
{
    Console.WriteLine("Mongo DB Error ",e.StackTrace);
   
}
});

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy()
=> HttpPolicyExtensions
.HandleTransientHttpError()
.OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
.WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));
