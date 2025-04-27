using System.Reflection;
using ApiBookly.Context;
using ApiBookly.Helper;
using ApiBookly.Repositories;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using ApiBookly.Helper;
using ApiBookly.Services;
using Microsoft.Extensions.Azure;
using Azure.Security.KeyVault.Secrets;

var builder = WebApplication.CreateBuilder(args);

//Para los blobs
builder.Services.AddAzureClients(factory =>
{
    factory.AddSecretClient
    (builder.Configuration.GetSection("KeyVault"));
});

SecretClient secretClient = builder.Services.BuildServiceProvider().GetService<SecretClient>();
KeyVaultSecret secretBlob = await secretClient.GetSecretAsync("StorageAccount");
var azureKeys = secretBlob.Value;
BlobServiceClient blob = new BlobServiceClient(azureKeys);
builder.Services.AddTransient<BlobServiceClient>(x => blob);

builder.Services.AddOpenApi();
KeyVaultSecret booklySecret = await secretClient.GetSecretAsync("Bookly");
string connectionString = booklySecret.Value;

//Auth usuarios
HelperActionServicesOAuth helper =
    new HelperActionServicesOAuth(builder.Configuration);
builder.Services.AddSingleton<HelperActionServicesOAuth>(helper);
builder.Services.AddAuthentication(helper.GetAuthenticateSchema())
                        .AddJwtBearer(helper.GetJwtBearerOptions());

builder.Services.AddTransient<IRepositoryLibros,RepositoryLibros>();
builder.Services.AddTransient<ServiceStorageBlobs>();
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