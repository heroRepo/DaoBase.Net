using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdChina.Base.Data.Provider
{
    /// <summary>
    /// 连接信息
    /// </summary>
    public class ConnectionInfo
    {
        /// <summary>
        /// 数据库提供程序名称
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
