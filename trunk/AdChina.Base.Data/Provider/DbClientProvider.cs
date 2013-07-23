using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AdChina.Base.Data.Provider
{
    public class DbClientProvider
    {
        /// <summary>
        /// 默认读取应用程序配置文件中名称为ConnectionString的连接字符串
        /// </summary>
        public const string DefaultConnectionStringName = "ConnectionString";
        protected static Dictionary<string, ConnectionInfo> ConnectionInfos = new Dictionary<string, ConnectionInfo>();
        private static Dictionary<string, DbClientProvider> _providers = new Dictionary<string, DbClientProvider>();
        protected DbProviderFactory _dbProviderFactory;
        protected ConnectionInfo _connectionInfo;
        static DbClientProvider() { Init(); }
        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            //获取所有的DbClientProvider
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.GlobalAssemblyCache)
                {
                    Type[] types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (type.IsSubclassOf(typeof(DbClientProvider)))
                        {
                            Activator.CreateInstance(type);
                        }
                    }
                }
            }

            //获取所有的连接信息
            foreach (ConnectionStringSettings item in ConfigurationManager.ConnectionStrings)
            {
                ConnectionInfos[item.Name] = new ConnectionInfo { ConnectionString = item.ConnectionString, ProviderName = (item.ProviderName ?? "System.Data.OracleClient") };
            }
        }

        /// <summary>
        /// 注册提供程序DbClientProvider
        /// </summary>
        /// <param name="providerName">提供程序名称</param>
        /// <param name="provider">DbClientProvider的实现</param>
        public static void Regist(string providerName, DbClientProvider provider)
        {
            _providers[providerName.Trim()] = provider;
        }

        /// <summary>
        /// 获取默认连接名称的数据库客户端提供程序
        /// </summary>
        /// <returns></returns>
        public static DbClientProvider GetClientProvider()
        {
            return GetClientProvider(DefaultConnectionStringName);
        }

        /// <summary>
        /// 获取指定连接名称的数据库客户端提供程序
        /// </summary>
        /// <param name="connectionStringName"></param>
        /// <returns></returns>
        public static DbClientProvider GetClientProvider(string connectionStringName)
        {
            if (ConnectionInfos.ContainsKey(connectionStringName))
            {
                var connectionInfo = ConnectionInfos[connectionStringName];
                var provider = _providers[connectionInfo.ProviderName].MemberwiseClone() as DbClientProvider;
                provider._connectionInfo = connectionInfo;
                return provider;
            }
            throw new Exception(string.Format("不存在连接{0}", connectionStringName));
        }

        internal System.Data.IDbConnection CreateConnection()
        {
            var connection = _dbProviderFactory.CreateConnection();
            connection.ConnectionString = _connectionInfo.ConnectionString;
            return connection;
        }

        internal System.Data.IDbCommand CreateCommand()
        {
            return _dbProviderFactory.CreateCommand();
        }

        internal System.Data.IDbDataAdapter CreateDataAdapter()
        {
            return _dbProviderFactory.CreateDataAdapter();
        }

        internal System.Data.IDbDataParameter CreateDataParameter()
        {
            return _dbProviderFactory.CreateParameter();
        }

        public virtual IDbDataParameter ConvertObjectParameterToDataParameter(DBClient.ObjectParameter objectParameter)
        {
            var para = CreateDataParameter();
            para.ParameterName = objectParameter.Name;
            para.Value = objectParameter.Value;
            return para;
        }

        public static DbType TypeToDbType(Type t)
        {
            DbType dbt;
            try
            {
                dbt = (DbType)Enum.Parse(typeof(DbType), t.Name);
            }
            catch
            {
                dbt = DbType.Object;
            }
            return dbt;
        }

        public static Type DbTypeToType(DbType dbType)
        {
            Type toReturn = typeof(DBNull);

            switch (dbType)
            {
                case DbType.UInt64:
                    toReturn = typeof(UInt64);
                    break;

                case DbType.Int64:
                    toReturn = typeof(Int64);
                    break;

                case DbType.Int32:
                    toReturn = typeof(Int32);
                    break;

                case DbType.UInt32:
                    toReturn = typeof(UInt32);
                    break;

                case DbType.Single:
                    toReturn = typeof(float);
                    break;

                case DbType.Date:
                case DbType.DateTime:
                case DbType.Time:
                    toReturn = typeof(DateTime);
                    break;

                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                    toReturn = typeof(string);
                    break;

                case DbType.UInt16:
                    toReturn = typeof(UInt16);
                    break;

                case DbType.Int16:
                    toReturn = typeof(Int16);
                    break;

                case DbType.SByte:
                    toReturn = typeof(byte);
                    break;

                case DbType.Object:
                    toReturn = typeof(object);
                    break;

                case DbType.VarNumeric:
                case DbType.Decimal:
                    toReturn = typeof(decimal);
                    break;

                case DbType.Currency:
                    toReturn = typeof(double);
                    break;

                case DbType.Binary:
                    toReturn = typeof(byte[]);
                    break;

                case DbType.Double:
                    toReturn = typeof(Double);
                    break;

                case DbType.Guid:
                    toReturn = typeof(Guid);
                    break;

                case DbType.Boolean:
                    toReturn = typeof(bool);
                    break;
            }

            return toReturn;
        }
    }
}
