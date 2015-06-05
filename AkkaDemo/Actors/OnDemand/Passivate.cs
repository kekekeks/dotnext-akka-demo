using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaDemo.Actors.OnDemand
{
    public sealed class Passivate
    {
        public static Passivate Instance { get; } = new Passivate();
        private Passivate()
        {
            
        }
    }
}
