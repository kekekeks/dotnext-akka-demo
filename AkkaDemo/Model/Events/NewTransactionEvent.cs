using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaDemo.Model.Events
{
    public class NewTransactionEvent : Event
    {
        public Transaction Transaction { get; }
        public bool IsPending { get; }

        public NewTransactionEvent(Transaction transaction, bool isPending = false)
        {
            Transaction = transaction;
            IsPending = isPending;
        }
    }
}
