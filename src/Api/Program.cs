using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace AuthorizationManagement.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, configBuilder) =>
                {
                    configBuilder.AddJsonFile("appsettings.json", false, true)
                        .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", false, true);

                    if (context.HostingEnvironment.IsLocal()) return;

                    var config = configBuilder.Build();

                    configBuilder.AddAzureKeyVault(
                        new Uri($"https://{config["KeyVaultName"]}.vault.azure.net/"), 
                        new DefaultAzureCredential());
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
