using System;
using Xunit;

namespace DapperSession.Tests
{
    public class DapperSessionIntegrationTests : IDisposable
    {

        private SQLiteConnecionFactory _sqliteConnecionFactory;

        public DapperSessionIntegrationTests()
        {
            _sqliteConnecionFactory = new SQLiteConnecionFactory();
            _sqliteConnecionFactory.SetupDatabase();
        }

        [Fact]
        public void CommandsRanInsideTransactionThatWasRolledbackShouldNotAlterDatabase()
        {
            var dapperSession = new DapperSession(_sqliteConnecionFactory);
            dapperSession.Delete(new Person() { Id = 1, Name = "John" });
            dapperSession.RollbackTransaction();
            var user = dapperSession.Get<Person>(1);

            Assert.NotNull(user);
            Assert.Equal(1, user.Id);
        }

        [Fact]
        public void CommandsRanInsideTransactionThatWasCommittedShouldAlterDatabase()
        {
            var dapperSession = new DapperSession(_sqliteConnecionFactory);
            dapperSession.Delete(new Person() { Id = 1, Name = "John" });
            dapperSession.CommitTransaction();
            var user = dapperSession.Get<Person>(1);

            Assert.Null(user);
        }

        [Fact]
        public void AttemptToOpenMoreThanOneTransactionSimultaneoulyShouldThrowAnException()
        {
            var dapperSession = new DapperSession(_sqliteConnecionFactory);
            
            Assert.Throws<InvalidOperationException>(() => dapperSession.BeginTransaction());
        }

        [Fact]
        public void AttemptToCommitAClosedTransactionShouldThrowAnException()
        {
            var dapperSession = new DapperSession(_sqliteConnecionFactory);
            dapperSession.CommitTransaction();
            Assert.Throws<InvalidOperationException>(() => dapperSession.CommitTransaction());
        }

        [Fact]
        public void AttemptToRollbackAClosedTransactionShouldThrowAnException()
        {
            var dapperSession = new DapperSession(_sqliteConnecionFactory);
            dapperSession.CommitTransaction();
            Assert.Throws<InvalidOperationException>(() => dapperSession.CommitTransaction());
        }

        [Fact]
        public void CommandRanOutsideTransactionShouldUpdateDatabaseImmediatelly()
        {
            var dapperSession = new DapperSession(_sqliteConnecionFactory);
            dapperSession.CommitTransaction();
            dapperSession.Update(new Person() { Id = 1, Name = "Luis" });
            var person = dapperSession.Get<Person>(1);

            Assert.NotNull(person);
            Assert.Equal("Luis", person.Name);
        }

        [Fact]
        public void InsertShouldUpdateIdOfInsertedEntity()
        {
            var dapperSession = new DapperSession(_sqliteConnecionFactory);
            dapperSession.CommitTransaction();
            var person = new Person() { Id = 0, Name = "Luis" };

            dapperSession.Insert(person);

            Assert.Equal(4, person.Id);
        }



        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                _sqliteConnecionFactory.TearDown();

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DapperSessionIntegrationTests() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}