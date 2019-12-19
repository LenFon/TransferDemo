
using Automatonymous;
using GreenPipes;
using Len.Transfer.AccountBoundedContext.Commands;
using Len.Transfer.AccountBoundedContext.Events;
using MassTransit;
using MassTransit.Definition;
using System;
using System.Threading.Tasks;

namespace Len.Transfer.Saga
{
    /// <summary>
    /// 转账状态机
    /// </summary>
    public class AccountTransferStateMachine : MassTransitStateMachine<AccountTransferStateInstance>
    {
        public AccountTransferStateMachine()
        {
            InstanceState(x => x.CurrentState);
            //InstanceState(x => x.CurrentState, Start, FromAccountTransferSuccess, End);

            Event(() => TransferStarted, x => x.CorrelateById(context => context.Message.Id));
            Event(() => TransferOutAmountCompleted, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => TransferInAmountCompleted, x => x.CorrelateById(context => context.Message.CorrelationId));

            //初始化状态机实例的状态数据，并且对转出账户执行转出金额操作，设置状态机的状态为开始状态
            Initially(
                When(TransferStarted)
                    .Then(Initialize)
                    .ThenAsync(Handle)
                    .TransitionTo(WaitingTransferOutAmount));

            //当转出账户转出金额成功后，对转入账户执行转入金额操作，设置状态机的状态为  转出账户转出金额成功
            //当转出账户转出金额失败后，设置状态机的状态为 暂停
            During(WaitingTransferOutAmount,
                When(TransferOutAmountCompleted)
                    .ThenAsync(Handle)
                    .TransitionTo(WaitingTransferInAmount));

            //当转入账户转入金额成功后，设置状态机的状态为 结束
            //当转入账户转入金额失败后，设置状态机的状态为 暂停
            During(WaitingTransferInAmount,
                When(TransferInAmountCompleted)
                    .Then(Handle)
                    .TransitionTo(Final));

            SetCompletedWhenFinalized();
        }

        public Event<TransferStarted> TransferStarted { get; private set; }

        /// <summary>
        /// 转出金额完成
        /// </summary>
        public Event<TransferOutAmountCompleted> TransferOutAmountCompleted { get; private set; }

        /// <summary>
        /// 转入金额完成
        /// </summary>
        public Event<TransferInAmountCompleted> TransferInAmountCompleted { get; private set; }

        /// <summary>
        /// 等等转出金额
        /// </summary>
        public State WaitingTransferOutAmount { get; private set; }

        /// <summary>
        /// 等等转入金额
        /// </summary>
        public State WaitingTransferInAmount { get; private set; }


        private void Handle(BehaviorContext<AccountTransferStateInstance, TransferInAmountCompleted> context)
        {
            Console.WriteLine("TransferInAmountCompleted");
        }

        private async Task Handle(BehaviorContext<AccountTransferStateInstance, TransferOutAmountCompleted> context)
        {
            Console.WriteLine("TransferOutAmountCompleted");
            var command = CreateToAccountTransferInAmountCommand(context.Instance, context.Data);

            //if (!EndpointConvention.TryGetDestinationAddress(command.GetType(), out var destinationAddress))
            //{
            //    throw new ConfigurationException($"The endpoint convention was not configured: {TypeMetadataCache<ITransferInAmount>.ShortName}");
            //}

            await context.GetPayload<ConsumeContext>().Publish<ITransferInAmount>(command);
            //await context.Publish(command);
        }

        private object CreateToAccountTransferInAmountCommand(AccountTransferStateInstance instance, TransferOutAmountCompleted data)
        {
            return new
            {
                AccountId = instance.ToAccountId,
                FromAccountId = instance.FromAccountId,
                Amount = instance.Amount,
                CorrelationId = instance.CorrelationId,
            };
        }

        private async Task Handle(BehaviorContext<AccountTransferStateInstance, TransferStarted> context)
        {
            var command = CreateFromAccountTransferOutAmountCommand(context.Instance);

            //if (!EndpointConvention.TryGetDestinationAddress(command.GetType(), out var destinationAddress))
            //{
            //    throw new ConfigurationException($"The endpoint convention was not configured: {TypeMetadataCache<ITransferOutAmount>.ShortName}");
            //}
            await context.GetPayload<ConsumeContext>().Publish<ITransferOutAmount>(command);

            //await context.Publish(command);
            // await context.GetPayload<ConsumeContext>().Send(destinationAddress, command).ConfigureAwait(false);
        }

        private object CreateFromAccountTransferOutAmountCommand(AccountTransferStateInstance instance)
        {
            return new
            {
                AccountId = instance.FromAccountId,
                ToAccountId = instance.ToAccountId,
                Amount = instance.Amount,
                CorrelationId = instance.CorrelationId,
            };
        }

        private void Initialize(BehaviorContext<AccountTransferStateInstance, TransferStarted> context)
        {
            InitializeInstance(context.Instance, context.Data);
        }

        private void InitializeInstance(AccountTransferStateInstance instance, TransferStarted data)
        {
            instance.FromAccountId = data.FromAccountId;
            instance.ToAccountId = data.ToAccountId;
            instance.Amount = data.Amount;
        }
    }

    public class AccountTransferStateMachineDefinition : SagaDefinition<AccountTransferStateInstance>
    {
        public AccountTransferStateMachineDefinition()
        {
            EndpointName = "account-transfer-state";
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<AccountTransferStateInstance> sagaConfigurator)
        {
            var partition = endpointConfigurator.CreatePartitioner(Environment.ProcessorCount);

            sagaConfigurator.Message<TransferStarted>(x => x.UsePartitioner(partition, m => m.Message.Id));
            sagaConfigurator.Message<TransferOutAmountCompleted>(x => x.UsePartitioner(partition, m => m.Message.CorrelationId));
            sagaConfigurator.Message<TransferInAmountCompleted>(x => x.UsePartitioner(partition, m => m.Message.CorrelationId));
        }
    }

    public class AccountTransferStateInstance : SagaStateMachineInstance
    {
        public Guid FromAccountId { get; set; }

        public Guid ToAccountId { get; set; }

        public decimal Amount { get; set; }

        public Guid CorrelationId { get; set; }

        public string CurrentState { get; set; }
    }
}
