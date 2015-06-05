using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace AkkaDemo.Actors.OnDemand
{
    class OnDemandActorManager<TId, TActor> : ReceiveActor where TActor : ActorBase
    {
        class ActorDescriptor
        {
            public IActorRef ActorRef;
            public TId Id;
            public bool IsTerminating;

            public ActorDescriptor(TId id, IActorRef actorRef)
            {
                Id = id;
                ActorRef = actorRef;
            }
        }

        struct QueuedMessage
        {
            public readonly object Message;
            public readonly IActorRef Sender;

            public QueuedMessage(object message)
            {
                Message = message;
                Sender = Context.Sender;
            }
        }
        private readonly Dictionary<TId, ActorDescriptor> _activeActors = new Dictionary<TId, ActorDescriptor>();
        private readonly Dictionary<TId, List<QueuedMessage>> _pendingMessages = new Dictionary<TId, List<QueuedMessage>>();
        public OnDemandActorManager()
        {
            Receive((Action<Passivate>)OnPassivate);
            Receive((Action<OnDemandActorEnvelope<TId>>) OnMessage);
            Receive((Action<Terminated>) OnChildTerminated);
        }

        private void OnChildTerminated(Terminated terminated)
        {
            Context.Unwatch(Sender);
            var desc = _activeActors.FirstOrDefault(p => p.Value.ActorRef.Equals(Sender)).Value;
            if (desc == null)
                return;
            List<QueuedMessage> pending;
            if (_pendingMessages.TryGetValue(desc.Id, out pending))
            {
                //Recreate the actor since it has some stuff to process


                desc.IsTerminating = false;
                desc.ActorRef = Create(desc.Id);

                _pendingMessages.Remove(desc.Id);
                foreach (var msg in pending)
                    desc.ActorRef.Tell(msg.Message, msg.Sender);

            }
            else
                _activeActors.Remove(desc.Id);
        }

        private IActorRef Create(TId id)
        {
            var r = Context.ActorOf(Props.Create<TActor>(id));
            Context.Watch(r);
            return r;
        }

        private void OnMessage(OnDemandActorEnvelope<TId> envelope)
        {
            ActorDescriptor descriptor;
            if (!_activeActors.TryGetValue(envelope.Id, out descriptor))
                _activeActors[envelope.Id] = descriptor = new ActorDescriptor(envelope.Id, Create(envelope.Id));
            if (!descriptor.IsTerminating)
                descriptor.ActorRef.Tell(envelope.Message, Sender);
            else
            {
                //If actor is marked as terminating begin buffered messages
                List<QueuedMessage> pending;
                if (!_pendingMessages.TryGetValue(envelope.Id, out pending))
                    _pendingMessages[envelope.Id] = pending = new List<QueuedMessage>();
                pending.Add(new QueuedMessage(envelope.Message));
            }
        }

        private void OnPassivate(Passivate passivate)
        {
            var desc = _activeActors.FirstOrDefault(p => p.Value.ActorRef.Equals(Sender)).Value;
            if(desc == null)
                return;
            
            desc.IsTerminating = true;
            Sender.Tell(PoisonPill.Instance);
        }

    }
}
