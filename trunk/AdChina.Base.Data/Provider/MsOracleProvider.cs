using AdChina.Base.Data.DBClient;
using System.Data;

namespace AdChina.Base.Data.Provider
{
    internal class MsOracleProvider : DbClientProvider
    {
        public MsOracleProvider()
        {
            Regist("System.Data.OracleClient", this);
            _dbProviderFactory = System.Data.OracleClient.OracleClientFactory.Instance;
        }

        public override IDbDataParameter ConvertObjectParameterToDataParameter(ObjectParameter objectParameter)
        {
            var parameterName = objectParameter.Name.Replace("@", ":").Trim();
            if (!parameterName.StartsWith(":"))
            {
                parameterName = ":" + parameterName;
            }
            objectParameter.Name = parameterName;
            return base.ConvertObjectParameterToDataParameter(objectParameter);
        }
    }
}
