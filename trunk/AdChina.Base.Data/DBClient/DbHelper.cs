using System;
using System.Linq;
using System.IO;
using System.Data;
using AdChina.Base.Data.Provider;
using System.Collections.Generic;

namespace AdChina.Base.Data.DBClient
{
    public class DbHelper : IDisposable
    {
        private static object _syncObj = new object();
        private static string _contextStoreKey = "CONTEXT_STORE_KEY";
        public IDbConnection Connection { get; set; }
        private IDbTransaction _transaction;
        private bool _isInActiveTransaction;
        public DbClientProvider Provider { get; set; }

        public static TextWriter Output
        {
            get;
            set;
        }

        public static void CloseAll()
        {
            Dictionary<string, DbHelper> helpers = ContextStorage.Load<Dictionary<string, DbHelper>>(_contextStoreKey);
            if (helpers != null)
            {
                try
                {
                    foreach (var value in helpers.Values)
                    {
                        if (value.Connection != null && value.Connection.State != ConnectionState.Closed)
                            value.Connection.Close();
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        /// <summary>
        /// 获取默认连接的当前DbHelper
        /// </summary>
        [Obsolete]
        public static DbHelper Current
        {
            get
            {
                return GetHelper();
            }
        }

        public static DbHelper GetHelper()
        {
            return GetHelper(DbClientProvider.DefaultConnectionStringName);
        }

        public static DbHelper GetHelper(string connectionStringName)
        {
            Dictionary<string, DbHelper> helpers = ContextStorage.Load<Dictionary<string, DbHelper>>(_contextStoreKey);
            if (helpers != null && helpers.ContainsKey(connectionStringName))
            {
                var value = helpers[connectionStringName];
                if (value != null && !value.IsDisposing && !value.IsDisposed)
                    return value;
            }
            if (helpers == null)
                helpers = new Dictionary<string, DbHelper>();

            lock (_syncObj)
            {
                var value = new DbHelper();
                value.Provider = DbClientProvider.GetClientProvider(connectionStringName);
                value.Connection = value.Provider.CreateConnection();
                helpers[connectionStringName] = value;
                ContextStorage.Save(_contextStoreKey, helpers);
                return value;
            }
        }

        public void ReConnectIfRequired()
        {
            if (Connection.State == ConnectionState.Closed || Connection.State != ConnectionState.Open)
            {
                try
                {
                    Connection.Open();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        internal IDbTransaction BeginTransaction()
        {
            if (Connection == null)
            {
                throw new Exception("Connection为空");
            }
            ReConnectIfRequired();
            try
            {
                _transaction = Connection.BeginTransaction();
                _isInActiveTransaction = true;
                return _transaction;
            }
            catch (Exception)
            {
                _isInActiveTransaction = false;
                throw;
            }
        }

        internal void Commit()
        {
            if (_transaction == null)
            {
                throw new Exception("Transaction为空");
            }
            try
            {
                _transaction.Commit();
                _isInActiveTransaction = false;
                Connection.Close();
                _transaction = null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal void RollBack()
        {
            if (_transaction == null)
            {
                throw new Exception("Transaction为空");
            }
            try
            {
                _transaction.Rollback();
                _isInActiveTransaction = false;
                Connection.Close();
                _transaction = null;
            }
            catch (Exception)
            {
                throw;
            }
        }



        //public DataTable ExecuteDataTable(string sql, params ObjectParameter[] paras)
        //{
        //    return ExecuteDataTable(CommandType.Text, sql, paras);
        //}

        //public DataTable ExecuteDataTable(CommandType type, string sql, params ObjectParameter[] paras)
        //{
        //    return ExecuteDataTable(type, sql, paras.Select(p => p.ToDbDataParameter()).ToArray());
        //}

        //public DataTable ExecuteDataTable(string sql, params IDbDataParameter[] paras)
        //{
        //    return ExecuteDataTable(CommandType.Text, sql, paras);
        //}

        //public DataTable ExecuteDataTable(CommandType type, string sql, params IDbDataParameter[] paras)
        //{
        //    ReConnectIfRequired();
        //    var command = Connection.CreateCommand();
        //    command.CommandText = sql;
        //    command.CommandType = type;
        //    foreach (var para in paras)
        //    {
        //        command.Parameters.Add(para);
        //    }
        //    if (_isInActiveTransaction)
        //    {
        //        command.Transaction = _transaction;
        //    }
        //    try
        //    {
        //        var reader = command.ExecuteReader();
        //        var dt = reader.GetSchemaTable();
        //        while (reader.Read())
        //        {
        //            var row = dt.NewRow();
        //            for (int i = 0; i < reader.FieldCount; i++)
        //            {
        //                row[reader.GetName(i)] = reader[i];
        //            }
        //            dt.Rows.Add(row);
        //        }
        //        reader.Close();
        //        return dt;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    finally
        //    {
        //        if (!_isInActiveTransaction)
        //        {
        //            command.Parameters.Clear();
        //            Connection.Close();
        //        }
        //    }
        //}

        public int ExecuteNonQuery(string sql, params ObjectParameter[] paras)
        {
            return ExecuteNonQuery(CommandType.Text, sql, paras);
        }

        public int ExecuteNonQuery(CommandType type, string sql, params ObjectParameter[] paras)
        {
            ReConnectIfRequired();
            var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = type;
            foreach (var para in paras)
            {
                command.Parameters.Add(para.ToDbDataParameter(Provider));
            }
            if (_isInActiveTransaction)
            {
                command.Transaction = _transaction;
            }
            try
            {
                return command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (!_isInActiveTransaction)
                {
                    command.Parameters.Clear();
                }
            }
        }

        public long GetScalar(string selStr, params ObjectParameter[] paras)
        {
            object obj = this.ExecuteScalar(selStr, paras);
            return obj == null ? 0 : Convert.ToInt64(obj);
        }

        public object ExecuteScalar(string sql, params ObjectParameter[] paras)
        {
            return ExecuteScalar(CommandType.Text, sql, paras);
        }

        public object ExecuteScalar(CommandType type, string sql, params ObjectParameter[] paras)
        {
            ReConnectIfRequired();
            var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = type;
            foreach (var para in paras)
            {
                command.Parameters.Add(para.ToDbDataParameter(Provider));
            }
            if (_isInActiveTransaction)
            {
                command.Transaction = _transaction;
            }
            try
            {
                var obj = command.ExecuteScalar();
                return obj;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (!_isInActiveTransaction)
                {
                    command.Parameters.Clear();
                }
            }
        }

        public AdDataReader ExecuteDataReader(string sql, params ObjectParameter[] paras)
        {
            return ExecuteDataReader(CommandType.Text, sql, paras);
        }

        public AdDataReader ExecuteDataReader(CommandType type, string sql, params ObjectParameter[] paras)
        {
            ReConnectIfRequired();
            var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = type;
            foreach (var para in paras)
            {
                command.Parameters.Add(para.ToDbDataParameter(Provider));
            }
            if (_isInActiveTransaction)
            {
                command.Transaction = _transaction;
            }
            try
            {
                return new AdDataReader(command.ExecuteReader());
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                command.Parameters.Clear();
            }
        }

        public AdDataReader GetDataReader(string sqlStr, params ObjectParameter[] paras)
        {
            ReConnectIfRequired();
            var command = Connection.CreateCommand();
            string sql = sqlStr;
            command.CommandText = sql;
            if (_isInActiveTransaction)
            {
                command.Transaction = _transaction;
            }
            try
            {
                return this.ExecuteDataReader(sql, paras);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                command.Parameters.Clear();
            }

        }

        public AdDataReader GetDataReader(string sqlStr, int pageid, int pageSize, params ObjectParameter[] paras)
        {
            ReConnectIfRequired();
            var command = Connection.CreateCommand();
            string sql = "";
            if (pageid >= 0 && pageSize >= 0)
            {
                long startIndex = (pageid) * pageSize + 1;
                long endIndex = (pageid + 1) * pageSize;
                sql = "select * from (select rownum rn, tablea.* from (" + sqlStr + ") tablea " + (endIndex > 0 ? "where rownum<=" + endIndex : "") + ") where rn>=" + startIndex;
            }
            else
                sql = sqlStr;

            command.CommandText = sql;
            if (_isInActiveTransaction)
            {
                command.Transaction = _transaction;
            }
            try
            {
                return this.ExecuteDataReader(sql, paras);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                command.Parameters.Clear();
            }

        }

        public AdDataReader GetDataReader(string columnStr, string tableStr, string whereStr, string orderStr, int pageid, int pageSize, params ObjectParameter[] paras)
        {
            ReConnectIfRequired();
            var command = Connection.CreateCommand();
            string sql = "";
            if (pageid >= 0 && pageSize >= 0)
            {
                long startIndex = (pageid) * pageSize + 1;
                long endIndex = (pageid + 1) * pageSize;
                sql = "select * from (select rownum rn, tablea.* from (select " + columnStr + " from " + tableStr + " where " + whereStr + " order by " + orderStr + ") tablea " + (endIndex > 0 ? "where rownum<=" + endIndex : "") + ") where rn>=" + startIndex;
            }
            else
                sql = "SELECT " + columnStr + " FROM " + tableStr + " WHERE " + whereStr + " ORDER BY " + orderStr;

            command.CommandText = sql;
            if (_isInActiveTransaction)
            {
                command.Transaction = _transaction;
            }
            try
            {
                return this.ExecuteDataReader(sql, paras);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                command.Parameters.Clear();
            }

        }

        public AdDataReader GetDataReader(string sqlStr, int pageid, int pageSize, ref long count, params ObjectParameter[] paras)
        {

            ReConnectIfRequired();
            var command = Connection.CreateCommand();
            string sql = "";
            if (pageid >= 0 && pageSize >= 0)
            {
                long startIndex = (pageid) * pageSize + 1;
                long endIndex = (pageid + 1) * pageSize;
                sql = "select * from (select rownum rn, tablea.* from (" + sqlStr + ") tablea " + (endIndex > 0 ? "where rownum<=" + endIndex : "") + ") where rn>=" + startIndex;
            }
            else
                sql = sqlStr;

            count = this.GetScalar("SELECT count(*) FROM (" + sqlStr + ")", paras);
            command.CommandText = sql;
            if (_isInActiveTransaction)
            {
                command.Transaction = _transaction;
            }
            try
            {
                return this.ExecuteDataReader(sql, paras);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                command.Parameters.Clear();
            }

        }

        public AdDataReader GetDataReader(string columnStr, string tableStr, string whereStr, string orderStr, int pageid, int pageSize, ref long count, params ObjectParameter[] paras)
        {

            ReConnectIfRequired();
            var command = Connection.CreateCommand();
            string sql = "";
            if (pageid >= 0 && pageSize >= 0)
            {
                long StartIndex = (pageid) * pageSize + 1;
                long EndIndex = (pageid + 1) * pageSize;
                sql = "select * from (select rownum rn, tablea.* from (select " + columnStr + " from " + tableStr + (string.IsNullOrEmpty(whereStr) ? "" : " WHERE " + whereStr) + " order by " + orderStr + ") tablea " + (EndIndex > 0 ? "where rownum<=" + EndIndex : "") + ") where rn>=" + StartIndex;
            }
            else
                sql = "SELECT " + columnStr + " FROM " + tableStr + " WHERE " + whereStr + " ORDER BY " + orderStr;

            count = this.GetScalar("SELECT count(*) FROM (SELECT " + columnStr + " FROM " + tableStr + (string.IsNullOrEmpty(whereStr) ? "" : " WHERE " + whereStr) + ")", paras);
            command.CommandText = sql;
            if (_isInActiveTransaction)
            {
                command.Transaction = _transaction;
            }
            try
            {
                return this.ExecuteDataReader(sql, paras);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                command.Parameters.Clear();
            }

        }



        //public long GetNextVal(string tablename)
        //{
        //    long nextID;
        //    ReConnectIfRequired();
        //    var command = Connection.CreateCommand();
        //    if (_isInActiveTransaction)
        //    {
        //        command.Transaction = _transaction;
        //    }
        //    try
        //    {
        //        if (tablename.ToLower().IndexOf(".nextval") >= 0)
        //        {
        //            command.CommandText = "SELECT " + tablename + " FROM DUAL WHERE rownum<=1";
        //            object next = command.ExecuteScalar();
        //            nextID = Convert.ToInt64(next);
        //        }
        //        else
        //        {
        //            command.CommandText = "SELECT next_id FROM pk_mapping WHERE table_name='" + tablename + "'";
        //            object next = command.ExecuteScalar();

        //            if (next == null)
        //            {
        //                nextID = 1;
        //                command.CommandText = "INSERT INTO pk_mapping(table_name,next_id) VALUES ('" + tablename + "',1)";
        //                command.ExecuteNonQuery();
        //            }
        //            else
        //                nextID = Convert.ToInt64(next);

        //            command.CommandText = "UPDATE pk_mapping SET Next_id=next_id+STEP where table_name='" + tablename + "'";
        //            command.ExecuteNonQuery();
        //        }
        //        return nextID;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    finally
        //    {
        //    }

        //    return nextID;
        //}

        private bool _isDisposed;

        public void Dispose()
        {
            IsDisposing = true;
            if (_isDisposed)
                return;
            try
            {
                if (Connection != null && Connection.State != ConnectionState.Closed)
                {
                    Connection.Close();
                }
            }
            catch (Exception)
            {

            }
            _isDisposed = true;
        }

        ~DbHelper()
        {
            Dispose();
        }

        public bool IsDisposed
        {
            get { return _isDisposed; }
        }

        public bool IsDisposing { get; set; }
    }
}
