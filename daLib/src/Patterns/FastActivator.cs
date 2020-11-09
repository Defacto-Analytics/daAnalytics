

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace daLib.Patterns
{
    public class InitializedList<T> : List<T> where T : new()
    {
        public InitializedList() : base() { }
        public InitializedList(int size) : base(size)
        {
            for (int i = 0; i < this.Capacity; i++)
                this.Add(default(T) == null ? FastActivator<T>.Create() : default(T));
        }
        public InitializedList(int size, T value) : base(size)
        {
            for (int i = 0; i < this.Capacity; i++)
                this.Add(value);
        }

        // erases the contents without changing the size
        public void Erase()
        {
            for (int i = 0; i < this.Count; i++)
                this[i] = default(T);       // do we need to use "new T()" instead of default(T) when T is class?
        }
    }


    public static class FastActivator<T> where T : new()
    {
        public static readonly Func<T> Create = DynamicModuleLambdaCompiler.GenerateFactory<T>();
    }

    public static class DynamicModuleLambdaCompiler
    {
        public static Func<T> GenerateFactory<T>() where T : new()
        {
            Expression<Func<T>> expr = () => new T();
            NewExpression newExpr = (NewExpression)expr.Body;

            var method = new DynamicMethod(name: "lambda",
                                           returnType: newExpr.Type,
                                           parameterTypes: new Type[0],
                                           m: typeof(DynamicModuleLambdaCompiler).GetType().Module,
                                           skipVisibility: true);

            ILGenerator ilGen = method.GetILGenerator();
            // Constructor for value types could be null
            if (newExpr.Constructor != null)
            {
                ilGen.Emit(OpCodes.Newobj, newExpr.Constructor);
            }
            else
            {
                LocalBuilder temp = ilGen.DeclareLocal(newExpr.Type);
                ilGen.Emit(OpCodes.Ldloca, temp);
                ilGen.Emit(OpCodes.Initobj, newExpr.Type);
                ilGen.Emit(OpCodes.Ldloc, temp);
            }

            ilGen.Emit(OpCodes.Ret);

            return (Func<T>)method.CreateDelegate(typeof(Func<T>));
        }
    }



}
