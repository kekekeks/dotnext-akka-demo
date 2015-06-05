using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaDemo.Actors.OnDemand;

namespace AkkaDemo.Actors
{
    public class AccountCommander
    {
        private readonly ActorSelection _actor;

        public AccountCommander(ActorSystem sys)
        {
            _actor = sys.ActorSelection("/user/accounts");
        }

        public Task<T> Ask<T>(Guid id, RequestBase<T> command)
        {
            return _actor.Ask<T>(new OnDemandActorEnvelope<Guid>(id, command));
        }
    }
}
