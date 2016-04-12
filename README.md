###IoC属性延迟注入解决思路
在我们的简易IoC Container中，我们提供了一个特性 InjectionAttribute
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
Logger实现ILogger接口，并在构造器中打印“Logger is created”：
```cs
public class Logger : ILogger
{
    public Logger()
    {
        Console.WriteLine("Logger is created");
    }
}
```
