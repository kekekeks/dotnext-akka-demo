using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AkkaDemo.Model.Events;

namespace AkkaDemo.Model
{
    class Account
    {
        public Account()
        {
            PendingTransactions = new List<Transaction>();
        }

        public void Update(Event ev)
        {
            ev.Handle()
                .With<NewTransactionEvent>(t =>
                {
                    Balance += t.Transaction.Amount;
                    if (t.IsPending)
                    {
                        PendingTransactions.Add(t.Transaction);
                        BlockedBalance -= t.Transaction.Amount;
                    }
                })
                .With<TransactionCompletedEvent>(e =>
                {
                    var transaction = PendingTransactions.FirstOrDefault(t => t.Id == e.Id);
                    if (transaction != null)
                    {
                        PendingTransactions.Remove(transaction);
                        BlockedBalance += transaction.Amount;
                    }
                })
                .With<TransactionCompletedEvent>(e =>
                {
                    var transaction = PendingTransactions.FirstOrDefault(t => t.Id == e.Id);
                    if (transaction != null)
                    {
                        PendingTransactions.Remove(transaction);
                        Balance -= transaction.Amount;
                        BlockedBalance += transaction.Amount;
                    }
                });
        }

        public decimal Balance { get; private set; }
        public decimal BlockedBalance { get; private set; }
       
        public List<Transaction> PendingTransactions { get; }
    }
}
