using Serilog;
using Serilog.Events;
using System;
using Volo.Abp;

namespace Len.Transfer.SnapshotService
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()//最小的输出单位是Debug级别的
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)//将Microsoft前缀的日志的最小输出级别改成Information
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            using var application = AbpApplicationFactory.Create<SnapshotServiceModule>(options =>
            {
                options.Configuration.CommandLineArgs = args;
                options.UseAutofac();
            });

            application.Initialize();

            Console.WriteLine("Hello World!");

            Console.Read();
        }
    }
}
