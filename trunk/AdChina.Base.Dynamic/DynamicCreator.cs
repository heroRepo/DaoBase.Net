using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace AdChina.Base.Dynamic
{
    public class DynamicCreator
    {
        public static dynamic CreateWithConstructor(Type type, Type[] constructorParaTypes, params object[] paras)
        {
            return CreateObjectCreator(type, constructorParaTypes, paras)(paras);
        }

        public static Func<object[], dynamic> CreateObjectCreator(Type type, Type[] constructorParaTypes, params object[] paras)
        {
            var constructor = type.GetConstructor(constructorParaTypes);
            var constructorParaExpressions = constructor.GetParameters().Select(p => Expression.Parameter(p.ParameterType, p.Name));
            var args = Expression.Parameter(typeof(object[]), "args");
            var body = Expression.New(constructor, constructorParaExpressions.Select((t, i) => Expression.Convert(Expression.ArrayIndex(args, Expression.Constant(i)), t.Type)).ToArray());
            var outer = Expression.Lambda<Func<object[], object>>(body, args);
            return Expression.Lambda<Func<object[], dynamic>>(body, args).Compile();
        }
    }
}
