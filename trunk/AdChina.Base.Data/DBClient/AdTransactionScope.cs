using System;

namespace AdChina.Base.Data.DBClient
{
    public class AdTransactionScope : IDisposable
    {
        bool _commited;
        bool _rollbacked;
        private DbHelper _helper;
        public AdTransactionScope()
        {
            _helper = DbHelper.GetHelper();
            _helper.BeginTransaction();
        }

        public AdTransactionScope(string connectionStringName)
        { 
            _helper = DbHelper.GetHelper(connectionStringName);
            _helper.BeginTransaction();
        }

        public void Complete()
        {
            try
            {
                _helper.Commit();
                _commited = true;
            }
            catch (Exception)
            {
                try
                {
                    _helper.RollBack();
                    _rollbacked = true;
                }
                catch (Exception)
                {
                    throw;
                }
                throw;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (!_commited && !_rollbacked)
                try
                {
                    _helper.RollBack();
                }
                catch (Exception)
                {
                    throw;
                }

        }

        #endregion
    }
}
