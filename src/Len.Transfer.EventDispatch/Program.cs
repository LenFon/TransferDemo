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
using System.Linq;
using System.Threading.Tasks;

namespace Len.Transfer.EventDispatch
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var baseUri = new Uri("loopback://localhost/");

            var bus1 = Bus.Factory.CreateUsingInMemory(new Uri(baseUri, "first" + '/'), cfg =>
            {
                cfg.ReceiveEndpoint(c =>
                {
                    c.Consumer<RealConsumer>();
                });
            });

            var bus2 = Bus.Factory.CreateUsingInMemory(new Uri(baseUri, "second" + '/'), cfg =>
            {
            });
            
            bus1.Start();
            bus2.Start();
            
            await bus2.Publish(new A());

            Console.Read();
        }

        static string GetVirtualHost(Uri address)
        {
            return address.AbsolutePath.Split('/').First(x => !string.IsNullOrWhiteSpace(x));

        }
    }

    class RealConsumer :
        IConsumer<A>
    {
        public Task Consume(ConsumeContext<A> context)
        {
            Console.WriteLine("111");
            return Task.CompletedTask;
        }
    }


    public class A
    {
    }
}
