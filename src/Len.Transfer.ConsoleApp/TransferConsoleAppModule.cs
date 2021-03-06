﻿using Len.Commands;
using Len.Transfer.AccountBoundedContext.Saga;
using Len.Transfer.Consomers;
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

namespace Len.Transfer
{
    [DependsOn(typeof(TransferModule), typeof(TransferSagaModule), typeof(NEventStoreModule))]
    public class TransferConsoleAppModule : AbpModule
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

            context.Services.AddSingleton<ISendCommandsAndWaitForAResponse, CommandSender>();

            context.Services.AddSingleton<PollingClient2>(p =>
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

                    if (commit.StreamRevision % 2 == 0)
                    {
                        var aggregateType = commit.Headers[Domain.Persistence.Repositories.Repository.AggregateType]?.ToString();

                        if (!string.IsNullOrEmpty(aggregateType))
                        {
                            bus.Publish<ICreateSnapshot>(new
                            {
                                Id = Guid.NewGuid(),
                                AggregateId = Guid.Parse(commit.StreamId),
                                AggregateTypeFullName = aggregateType,
                                Version = commit.StreamRevision,
                            }).ConfigureAwait(false).GetAwaiter().GetResult();
                        }
                    }

                    return PollingClient2.HandlingResult.MoveToNext;
                },
                waitInterval: 3000);
            });

            context.Services.AddMassTransit(x =>
            {
                x.AddConsumersFromNamespaceContaining<CreateAccountCommandConsumer>();
                x.AddSagaStateMachinesFromNamespaceContaining(typeof(AccountTransferStateMachine));
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

            var client = context.ServiceProvider.GetService<PollingClient2>();

            client.StartFrom();
        }

        public override void OnApplicationShutdown(ApplicationShutdownContext context)
        {
            var bus = context.ServiceProvider.GetService<IBusControl>();

            bus.Stop();

            var client = context.ServiceProvider.GetService<PollingClient2>();

            client.Stop();
        }
    }
}
