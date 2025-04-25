using System.Reflection;
using ApiBookly.Context;
using ApiBookly.Repositories;
using Microsoft.EntityFrameworkCore;
using SegundoExamenAzure.Helper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
string connectionString = builder.Configuration.GetConnectionString("Bookly");

//Auth usuarios
HelperActionServicesOAuth helper =
    new HelperActionServicesOAuth(builder.Configuration);
builder.Services.AddSingleton<HelperActionServicesOAuth>(helper);
builder.Services.AddAuthentication(helper.GetAuthenticateSchema())
                        .AddJwtBearer(helper.GetJwtBearerOptions());


builder.Services.AddTransient<IRepositoryLibros,RepositoryLibros>();
builder.Services.AddDbContext<StoryContext>(options =>
{
    options.UseSqlServer(connectionString);
});
builder.Services.AddControllers();
builder.Services.AddAuthorization();
var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//}
app.MapOpenApi();

app.UseHttpsRedirection();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "API Bookly");
    options.RoutePrefix = "";
});



app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

