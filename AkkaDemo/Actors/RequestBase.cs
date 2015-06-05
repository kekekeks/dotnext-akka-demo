using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaDemo.Actors
{
    public abstract class RequestBase<TResult>
    {
    }

    public abstract class GenericRequest : RequestBase<GenericRequestResult>
    {
        
    }

    public class GenericRequestResult
    {
        public GenericRequestResult(bool success)
        {
            Success = success;
        }

        public bool Success { get; }
    }
}
