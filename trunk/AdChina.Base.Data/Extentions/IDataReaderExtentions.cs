using AdChina.Base.Data.Mapping;
using AdChina.Base.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace AdChina.Base.Data.Extentions
{
    /// <summary>
    /// DataReader扩展
    /// </summary>
    public static class IDataReaderExtentions
    {
        /// <summary>
        /// 由DataReader直接生成实体对象迭代
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="reader">要进行查询的DataReader</param>
        /// <returns>实体对象迭代</returns>
        public static IEnumerable<T> ToEnumerable<T>(this IDataReader reader)
            where T : new()
        {
            foreach (var item in ToEnumerable<T>(reader, null))
            {
                yield return item;
            }
        }

        /// <summary>
        /// 由DataReader直接生成实体对象迭代
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="reader">要进行查询的DataReader</param>
        /// <param name="reader">数据字段和属性名映射关系</param>
        /// <returns>实体对象迭代</returns>
        public static IEnumerable<T> ToEnumerable<T>(this IDataReader reader, Dictionary<string, string> fieldPropertyMappings)
            where T : new()
        {
            var fieldPropertyMapping = DataFieldPropertyMapping<T>.GetDataFieldPropertyMapping(fieldPropertyMappings);
            //读取Reader值，类型转换，并赋给对象属性
            while (reader.Read())
            {
                //创建实体对象
                T obj = new T();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    //读取Reader值
                    var dbValue = reader.GetValue(i);
                    if (dbValue != DBNull.Value)
                    {
                        var fieldName = reader.GetName(i);
                        var mappingKey = fieldName.ToLower();
                        if (fieldPropertyMapping.ContainsKey(mappingKey))
                        {
                            fieldName = fieldPropertyMapping[mappingKey];
                            //赋给属性
                            PropertyHelper<T>.SetValue(obj, fieldName, dbValue);
                        }
                    }
                }
                yield return obj;
            }
        }
    }
}
