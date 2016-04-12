using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace PropertyLazyInjection
{
    internal static class ProxyUtils
    {
        private static readonly AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Dynamic") , AssemblyBuilderAccess.Run);
        private static readonly ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("main");
        private static readonly ConcurrentDictionary<Type , Type> m_table = new ConcurrentDictionary<Type , Type>();
        public static Type CreateProxyType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return m_table.GetOrAdd(type , InternalCreateProxyType);
        }

        private static Type InternalCreateProxyType(Type type)
        {
            var builder = moduleBuilder.DefineType($"{type.FullName}_Proxy" , type.Attributes , type , type.GetInterfaces());
            DefineConstructor(type.GetTypeInfo().DeclaredConstructors , builder);
            var container = DefineContainerField(builder);
            DefineLazyProperty(type.GetTypeInfo().DeclaredProperties , container , builder);
            return builder.CreateType();
        }

        private static void DefineConstructor(IEnumerable<ConstructorInfo> constructors , TypeBuilder builder)
        {
            foreach (var ctor in constructors)
            {
                var constructorr = builder.DefineConstructor(ctor.Attributes ,
                      ctor.CallingConvention , ctor.GetParameters().Select(p => p.ParameterType).ToArray());
                var ilGen = constructorr.GetILGenerator();
                ilGen.EmitBaseConstructor(ctor);
                ilGen.Emit(OpCodes.Ret);
            }
        }
        private static FieldBuilder DefineContainerField(TypeBuilder builder)
        {
            return builder.DefineField("m_inner_contaniner" , typeof(Container) , FieldAttributes.Public);
        }

        private static readonly MethodInfo Resolve = typeof(Container).GetMethod("Resolve");
        private static void DefineLazyProperty(IEnumerable<PropertyInfo> properties , FieldBuilder container , TypeBuilder builder)
        {
            var attributes = MethodAttributes.Public | MethodAttributes.Virtual;
            foreach (var property in properties)
            {
                if (!property.CanRead || !property.CanWrite) continue;
                if (property.GetMethod.IsStatic || !property.GetMethod.IsVirtual) continue;
                var attr = property.GetCustomAttribute<InjectionAttribute>();
                if (attr == null || !attr.Lazy) continue;
                var propBuilder = builder.DefineProperty(property.Name , property.Attributes , property.PropertyType , null);
                var getMethod = builder.DefineMethod("get_" + property.Name , attributes , property.PropertyType , null);
                var ilGen = getMethod.GetILGenerator();

                var lable = ilGen.DefineLabel();

                ilGen.EmitLoadArg(0);
                ilGen.EmitCall(OpCodes.Call , property.GetMethod , null);
                ilGen.Emit(OpCodes.Brfalse_S , lable);

                ilGen.EmitLoadArg(0);
                ilGen.EmitCall(OpCodes.Call , property.GetMethod , null);
                ilGen.Emit(OpCodes.Ret);
                ilGen.MarkLabel(lable);

                ilGen.EmitLoadArg(0);
                ilGen.EmitLoadArg(0);
                ilGen.Emit(OpCodes.Ldfld , container);
                ilGen.EmitTypeof(property.PropertyType);
                ilGen.EmitCall(OpCodes.Callvirt , Resolve , null);
                if (property.PropertyType.IsValueType)
                    ilGen.Emit(OpCodes.Box , property.PropertyType);
                else
                    ilGen.Emit(OpCodes.Castclass , property.PropertyType);
                ilGen.EmitCall(OpCodes.Call , property.SetMethod , null);
                ilGen.EmitLoadArg(0);
                ilGen.EmitCall(OpCodes.Call , property.GetMethod , null);
                ilGen.Emit(OpCodes.Ret);

                var setMethod = builder.DefineMethod("set_" + property.Name , attributes , typeof(void) ,
                    new[] { property.PropertyType });
                ilGen = setMethod.GetILGenerator();
                ilGen.EmitLoadArg(0);
                ilGen.EmitLoadArg(1);
                ilGen.EmitCall(OpCodes.Call , property.SetMethod , null);
                ilGen.Emit(OpCodes.Ret);
                propBuilder.SetGetMethod(getMethod);
                propBuilder.SetSetMethod(setMethod);
            }
        }
    }
}
