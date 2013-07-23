using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdChina.Base.Reflection
{
    public class TypeConvertor
    {
        public static object ConvertTo(object sourcevalue, Type desType)
        {
            switch (desType.FullName)
            {
                case "System.Int32":
                    return Convert.ToInt32(sourcevalue);
                case "System.Boolean":
                    return Convert.ToBoolean(sourcevalue);
                case "System.Byte":
                    return Convert.ToByte(sourcevalue);
                case "System.Char":
                    return Convert.ToChar(sourcevalue);
                case "System.DateTime":
                    return Convert.ToDateTime(sourcevalue);
                case "System.Decimal":
                    return Convert.ToDecimal(sourcevalue);
                case "System.Double":
                    return Convert.ToDouble(sourcevalue);
                case "System.Int16":
                    return Convert.ToInt16(sourcevalue);
                case "System.Int64":
                    return Convert.ToInt64(sourcevalue);
                case "System.SByte":
                    return Convert.ToSByte(sourcevalue);
                case "System.Single":
                    return Convert.ToSingle(sourcevalue);
                case "System.String":
                    return Convert.ToString(sourcevalue);
                case "System.UInt6":
                    return Convert.ToUInt16(sourcevalue);
                case "System.UInt32":
                    return Convert.ToUInt32(sourcevalue);
                case "System.UInt64":
                    return Convert.ToUInt64(sourcevalue);
                default:
                    break;
            }
            if (desType.IsValueType)
                return Activator.CreateInstance(desType);
            return default(object);
        }
    }
}
