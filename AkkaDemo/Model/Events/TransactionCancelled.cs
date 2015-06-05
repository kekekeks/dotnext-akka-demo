using System;

namespace AkkaDemo.Model.Events
{
    public class TransactionCancelled : Event
    {
        public TransactionCancelled(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }
}