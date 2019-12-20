using Len.Commands;
using Len.Domain;
using Len.Domain.Persistence;
using Len.Domain.Repositories;
using Len.Transfer.AccountBoundedContext;
using Len.Transfer.AccountBoundedContext.CommandHandlers;
using Len.Transfer.AccountBoundedContext.Commands;
using Len.Transfer.Consomers;
using Len.Transfer.Saga;
using MassTransit;
using MassTransit.Context;
using MassTransit.Saga;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NEventStore;
using NEventStore.Persistence.Sql.SqlDialects;
using NEventStore.PollingClient;
using NEventStore.Serialization.Json;
using Serilog;
using Serilog.Events;
using System;
using System.Threading.Tasks;

namespace Len.Transfer
{
    class Program
    {
        private static readonly byte[] EncryptionKey = { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xa, 0xb, 0xc, 0xd, 0xe, 0xf };

        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                 .MinimumLevel.Debug()//最小的输出单位是Debug级别的
                 .MinimumLevel.Override("Microsoft", LogEventLevel.Information)//将Microsoft前缀的日志的最小输出级别改成Information
                 .Enrich.FromLogContext()
                 .WriteTo.Console()
                 .CreateLogger();

            var services = new ServiceCollection();

            services.AddLogging(builder => builder.AddSerilog(dispose: true));

            var connStr = @"Server=(LocalDb)\MSSQLLocalDB;Database=Transfer-EventDb;User ID=sa;Password=1;";
            services.AddSingleton<IStoreEvents>(Wireup
                .Init()
                .UseOptimisticPipelineHook()
                //.UsingInMemoryPersistence()
                .UsingSqlPersistence(System.Data.SqlClient.SqlClientFactory.Instance, connStr)
                .WithDialect(new MsSqlDialect())
                .InitializeStorageEngine()
                .UsingJsonSerialization()
                .Compress()
                .EncryptWith(EncryptionKey)
                .UsingEventUpconversion()
                .Build());
            services.AddSingleton<IAggregateFactory, AggregateFactory>();
            services.AddSingleton<IConflictDetector, ConflictDetector>();
            services.AddTransient<IRepository, Repository>();
            services.AddTransient<ISagaRepository<AccountTransferStateInstance>, InMemorySagaRepository<AccountTransferStateInstance>>();
            services.AddSingleton<ISendCommandsAndWaitForAResponse, CommandSender>();

            services.Scan(scan =>
            {
                scan.FromAssemblyOf<CreateAccountCommandHandler>()
                    .AddClasses(c => c.Where(w => w.Name.EndsWith("CommandHandler")))
                    .AddClasses(c => c.WithAttribute<CommandHandlerAttribute>())
                    .AsImplementedInterfaces()
                    .WithTransientLifetime();
            });

            services.AddSingleton<PollingClient2>(p =>
            {
                var bus = p.GetService<IBus>();
                var store = p.GetService<IStoreEvents>();

                return new PollingClient2(store.Advanced, commit =>
                {
                    // Project / Dispatch the commit etc
                    Console.WriteLine("BucketId={0};StreamId={1};CommitSequence={2}", commit.BucketId, commit.StreamId, commit.CommitSequence);
                    // Track the most recent checkpoint
                    //checkpointToken = commit.CheckpointToken;
                    foreach (var item in commit.Events)
                    {
                        bus.Publish(item.Body).ConfigureAwait(false).GetAwaiter().GetResult();
                    }

                    return PollingClient2.HandlingResult.MoveToNext;
                },
                waitInterval: 3000);
            });

            services.AddMassTransit(x =>
            {
                x.AddConsumersFromNamespaceContaining<CreateAccountCommandConsumer>();
                //x.AddConsumer<AccountTransferCommandHandler>();
                //x.AddConsumer<TransferOutAmountCommandHandler>();
                //x.AddConsumer<TransferInAmountCommandHandler>();

                x.AddSagaStateMachinesFromNamespaceContaining(typeof(AccountTransferStateMachine));

                x.AddBus(provider => Bus.Factory.CreateUsingInMemory(cfg =>
                {
                    //cfg.Host(new Uri("rabbitmq://192.168.199.16:5673"), host =>
                    //{
                    //    host.Username("guest");
                    //    host.Password("guest");
                    //});

                    cfg.ConfigureEndpoints(provider);
                }));
            });


            using var scope = services.BuildServiceProvider().CreateScope();
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

            var repository = scope.ServiceProvider.GetService<IRepository>();
            var account1 = await repository.GetByIdAsync<Account>(id1);
            var account2 = await repository.GetByIdAsync<Account>(id2);

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
            account1 = await repository.GetByIdAsync<Account>(id1);
            account2 = await repository.GetByIdAsync<Account>(id2);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("account1:" + account1.ToString());
            Console.WriteLine("account2:" + account2.ToString());
            Console.ResetColor();

            Console.Read();
        }
    }
}
