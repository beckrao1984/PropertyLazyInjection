using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PropertyLazyInjection
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void LazyInjection_Test()
        {
            Container container = new Container();
            container.Register<ILogger , Logger>();
            container.Register<IFooService , FooService>();
            var foo = container.Resolve<IFooService>();

            Console.WriteLine("IFooService is created");
            Console.WriteLine("Logger is not used");

            var logger = foo.Logger;
            
            Console.WriteLine("Logger is used");
        }
    }
}
