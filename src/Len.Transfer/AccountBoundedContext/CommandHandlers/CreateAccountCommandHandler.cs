﻿using Len.Commands;
using Len.Domain.Persistence.Repositories;
using Len.Transfer.AccountBoundedContext.Commands;
using System;
using System.Threading.Tasks;

namespace Len.Transfer.AccountBoundedContext.CommandHandlers
{
    [CommandHandler]
    public class CreateAccountCommandHandler : ICommandHandler<ICreateAccountCommand>
    {
        private readonly IRepository _repository;

        public CreateAccountCommandHandler(IRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(ICreateAccountCommand command)
        {
            var account = await _repository.GetByIdAsync<Account>(command.AccountId);

            if (account.Id == Guid.Empty)
            {
                account = new Account(command.AccountId, command.InitialAmount);

                await _repository.SaveAsync(account);
            }
        }
    }
}
