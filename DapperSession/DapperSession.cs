using System;
using System.Data;

namespace DapperSession
{
    public class DapperSession : IDisposable
    {

        private readonly IConnectionFactory _connectionFactory;

        private IDbConnection _activeConnection;
        private IDbTransaction _activeTransaction;

        public DapperSession(IConnectionFactory connectionFactory)
        {
            
            _connectionFactory = connectionFactory;
        }

        private void OpenConnectionIfNeedded()
        {
            if(_activeTransaction != null)
                return;
            
            _activeConnection = _connectionFactory.GetConnection();
            _activeTransaction = _activeConnection.BeginTransaction();
        }

        private void DisposeConnectionIfNeedded()
        {
            if(_activeTransaction != null)
                return;
            
            _activeConnection.Dispose();
            _activeConnection = null;
        }

        public void BeginTransaction()
        {
            if(_activeTransaction != null)
                throw new System.InvalidOperationException("There is a transaction already opened.");
            
            _activeConnection = _connectionFactory.GetConnection();
            _activeTransaction = _activeConnection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if(_activeConnection == null)
                throw new System.InvalidOperationException("There is no transaction opened.");

            _activeTransaction.Commit();

            _activeTransaction.Dispose();
            _activeTransaction = null;
            _activeConnection.Dispose();
            _activeConnection = null;
        }

        public void RollbackTransaction()
        {
            if(_activeConnection == null)
                throw new System.InvalidOperationException("There is no transaction opened.");

            _activeTransaction.Rollback();
            
            _activeTransaction.Dispose();
            _activeTransaction = null;
            _activeConnection.Dispose();
            _activeConnection = null;
        }


        public void Dispose()
        {
            if(_activeTransaction != null)
            {
                _activeTransaction.Rollback();
                _activeTransaction.Dispose();
                _activeTransaction = null;
            }
            if(_activeConnection != null)
            {
                _activeConnection.Dispose();
                _activeConnection = null;
            }
        }
    }
}