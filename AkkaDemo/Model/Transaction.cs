using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaDemo.Model
{
    public class Transaction
    {
        public Transaction(Guid id, decimal amount, string description)
        {
            Id = id;
            Amount = amount;
            Description = description;
        }

        public Transaction(decimal amount, string description) : this(Guid.NewGuid(), amount, description)
        {
            
        }

        public decimal Amount { get; }
        public Guid Id { get; }
        public string Description { get; }
    }
}
