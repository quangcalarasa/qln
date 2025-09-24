using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOITQln
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(options => {
                        options.AddServerHeader = false;
                        options.Limits.KeepAliveTimeout = TimeSpan.FromDays(1);
                    }).UseStartup<Startup>().UseIIS();
                    //webBuilder.UseKestrel((options) =>
                    //{
                    //    options.AddServerHeader = false;
                    //});
                });
    }
}
