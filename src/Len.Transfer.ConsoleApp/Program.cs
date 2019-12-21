using Len.Commands;
using Len.Domain;
using Len.Domain.Persistence;
using Len.Domain.Persistence.Memento;
using Len.Domain.Persistence.Repositories;
using Len.Events.Persistence;
using Len.Transfer.AccountBoundedContext;
using Len.Transfer.AccountBoundedContext.CommandHandlers;
using Len.Transfer.AccountBoundedContext.Commands;
using Len.Transfer.Consomers;
using Len.Transfer.Saga;
using MassTransit;
using MassTransit.Saga;
using Microsoft.Extensions.DependencyInjection;
using NEventStore;
using NEventStore.Persistence;
using NEventStore.Persistence.Sql.SqlDialects;
using NEventStore.PollingClient;
using NEventStore.Serialization.Json;
using Serilog;
using Serilog.Events;
using System;
using System.Threading.Tasks;
using Volo.Abp;

namespace Len.Transfer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                 .MinimumLevel.Debug()//最小的输出单位是Debug级别的
                 .MinimumLevel.Override("Microsoft", LogEventLevel.Information)//将Microsoft前缀的日志的最小输出级别改成Information
                 .Enrich.FromLogContext()
                 .WriteTo.Console()
                 .CreateLogger();

            using var application = AbpApplicationFactory.Create<TransferConsoleAppModule>(options =>
            {
                options.Configuration.CommandLineArgs = args;
                options.UseAutofac();
            });

            application.Initialize();

            using var scope = application.ServiceProvider.CreateScope();
            var bus = scope.ServiceProvider.GetService<IBusControl>();
            await bus.StartAsync();

            var client = scope.ServiceProvider.GetService<PollingClient2>();

            client.StartFrom();

            var sender = scope.ServiceProvider.GetService<ISendCommandsAndWaitForAResponse>();

            //初始账户数据 ，金额各100
            var id1 = Guid.Parse("359ca1d5-e315-4b92-b0e9-e1832749aa88");
            var id2 = Guid.Parse("359ca1d5-e315-4b92-b0e9-e1832749aa89");

            var response1 = await sender.SendAsync<ICreateAccountCommand>(new
            {
                Id = Guid.NewGuid(),
                AccountId = id1,
                InitialAmount = 100,
            });

            var response2 = await sender.SendAsync<ICreateAccountCommand>(new
            {
                Id = Guid.NewGuid(),
                AccountId = id2,
                InitialAmount = 100,
            });

            var repository = scope.ServiceProvider.GetService<IRepository<Account>>();
            var account1 = await repository.GetByIdAsync(id1);
            var account2 = await repository.GetByIdAsync(id2);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("account1:" + account1.ToString());
            Console.WriteLine("account2:" + account2.ToString());
            Console.ResetColor();

            //发起转账
            var response3 = await sender.SendAsync<IAccountTransferCommand>(new
            {
                Id = Guid.NewGuid(),
                FromAccountId = id1,
                ToAccountId = id2,
                Amount = 50,
            });

            Console.WriteLine("Hello World!");
            await Task.Delay(10 * 1000);
            account1 = await repository.GetByIdAsync(id1);
            account2 = await repository.GetByIdAsync(id2);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("account1:" + account1.ToString());
            Console.WriteLine("account2:" + account2.ToString());
            Console.ResetColor();

            Console.Read();
        }
    }
}
