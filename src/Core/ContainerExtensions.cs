using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyLazyInjection
{
   static class ContainerExtensions
    {
        public static Container Register<TService, TImplementation>(this Container container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            container.Register(typeof(TService) , typeof(TImplementation));
            return container;
        }

        public static TService Resolve<TService>(this Container container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            return (TService)container.Resolve(typeof(TService));
        }

    }
}
