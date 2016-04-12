using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace PropertyLazyInjection
{
    static class ILGenUtils
    {
        public static void EmitLoadArg(this ILGenerator il , int index)
        {
            switch (index)
            {
                case 0:
                    il.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    il.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    il.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    il.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if (index <= Byte.MaxValue) il.Emit(OpCodes.Ldarg_S , (byte)index);
                    else il.Emit(OpCodes.Ldarg , index);
                    break;
            }
        }

        internal static void EmitBaseConstructor(this ILGenerator il , ConstructorInfo constructor)
        {
            var parameters = constructor.GetParameters().Select(p => p.ParameterType).ToArray();
            il.EmitLoadArg(0);
            if (parameters.Any())
            {
                for (var index = 1 ; index <= parameters.Length ; index++)
                {
                    il.EmitLoadArg(index);
                }
            }
            il.Emit(OpCodes.Call , constructor);
        }

        private static readonly MethodInfo GetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle");
        internal static void EmitTypeof(this ILGenerator ilGenerator , Type type)
        {
            ilGenerator.Emit(OpCodes.Ldtoken , type);
            ilGenerator.EmitCall(OpCodes.Call , GetTypeFromHandle , null);
        }
    }
}
