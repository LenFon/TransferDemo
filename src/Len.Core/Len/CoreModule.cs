﻿using Len.CommandHandlers;
using Len.Commands;
using Len.Domain.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace Len
{
    public class CoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            context.Services.AddTransient<ICommandHandler<ICreateSnapshot>, CreateSnapshotCommandHandler>();
        }
    }
}
