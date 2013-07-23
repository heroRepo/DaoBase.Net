using AdChina.Base.Data.Provider;
using AdChina.Base.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace AdChina.Base.Data.Dynamic.Extentions
{
    public static class IDataReaderExtentions
    {
        /// <summary>
        /// 由DataReader直接生成实体对象迭代
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="reader">要进行查询的DataReader</param>
        /// <returns>实体对象迭代</returns>
        public static IEnumerable<dynamic> ToEnumerable(this IDataReader reader)
        {
            var columns = Enumerable.Range(0, reader.FieldCount).Select(i => new DynamicProperty(reader.GetName(i), reader.GetFieldType(i))).ToArray();
            var type = ClassFactory.Instance.GetDynamicClass(columns);
            while (reader.Read())
            {
                IList<object> values = new List<object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    //读取Reader值
                    var dbValue = reader.GetValue(i);
                    //var fieldName = reader.GetName(i);
                    if (dbValue == DBNull.Value)
                    {
                        if (columns[i].Type.IsValueType)
                            dbValue = Activator.CreateInstance(columns[i].Type);
                    }
                    values.Add(dbValue);
                }
                yield return AdChina.Base.Dynamic.DynamicCreator.CreateWithConstructor(type, columns.Select(c => c.Type).ToArray(), values.ToArray());
            }
        }
    }
}
