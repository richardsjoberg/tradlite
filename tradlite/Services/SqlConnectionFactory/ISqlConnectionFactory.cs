using System.Data;

namespace Tradlite.Services.SqlConnectionFactory
{
    public interface ISqlConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}