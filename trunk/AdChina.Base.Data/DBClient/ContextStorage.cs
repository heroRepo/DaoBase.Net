using System.Web;
using System.Threading;

namespace AdChina.Base.Data.DBClient
{
    internal class ContextStorage
    {
        static object _syncObj = new object();
        internal static void Save<T>(string key, T value)
        {
            lock (_syncObj)
            {
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Items.Contains(key))
                    {
                        HttpContext.Current.Items.Remove(key);
                    }
                    HttpContext.Current.Items.Add(key, value);
                }
                else
                {
                    Thread.SetData(Thread.GetNamedDataSlot(key), value);
                }
            }
        }

        internal static T Load<T>(string key)
        {
            lock (_syncObj)
            {
                if (HttpContext.Current != null)
                {
                    return (T)HttpContext.Current.Items[key];
                }
                else
                {
                    return (T)Thread.GetData(Thread.GetNamedDataSlot(key));
                }
            }
        }
    }
}
