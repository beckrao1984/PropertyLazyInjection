using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyLazyInjection
{
    public class FooService : IFooService
    {
        [Injection(Lazy = true)]
        public virtual ILogger Logger { get; set; }
        public FooService()
        {
        }
    }
}
