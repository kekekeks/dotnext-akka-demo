using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Routing;
using Akka.Configuration;
using Akka.Configuration.Hocon;
using Akka.Routing;
using AkkaDemo.Actors;
using AkkaDemo.Actors.OnDemand;

namespace AkkaDemo
{
    public static class ActorSystemFactory
    {

        public static ActorSystem CreateSingleNodeSystem()
        {
            var sys = ActorSystem.Create("demo");
            sys.ActorOf<OnDemandActorManager<Guid, AccountActor>>("accounts");
            return sys;
        }

        public static ActorSystem CreateMultiNodeSystemAndReturnFrontend()
        {
            ActorSystem rv = null;
            var defconfig = ((AkkaConfigurationSection) ConfigurationManager.GetSection("akka")).AkkaConfig;
            for (var c = 0; c < 3; c++)
            {
                var frontend = c == 2;
                var config = ConfigurationFactory.ParseString($"akka.remote.helios.tcp.port={2550 + c}")
                    .WithFallback(
                        ConfigurationFactory.ParseString($"akka.cluster.roles = [{(frontend ? "frontend" : "backend")}]"))
                    .WithFallback(defconfig);

                var system = ActorSystem.Create("demo", config);
                if (!frontend)
                {
                    system.ActorOf<OnDemandActorManager<Guid, AccountActor>>("accounts-shard");
                }
                else
                {
                    rv = system;
                    //TODO: Replace when proper sharding is done, DO NOT use in production

                    system.ActorOf(
                        new ClusterRouterGroup(new ConsistentHashingGroup("/user/accounts-shard").WithHashMapping(obj => (obj as OnDemandActorEnvelope<Guid>)?.Id),
                            new ClusterRouterGroupSettings(2, false, "backend",
                                ImmutableHashSet.Create("/user/accounts-shard"))).Props(), "accounts");
                }
            }
            return rv;

        }
    }
}
