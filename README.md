##延迟注入(Lazy Injection)
对IoC和DI的一个思考：
```
在属性注入中，属性注入的时机发生在我们首次使用属性时，即被注入的属性第一次在使用前都为null来提高性能和节约内存，我把这个方式叫做——"延迟注入(Lazy Injection)"
```
在我们的简易IoC Container中，提供了一个特性 InjectionAttribute
```cs
[AttributeUsage(AttributeTargets.Property , AllowMultiple = false)]
public class InjectionAttribute : Attribute
{
    public bool Lazy { get; set; }
}
```
为虚属性标记Injection并把Lazy设置为true来启动延迟注入，例如：
```cs
public class FooService : IFooService
{
    [Injection(Lazy = true)]
    public virtual ILogger Logger { get; set; }
    
    public FooService()
    {
    }
}
```
Logger实现ILogger接口，并在构造器中打印"Logger is creating .."
```cs
public class Logger : ILogger
{
    public Logger()
    {
        Console.WriteLine("Logger is creating ..");
    }
}
```
测试代码：
```cs
[TestMethod]
public void LazyInjection_Test()
{
    Container container = new Container();
    container.Register<ILogger , Logger>();
    container.Register<IFooService , FooService>();
    var foo = container.Resolve<IFooService>();
    Console.WriteLine("IFooService is created");
    Console.WriteLine("Logger is not create");
    var logger = foo.Logger;           
    Console.WriteLine("Logger is created");
    var logger2 = foo.Logger;
}
```
输出：
```		
IFooService is created
Logger is not create
Logger is creating ..
Logger is created
```

