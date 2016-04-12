using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace PropertyLazyInjection
{
    public sealed class Container
    {
        private readonly ConcurrentDictionary<Type , Type> m_table = new ConcurrentDictionary<Type , Type>();

        public void Register(Type serviceType , Type implementationType)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));

            if (implementationType.GetTypeInfo().IsAbstract || implementationType.GetTypeInfo().IsInterface)
                throw new ArgumentException($"实现类型{implementationType.FullName}不能为抽象类或接口");

            if (!serviceType.GetTypeInfo().IsAssignableFrom(implementationType.GetTypeInfo()))
                throw new ArgumentException($"类型{implementationType.FullName}未实现或继承{serviceType.FullName}");

            m_table[serviceType] = implementationType;
        }

        public object Resolve(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            Type implementationType;

            if(!m_table.TryGetValue(serviceType,out implementationType))
            {
                throw new ArgumentException($"类型{serviceType.FullName}未注册");
            }

            if (HasLazyInjectionProperty(implementationType.GetTypeInfo()))
            {
                implementationType = ProxyUtils.CreateProxyType(implementationType);
            }

            return InternalResolve(implementationType);
        }

        private object InternalResolve(Type implementationType)
        { 
            object service = CreateService(implementationType);
            SetLazyInjectionProperty(implementationType.GetTypeInfo() , service);
            SetInjectionProperty(implementationType.GetTypeInfo() , service);
            return service;
        }

        private object CreateService(Type implementationType)
        {
            var constructor = implementationType.GetTypeInfo().DeclaredConstructors.First();
            var parameters = constructor.GetParameters().Select(c => Resolve(c.ParameterType)).ToArray();
            return constructor.Invoke(parameters);
        }


        private void SetLazyInjectionProperty(TypeInfo proxyType , object instance)
        {
            var field = proxyType.GetField($"m_inner_contaniner");
            if (field != null) field.SetValue(instance , this);
        }

        private void SetInjectionProperty(TypeInfo proxyType , object instance)
        {
            foreach (var property in proxyType.DeclaredProperties)
            {
                if (!m_table.ContainsKey(property.PropertyType)) continue;
                var attr = property.GetCustomAttribute<InjectionAttribute>();
                if (attr == null) continue;
                if (attr.Lazy) continue;
                property.SetValue(instance , Resolve(property.PropertyType));
            }
        }

        private bool HasLazyInjectionProperty(TypeInfo proxyType)
        {
            foreach (var property in proxyType.DeclaredProperties)
            {
                if (!m_table.ContainsKey(property.PropertyType)) continue;
                var attr = property.GetCustomAttribute<InjectionAttribute>();
                if (attr == null) continue;
                if (attr.Lazy) return true;
            }
            return false;
        }
    }
}
