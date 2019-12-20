using Len.Commands;
using Len.Transfer.AccountBoundedContext.Commands;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using NEventStore;
using NEventStore.Persistence.Sql.SqlDialects;
using NEventStore.PollingClient;
using NEventStore.Serialization.Json;
using Serilog;
using Serilog.Events;
using System;
using System.Threading.Tasks;

namespace Len.Transfer.EventDispatch
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Console.Read();
        }
    }
}
