using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyLazyInjection
{
    [AttributeUsage(AttributeTargets.Property , AllowMultiple = false)]
    public class InjectionAttribute : Attribute
    {
        public bool Lazy { get; set; }
    }
}
