using Automatonymous;
using Len.Commands;
using Len.Transfer.AccountBoundedContext.CommandHandlers;
using Len.Transfer.AccountBoundedContext.Saga;
using MassTransit;
using MassTransit.Saga;
using Microsoft.Extensions.DependencyInjection;
using System;
using Volo.Abp.Modularity;

namespace Len.Transfer
{
    [DependsOn(typeof(TransferModule))]
    public class TransferSagaModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            //context.Services.AddMassTransit(x =>
            //{
            //    x.AddSagaStateMachinesFromNamespaceContaining(typeof(AccountTransferStateMachine));
            //});

            context.Services.AddTransient<ISagaRepository<AccountTransferStateInstance>, InMemorySagaRepository<AccountTransferStateInstance>>();
        }
    }
}
