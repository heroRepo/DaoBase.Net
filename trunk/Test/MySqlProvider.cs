using AdChina.Base.Data.DBClient;
using System.Data;

namespace AdChina.Base.Data.Provider
{
    /*
     * 可以自己定义新的Provider，来实现对更多数据库的支持
     */
    internal class MySqlProvider : DbClientProvider
    {
        public MySqlProvider()
        {
            Regist("MySql.Data.MySqlClient", this);
            _dbProviderFactory = MySql.Data.MySqlClient.MySqlClientFactory.Instance;
        }

        public override IDbDataParameter ConvertObjectParameterToDataParameter(ObjectParameter objectParameter)
        {
            var parameterName = objectParameter.Name.Replace(":", "@").Trim();
            if (!parameterName.StartsWith("@"))
            {
                parameterName = "@" + parameterName;
            }
            objectParameter.Name = parameterName;
            return base.ConvertObjectParameterToDataParameter(objectParameter);
        }
    }
}
