using Microsoft.Extensions.DependencyInjection;
using NEventStore;
using NEventStore.Persistence;
using Volo.Abp.Modularity;

namespace Len
{
    [DependsOn(typeof(CoreModule))]
    public class NEventStoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddSingleton<IPersistStreams>(p => p.GetService<IStoreEvents>().Advanced);
        }
    }
}
