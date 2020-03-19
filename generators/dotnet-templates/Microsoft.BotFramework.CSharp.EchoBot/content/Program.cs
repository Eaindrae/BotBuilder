// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with EchoBot .NET Template version __vX.X.X__

// #if (UseCore31)
#if (Framework==\"netcoreapp3.1\")
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting
#else
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
#endif

namespace __PROJECT_NAME__
{
    // #if (UseCore31)
    #if (Framework==\"netcoreapp3.1\")
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
                    webBuilder.UseStartup<Startup>();
                })
    }
    #else
    public class Program
    {

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
    #endif
}
