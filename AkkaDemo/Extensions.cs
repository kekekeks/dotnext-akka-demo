using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster;
using AkkaDemo.Actors;

namespace AkkaDemo
{
    static class Extensions
    {

        public class MessageHandlingHelper
        {
            private readonly object _obj;
            private bool _handled;

            public MessageHandlingHelper(object obj)
            {
                _obj = obj;
            }

            public MessageHandlingHelper With<T>(Action<T> cb)
            {
                if (!_handled && _obj is T)
                {
                    cb((T) _obj);
                    _handled = true;
                }
                return this;
            }

            public static implicit operator bool(MessageHandlingHelper obj)
            {
                return obj._handled;
            }
        }

        public static MessageHandlingHelper Handle(this object obj)
        {
            return new MessageHandlingHelper(obj);
        }

        public static Task<TResult> Command<TResult>(this IActorRef actorRef, RequestBase<TResult> command)
        {
            return actorRef.Ask<TResult>(command);
        }

        public static string SelfAddress(this ActorSystem sys)
        {
            try
            {
                return Cluster.Get(sys).SelfAddress.ToString();
            }
            catch { }
            return "'local'";
        }
    }
}
