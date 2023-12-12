using AuctionService.Consumers;
using AuctionService.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

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


//  builder.Services.AddAuthentication(sharedOptions =>
//             {
//                 sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//                 sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//                 sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
//             })
//             .AddCookie()
//             .AddOpenIdConnect(options =>
//             {
//                 options.Authority = "https://kanga.cd14.net/adfs";
//                 options.ClientId = "9ec38358-8409-4f08-98b7-4bcbfb065b59"; // Replace with your client ID
//                 options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
//                 options.CallbackPath = "/signin-adfs"; // Replace with your callback path

//                 // Configure other options as needed based on your ADFS setup

//                 options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
//                 {
//                     // Configure token validation parameters if needed
//                 };
//             });


 
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options => {
   options.Authority = builder.Configuration["IdentityServiceUrl"];
   options.RequireHttpsMetadata = false;
   options.TokenValidationParameters.ValidateAudience = false;
   // options.TokenValidationParameters.NameClaimType = "username";
     options.TokenValidationParameters.NameClaimType = "Username";
});


var app = builder.Build();
 
// app.UseHttpsRedirection();
app.UseAuthentication();

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
 