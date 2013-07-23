using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdChina.Base.Data.Mapping
{
    /// <summary>
    /// 字段名特性
    /// </summary>
    public class DataFieldAttribute : Attribute
    {
        private string[] _fieldNames; 
        /// <summary>
        /// 映射到同一个属性名的多个字段名
        /// </summary>
        public string[] FieldNames
        {
            get { return _fieldNames; }
            set { _fieldNames = value; }
        }

        private string _fieldName;
        /// <summary>
        /// 映射到属性名的字段名
        /// </summary>
        public string FieldName
        {
            get { return _fieldName; }
            set 
            {
                _fieldName = value; 
            }
        }
    }
}
