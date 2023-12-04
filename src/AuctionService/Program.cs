using AuctionService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDBContext>(options => {
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
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
 