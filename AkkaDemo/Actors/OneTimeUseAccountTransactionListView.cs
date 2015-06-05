using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Persistence;

namespace AkkaDemo.Actors
{
    //TBD
    /*
    public class OneTimeUseAccountTransactionListView : PersistentView
    {
        public class TransactionEventInfo
        {
            public Guid Id { get; }
            public Guid Amount { get; }
            public string Info { get; }
        }
        public OneTimeUseAccountTransactionListView(string persistenceId)
        {
            PersistenceId = persistenceId;
        }

        protected override bool Receive(object message)
        {
            return false;
        }

        public override string ViewId { get; } = Guid.NewGuid().ToString();
        public override string PersistenceId { get; }
    }
    */
}
