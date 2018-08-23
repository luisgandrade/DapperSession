using System.Data;

namespace DapperSession
{
    public interface IConnectionFactory
    {         
         IDbConnection GetConnection();
    }
}