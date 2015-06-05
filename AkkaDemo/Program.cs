using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Akka.Actor;
using AkkaDemo.Actors.OnDemand;
using AkkaDemo.Web;
using Microsoft.Owin.Hosting;

namespace AkkaDemo
{

    class Program
    {
        static void Main(string[] args)
        {
            using (WebApp.Start<Startup>("http://localhost:5000/"))
                Console.ReadLine();
        }
    }
}
