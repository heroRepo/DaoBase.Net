using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;

namespace AdChina.Base.Reflection
{
    public class PropertyHelper<T>
    {
        static Dictionary<string, Action<T, object>> PropertySetterActions;
        static PropertyHelper()
        {
            PropertySetterActions = new Dictionary<string, Action<T, object>>();
        }

        /// <summary>
        /// 为指定对象的目标属性进行赋值
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="name">目标属性名</param>
        /// <param name="value">要赋的值</param>
        public static void SetValue(T obj, string name, object value)
        {
            Action<T, object> action = null;
            if (!PropertySetterActions.ContainsKey(name))
            {
                Thread.MemoryBarrier();
                action = CreatePropertySetter(name);
                if (action != null)
                    PropertySetterActions[name] = action;
            }
            else
            {
                action = PropertySetterActions[name];
            }
            action(obj, value);
        }

        /// <summary>
        /// 生成属性赋值lambda
        /// </summary>
        /// <param name="name">属性名</param>
        /// <returns>属性赋值lambda</returns>
        static Action<T, object> CreatePropertySetter(string name)
        {
            Type objType = typeof(T);
            //设置参数
            //赋值目标对象参数
            var targetObj = Expression.Parameter(objType, "targetobj");
            //赋给对象的属性值参数
            var propertyValue = Expression.Parameter(typeof(object), "value");
            //值表达式
            Expression valueExpression = propertyValue;

            //获取属性
            var property = objType.GetProperty(name);
            if (property == null)
                throw new Exception(string.Format("{0}中不存在名称为{1}的属性", objType, name));
            var setMethod = property.GetSetMethod();
            if (setMethod == null)
                setMethod = property.GetSetMethod(true);
            if (setMethod == null)
                throw new Exception(string.Format("{0}的{1}属性没有可调用的Set方法", objType, name));

            //如果设置的值的类型和属性类型不一样，进行类型转换
            if (valueExpression.Type != property.PropertyType)
                valueExpression = Expression.Convert(Expression.Call(typeof(TypeConvertor).GetMethod("ConvertTo"), propertyValue, Expression.Constant(property.PropertyType)), property.PropertyType);
            //valueExpression = Expression.Convert(valueExpression, property.PropertyType);

            //调用SetMethod完成属性赋值
            var setValueExpression = Expression.Call(targetObj, setMethod, valueExpression);

            //生成Lambda表达式
            return Expression.Lambda<Action<T, object>>(setValueExpression, targetObj, propertyValue).Compile();
        }
    }

    public class PropertyHelper
    {
        static Dictionary<string, Action<object, object>> PropertySetterActions;
        static PropertyHelper()
        {
            PropertySetterActions = new Dictionary<string, Action<object, object>>();
        }

        /// <summary>
        /// 为指定对象的目标属性进行赋值
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="name">目标属性名</param>
        /// <param name="value">要赋的值</param>
        public static void SetValue(object obj, string name, object value)
        {
            Action<object, object> action = null;
            var type = obj.GetType();
            var key = type.FullName + "." + name;
            if (!PropertySetterActions.ContainsKey(key))
            {
                Thread.MemoryBarrier();
                action = CreatePropertySetter(type, name);
                if (action != null)
                    PropertySetterActions[key] = action;
            }
            else
            {
                action = PropertySetterActions[key];
            }
            action(obj, value);
        }

        /// <summary>
        /// 生成属性赋值lambda
        /// </summary>
        /// <param name="name">属性名</param>
        /// <returns>属性赋值lambda</returns>
        static Action<object, object> CreatePropertySetter(Type type, string name)
        {
            Type objType = type;
            //设置参数
            //赋值目标对象参数
            var targetObj = Expression.Parameter(typeof(object), "targetobj");
            //赋给对象的属性值参数
            var propertyValue = Expression.Parameter(typeof(object), "value");
            //值表达式
            Expression valueExpression = propertyValue;

            //获取属性
            var property = objType.GetProperty(name);
            if (property == null)
                throw new Exception(string.Format("{0}中不存在名称为{1}的属性", objType, name));
            var setMethod = property.GetSetMethod();
            if (setMethod == null)
                setMethod = property.GetSetMethod(true);
            if (setMethod == null)
                throw new Exception(string.Format("{0}的{1}属性没有可调用的Set方法", objType, name));

            //如果设置的值的类型和属性类型不一样，进行类型转换
            if (valueExpression.Type != property.PropertyType)
                valueExpression = Expression.Convert(Expression.Call(typeof(TypeConvertor).GetMethod("ConvertTo"), propertyValue, Expression.Constant(property.PropertyType)), property.PropertyType);
                //valueExpression = Expression.Convert(valueExpression, property.PropertyType);

            //调用SetMethod完成属性赋值
            var setValueExpression = Expression.Call(Expression.Convert(targetObj, type), setMethod, valueExpression);

            //生成Lambda表达式
            return Expression.Lambda<Action<object, object>>(setValueExpression, targetObj, propertyValue).Compile();
        }
    }
}
