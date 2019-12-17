using Len.Commands;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Len.Transfer
{
    public class CommandSender : ISendCommandsAndWaitForAResponse
    {
        private readonly IClientFactory _clientFactory;
        private readonly TimeSpan _timeOut = TimeSpan.FromDays(5);
        public CommandSender(IClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<CommandResponse> SendAsync<TCommand>(object command, CancellationToken cancellationToken = default)
             where TCommand : class, ICommand
        {
            try
            {
                var client = _clientFactory.CreateRequestClient<TCommand>(_timeOut);
                var request = client.Create(command, cancellationToken);
                var response = await request.GetResponse<CommandResponse>();

                return response.Message;
            }
            catch (RequestTimeoutException ex)
            {
                throw new TimeoutException(
                   string.Format(
                       "The request timed out after {0} seconds.", _timeOut.Seconds), ex);
            }
        }
    }
}
