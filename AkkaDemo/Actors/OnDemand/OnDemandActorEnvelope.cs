using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaDemo.Actors.OnDemand
{
    public class OnDemandActorEnvelope<TId>
    {
        public OnDemandActorEnvelope(TId id, object message)
        {
            Id = id;
            Message = message;
        }

        public TId Id { get; }
        public object Message {get;}
    }
}
