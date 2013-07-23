using System;
using System.Data;
using AdChina.Base.Data.Provider;

namespace AdChina.Base.Data.DBClient
{
    /// <summary>
    /// 数据库参数
    /// </summary>
    public class ObjectParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public Type Type { get; set; }
        public DbType DbType
        {
            get
            {
                DbType dbt;
                if (Type == null)
                    Type = Value.GetType();

                try
                {
                    dbt = (DbType)Enum.Parse(typeof(DbType), Type.Name);
                }
                catch
                {
                    dbt = DbType.Object;
                }
                return dbt;
            }
        }

        public IDbDataParameter ToDbDataParameter(DbClientProvider provider)
        {
            return provider.ConvertObjectParameterToDataParameter(this);
            //throw new NotImplementedException();
        }
    }
}
