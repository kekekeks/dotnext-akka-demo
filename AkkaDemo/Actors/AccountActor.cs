using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using Akka.Event;
using Akka.Persistence;
using AkkaDemo.Actors.OnDemand;
using AkkaDemo.Model;
using AkkaDemo.Model.Events;

namespace AkkaDemo.Actors
{
    public class AccountActor : PersistentActor
    {
        private readonly Guid _id;
        Account _state = new Account();
        public AccountActor(Guid id)
        {
            _id = id;
            PersistenceId = "account-" + id;
            Context.GetLogger()
                .Log(LogLevel.InfoLevel, "AccountActor is created on node {0} for id {1}", Context.System.SelfAddress(), id);
            Context.SetReceiveTimeout(new TimeSpan(0, 0, 15));
        }

        protected override void PostStop()
        {
            Context.GetLogger()
                .Log(LogLevel.InfoLevel, "AccountActor is stopped on node {0} for id {1}", Context.System.SelfAddress(), _id);
        }

        #region Commands
        public class WithdrawCommand : GenericRequest
        {
            public WithdrawCommand(decimal amount)
            {
                Amount = amount;
            }

            public decimal Amount { get; }    
        }

        public class DepositCommand : GenericRequest
        {
            public DepositCommand(decimal amount)
            {
                Amount = amount;
            }

            public decimal Amount { get; }
        }

        public class BlockFundsCommandResult : GenericRequestResult
        {
            public Guid? TransactionId { get; }
            public BlockFundsCommandResult(bool success, Guid? transactionId = null) : base(success)
            {
                TransactionId = transactionId;
            }
        }

        public class BlockFundsCommand : RequestBase<BlockFundsCommandResult>
        {
            public BlockFundsCommand(decimal amount)
            {
                Amount = amount;
            }

            public decimal Amount { get; }
        }

        public class CompleteTransaction : GenericRequest
        {
            public CompleteTransaction(Guid transactionId)
            {
                TransactionId = transactionId;
            }

            public Guid TransactionId { get; }
        }

        public class CancelTransaction : GenericRequest
        {
            public CancelTransaction(Guid transactionId)
            {
                TransactionId = transactionId;
            }

            public Guid TransactionId { get; }
        }

        #endregion

        #region Queries

        public class CurrentBalanceQuery : RequestBase<CurrentBalanceQueryResult>
        {
             
        }

        public class CurrentBalanceQueryResult
        {
            public CurrentBalanceQueryResult(decimal balance, decimal blockedBalance)
            {
                Balance = balance;
                BlockedBalance = blockedBalance;
            }

            public decimal Balance { get; }
            public decimal BlockedBalance { get; }
        }

        #endregion

        protected override bool ReceiveRecover(object message)
        {
            return message.Handle()
                .With<Event>(e => _state.Update(e))
                .With<SnapshotOffer>(offer =>
                {
                    var acc = offer.Snapshot as Account;
                    if (acc != null)
                        _state = acc;
                });
        }

        void UpdateAndReplySuccess(Event ev)
        {
            _state.Update(ev);
            Sender.Tell(new GenericRequestResult(true));
        }

        protected override bool ReceiveCommand(object message)
        {
            Context.GetLogger().Log(LogLevel.InfoLevel, "AccountActor is processing: {0} on node {1}", message.GetType().Name, Context.System.SelfAddress());
            return message.Handle()
                .With<WithdrawCommand>(withdraw =>
                {
                    if (_state.Balance < withdraw.Amount)
                        Sender.Tell(new GenericRequestResult(false));
                    else
                        Persist(new NewTransactionEvent(new Transaction(-withdraw.Amount, "Withdrawal")),
                            UpdateAndReplySuccess);
                })
                .With<DepositCommand>(
                    deposit =>
                        Persist(new NewTransactionEvent(new Transaction(deposit.Amount, "Deposited")),
                            UpdateAndReplySuccess))
                .With<BlockFundsCommand>(block =>
                {
                    if (_state.Balance < block.Amount)
                        Sender.Tell(new BlockFundsCommandResult(false));
                    else
                        Persist(new NewTransactionEvent(new Transaction(block.Amount, "Blocked"), true), ev =>
                        {
                            _state.Update(ev);
                            Sender.Tell(new BlockFundsCommandResult(true, ev.Transaction.Id));
                        });
                })
                .With<CompleteTransaction>(complete =>
                {
                    if (_state.PendingTransactions.All(t => t.Id != complete.TransactionId))
                        Sender.Tell(new GenericRequestResult(false));
                    else
                        Persist(new TransactionCompletedEvent(complete.TransactionId), UpdateAndReplySuccess);
                })
                .With<CancelTransaction>(cancel =>
                {
                    if (_state.PendingTransactions.All(t => t.Id != cancel.TransactionId))
                        Sender.Tell(new GenericRequestResult(false));
                    else
                        Persist(new TransactionCancelled(cancel.TransactionId), UpdateAndReplySuccess);
                })
                .With<CurrentBalanceQuery>(
                    _ => Sender.Tell(new CurrentBalanceQueryResult(_state.Balance, _state.BlockedBalance)))
                .With<ReceiveTimeout>(_ => Context.Parent.Tell(Passivate.Instance));


        }

        public override string PersistenceId { get; }
    }
}
