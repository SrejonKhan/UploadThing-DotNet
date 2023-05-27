using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UploadThing.Core;
using UploadThing.Example;

bool IS_DEVELOPMENT = true;

var builder = new ConfigurationBuilder();
builder.
    SetBasePath(Directory.GetCurrentDirectory()).
    AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

if(IS_DEVELOPMENT) 
{
    builder.AddUserSecrets<Program>();
}

IConfigurationRoot config = builder.Build();
IServiceCollection services = new ServiceCollection();

services.Configure<UploadThingOptions>(config.GetSection("UploadThing"));
var serviceProvider = services.
    AddTransient<Example>().
    BuildServiceProvider();

var uploadExample = serviceProvider.GetRequiredService<Example>();
Console.Read();
Console.WriteLine("Uploading file...\n");
await uploadExample.UploadFile();
Console.WriteLine("Program completed execution!");

