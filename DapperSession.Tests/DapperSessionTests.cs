using System;
using System.Data;
using Moq;
using Xunit;

namespace DapperSession.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void ShouldThrowExceptionWhenTryingToOpenMoreThanOneTransactionSimultaneosly()
        {

            var transactionMock = Mock.Of<IDbTransaction>();
            var connectionMock = Mock.Of<IDbConnection>(conn => conn.BeginTransaction() == transactionMock);

            var connectionFactoryMock = Mock.Of<IConnectionFactory>(connFac => connFac.GetConnection() == connectionMock);

            var dapperSession = new DapperSession(connectionFactoryMock);

            dapperSession.BeginTransaction();
            Assert.Throws<InvalidOperationException>(() => dapperSession.BeginTransaction());
        }

        [Fact]
        public void ShouldThrowExceptionIfCommittingANonInitializedTransaction()
        {
            var connectionFactoryMock = Mock.Of<IConnectionFactory>();
            var dapperSession = new DapperSession(connectionFactoryMock);
            Assert.Throws<InvalidOperationException>(() => dapperSession.CommitTransaction());
        }

        [Fact]
        public void ShouldThrowExceptionIfRollingbackANonInitializedTransaction()
        {
            var connectionFactoryMock = Mock.Of<IConnectionFactory>();
            var dapperSession = new DapperSession(connectionFactoryMock);
            Assert.Throws<InvalidOperationException>(() => dapperSession.RollbackTransaction());
        }

        [Fact]
        public void ShouldCreateNewTransactionAfterPreviousTransactionWasCommited()
        {

            var transactionMock = Mock.Of<IDbTransaction>();
            var connectionMock = Mock.Of<IDbConnection>(conn => conn.BeginTransaction() == transactionMock);

            var connectionFactoryMock = Mock.Of<IConnectionFactory>(connFac => connFac.GetConnection() == connectionMock);

            var dapperSession = new DapperSession(connectionFactoryMock);

            dapperSession.BeginTransaction();
            dapperSession.CommitTransaction();
            dapperSession.BeginTransaction();
        }
        [Fact]
        public void ShouldCreateNewTransactionAfterPreviousTransactionWasRolledback()
        {

            var transactionMock = Mock.Of<IDbTransaction>();
            var connectionMock = Mock.Of<IDbConnection>(conn => conn.BeginTransaction() == transactionMock);

            var connectionFactoryMock = Mock.Of<IConnectionFactory>(connFac => connFac.GetConnection() == connectionMock);

            var dapperSession = new DapperSession(connectionFactoryMock);

            dapperSession.BeginTransaction();
            dapperSession.RollbackTransaction();
            dapperSession.BeginTransaction();
        }
    }
}
