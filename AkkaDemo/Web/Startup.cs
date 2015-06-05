using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;
using Akka.Actor;
using AkkaDemo.Actors;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Ninject;
using Ninject.Web.WebApi;
using Owin;
using Unity.WebApi;

namespace AkkaDemo.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            Microsoft.Owin.Infrastructure.SignatureConversions.AddConversions(appBuilder);
            var config = new HttpConfiguration();
            appBuilder.UseWebApi(config);
            Register(config);

        }

        private static TextWriter Log;

        public void Register(HttpConfiguration config)
        {
            config.Formatters.Clear();

            config.MapHttpAttributeRoutes();
            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter
            {
                SerializerSettings = new JsonSerializerSettings {Formatting = Formatting.Indented}
            });
            config.Formatters.Add(new JQueryMvcFormUrlEncodedFormatter());
            
            var unity = new UnityContainer();
            config.DependencyResolver = new UnityDependencyResolver(unity);
            unity.RegisterInstance(ActorSystemFactory.CreateMultiNodeSystemAndReturnFrontend());


            config.EnsureInitialized();
        }


    }
}
