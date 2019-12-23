using Len.Commands;
using Len.Transfer.AccountBoundedContext.CommandHandlers;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace Len.Transfer
{
    [DependsOn(typeof(CoreModule))]
    public class TransferModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.Scan(scan =>
            {
                scan.FromAssemblyOf<TransferInAmountCommandHandler>()
                    .AddClasses(c => c.Where(w => w.Name.EndsWith("CommandHandler")))
                    .AddClasses(c => c.WithAttribute<CommandHandlerAttribute>())
                    .AsImplementedInterfaces()
                    .WithTransientLifetime();
            });
        }
    }
}
