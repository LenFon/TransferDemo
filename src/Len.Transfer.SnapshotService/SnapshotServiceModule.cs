using Len.Commands;
using Len.Transfer;
using Len.Transfer.SnapshotService.Consomers;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using NEventStore;
using NEventStore.Persistence.Sql.SqlDialects;
using NEventStore.PollingClient;
using NEventStore.Serialization.Json;
using Serilog;
using System;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace Len.Transfer.SnapshotService
{
    [DependsOn(typeof(NEventStoreModule), typeof(TransferModule))]
    public class SnapshotServiceModule : AbpModule
    {
        private static readonly byte[] EncryptionKey = { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xa, 0xb, 0xc, 0xd, 0xe, 0xf };

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var connStr = @"Server=(LocalDb)\MSSQLLocalDB;Database=Transfer-EventDb;User ID=sa;Password=1;";
            context.Services.AddLogging(builder => builder.AddSerilog(dispose: true));

            context.Services.AddSingleton<IStoreEvents>(p => Wireup
                .Init()
                .LogToConsoleWindow()
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

            context.Services.AddMassTransit(x =>
            {
                x.AddConsumer<CreateSnapshotCommandConsumer>();

                x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.Host(new Uri("rabbitmq://192.168.199.16:5673"), host =>
                    {
                        host.Username("guest");
                        host.Password("guest");
                    });

                    cfg.ConfigureEndpoints(provider);
                }));
            });
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var bus = context.ServiceProvider.GetService<IBusControl>();

            bus.Start();
        }

        public override void OnApplicationShutdown(ApplicationShutdownContext context)
        {
            var bus = context.ServiceProvider.GetService<IBusControl>();

            bus.Stop();
        }
    }
}
