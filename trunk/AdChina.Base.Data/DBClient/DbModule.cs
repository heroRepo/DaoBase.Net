using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace AdChina.Base.Data.DBClient
{
    /// <summary>
    /// 控制数据库连接统一关闭的module
    /// </summary>
    public class DbModule : IHttpModule
    {
        private HttpApplication _context;
        public void Init(HttpApplication context)
        {
            _context = context;
            context.EndRequest += Application_EndRequest;
        }

        void Application_EndRequest(object sender, EventArgs e)
        {
            //请求退出时，如果有数据库连接，尝试关闭
            DbHelper.CloseAll();
        }

        public void Dispose()
        {
            if (_context != null)
                _context.EndRequest -= Application_EndRequest;
        }
    }
}
