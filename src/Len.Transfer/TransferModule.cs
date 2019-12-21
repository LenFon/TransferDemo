using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Modularity;

namespace Len.Transfer
{
    [DependsOn(typeof(CoreModule))]
    public class TransferModule : AbpModule
    {
    }
}
