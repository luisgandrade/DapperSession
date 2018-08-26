using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;

namespace DapperSession {
    public sealed class DapperSession : IDisposable {

        private readonly IConnectionFactory _connectionFactory;

        private Lazy<IDbConnection> _lazyConnection;

        private Lazy<IDbTransaction> _lazyTransaction;

        private bool _lazyTransactionSet;

        public DapperSession (IConnectionFactory connectionFactory) {

            _connectionFactory = connectionFactory;
            _lazyConnection = NewLazyConnection();
            _lazyTransaction = NewLazyTransaction();
            _lazyTransactionSet = true;
        }

        private Lazy<IDbConnection> NewLazyConnection()
        {
            return new Lazy<IDbConnection> (() => _connectionFactory.GetConnection ());
        }
        private Lazy<IDbTransaction> NewLazyTransaction () {
            return new Lazy<IDbTransaction> (() => {
                if (_lazyConnection.Value.State != ConnectionState.Open)
                    _lazyConnection.Value.Open ();
                return _lazyConnection.Value.BeginTransaction ();
            });
        }

        private void RenewConnectionIfNeeded()
        {
            if(_lazyTransaction.IsValueCreated)
                return;

            _lazyConnection = NewLazyConnection();
            _lazyTransaction = new Lazy<IDbTransaction>(() => null);
        }
        
        public void BeginTransaction () {
            if (_lazyTransactionSet)
                throw new System.InvalidOperationException ("There is a transaction open already.");
            _lazyTransaction = NewLazyTransaction ();
        }

        public void CommitTransaction () {
            if (!_lazyTransactionSet)
                throw new System.InvalidOperationException ("There is no transaction open.");

            _lazyTransaction.Value.Commit();
            _lazyTransaction = new Lazy<IDbTransaction>(() => null);   
            _lazyTransactionSet = false;         
        }

        public void RollbackTransaction () {
            if (!_lazyTransactionSet)
                throw new System.InvalidOperationException ("There is no transaction open.");

            _lazyTransaction.Value.Rollback();
            _lazyTransaction = new Lazy<IDbTransaction>(() => null);            
            _lazyTransactionSet = false;
        }


        public void Insert<T>(T value) 
            where T : class
        {
            var id =_lazyConnection.Value.Insert(value, transaction: _lazyTransaction.Value);
            var idProperty = value.GetType().GetProperty("Id") ?? value.GetType().GetProperty("id");
            if(idProperty != null)
                idProperty.SetValue(value, (int)id);

            RenewConnectionIfNeeded();
        }

        public async Task InsertAsync<T>(T value) 
            where T : class
        {
            var id = await _lazyConnection.Value.InsertAsync(value, transaction: _lazyTransaction.Value);
            var idProperty = value.GetType().GetProperty("Id") ?? value.GetType().GetProperty("id");
            if(idProperty != null)
                idProperty.SetValue(value, id);

            RenewConnectionIfNeeded();
        }

        public T Get<T>(int id) 
            where T : class
        {
            var entity =_lazyConnection.Value.Get<T>(id, transaction: _lazyTransaction.Value);            
            RenewConnectionIfNeeded();

            return entity;
        }

        public async Task<T> GetAsync<T>(int id) 
            where T : class
        {
            var entity = await _lazyConnection.Value.GetAsync<T>(id, transaction: _lazyTransaction.Value);            
            RenewConnectionIfNeeded();

            return entity;
        }

        public void Update<T>(T value) 
            where T : class
        {
            _lazyConnection.Value.Update(value, transaction: _lazyTransaction.Value);            
            RenewConnectionIfNeeded();

        }

        public async Task UpdateAsync<T>(T value) 
            where T : class
        {
            await _lazyConnection.Value.UpdateAsync<T>(value, transaction: _lazyTransaction.Value);            
            RenewConnectionIfNeeded();
        }

        public void Delete<T>(T value) 
            where T : class
        {
            var entity =_lazyConnection.Value.Delete(value, transaction: _lazyTransaction.Value);            
            RenewConnectionIfNeeded();

        }

        public async Task DeleteAsync<T>(T value) 
            where T : class
        {
            var entity = await _lazyConnection.Value.DeleteAsync<T>(value, transaction: _lazyTransaction.Value);            
            RenewConnectionIfNeeded();
        }

        public IEnumerable<T> Query<T>(string sql) 
            where T : class
        {
            var queryResult =_lazyConnection.Value.Query<T>(sql, transaction: _lazyTransaction.Value);            
            RenewConnectionIfNeeded();

            return queryResult;

        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql) 
            where T : class
        {
            var queryResult = await _lazyConnection.Value.QueryAsync<T>(sql, transaction: _lazyTransaction.Value);            
            RenewConnectionIfNeeded();

            return queryResult;
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected void Dispose (bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects).
                }

                if(_lazyTransaction.IsValueCreated)
                {
                    _lazyTransaction.Value.Rollback();
                    _lazyTransaction = null;                    
                }
                if(!_lazyConnection.IsValueCreated)
                    _lazyConnection.Value.Dispose();
                _lazyConnection = null;

                disposedValue = true;
            }
        }
        
        ~DapperSession() {
          // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
          Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose () {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose (true);            
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}