using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AdChina.Base.Data.Mapping
{
    /// <summary>
    /// 数据字段和属性映射关系
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class DataFieldPropertyMapping<T>
    {
        private static Dictionary<string, string> _fieldPropertyMapping;

        static DataFieldPropertyMapping()
        {
            _fieldPropertyMapping = new Dictionary<string, string>();
            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                _fieldPropertyMapping[property.Name.ToLower()] = property.Name;
                ReadDataFieldPropertyMapping(property);
            }
        }

        /// <summary>
        /// 读取指定属性的字段属性映射
        /// </summary>
        /// <param name="property">可能带有DataFieldAttribute的属性</param>
        static void ReadDataFieldPropertyMapping(PropertyInfo property)
        {
            var attributes = property.GetCustomAttributes(typeof(DataFieldAttribute), true);
            foreach (DataFieldAttribute attribute in attributes)
            {
                if (!string.IsNullOrEmpty(attribute.FieldName))
                    _fieldPropertyMapping[attribute.FieldName.ToLower()] = property.Name;
                if (attribute.FieldNames != null)
                    foreach (var fieldName in attribute.FieldNames)
                    {
                        if (string.IsNullOrEmpty(fieldName))
                            continue;
                        _fieldPropertyMapping[fieldName.ToLower()] = property.Name;
                    }
            }
        }

        /// <summary>
        /// 获取T类型的DataFieldPropertyMapping信息
        /// </summary>
        /// <returns>DataFieldPropertyMapping信息</returns>
        public static Dictionary<string, string> GetDataFieldPropertyMapping()
        {
            return new Dictionary<string, string>(_fieldPropertyMapping);
        }

        /// <summary>
        /// 获取T类型的DataFieldPropertyMapping信息
        /// </summary>
        /// <param name="mergeMapping">需要进行合并的映射关系</param>
        /// <returns>DataFieldPropertyMapping信息</returns>
        public static Dictionary<string, string> GetDataFieldPropertyMapping(Dictionary<string, string> mergeMapping)
        {
            var mappings = new Dictionary<string, string>(_fieldPropertyMapping);
            if (mergeMapping != null)
                foreach (var kv in mergeMapping)
                {
                    mappings[kv.Key.ToLower()] = kv.Value;
                }
            return mappings;
        }
    }
}
