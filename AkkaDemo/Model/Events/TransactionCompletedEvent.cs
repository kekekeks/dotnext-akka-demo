using System;

namespace AkkaDemo.Model.Events
{
    public class TransactionCompletedEvent : Event
    {
        public TransactionCompletedEvent(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }
}