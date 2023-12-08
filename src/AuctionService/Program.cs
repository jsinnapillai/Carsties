using AuctionService.Consumers;
using AuctionService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDBContext>(options => {
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddMassTransit(x => {

x.AddEntityFrameworkOutbox<AuctionDBContext>(o => {
   o.QueryDelay = TimeSpan.FromSeconds(10);

   o.UsePostgres();
   o.UseBusOutbox();
});


   x.AddConsumersFromNamespaceContaining<AuctionCreatredFaultConsumer>();
   x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction",false));


   x.UsingRabbitMq((context,cfg) =>{
      cfg.ConfigureEndpoints(context);
   });
});
var app = builder.Build();
 
app.UseHttpsRedirection();

 app.UseAuthorization();

 app.MapControllers();

 try{
    DBInitializer.InitDb(app);

 }
 catch(Exception ex)
 {
    Console.WriteLine(ex.Message);
 }

app.Run();
 