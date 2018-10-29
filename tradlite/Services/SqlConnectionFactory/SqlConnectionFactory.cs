using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Tradlite.Services.SqlConnectionFactory
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        public string _connectionString;

        public SqlConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }


        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
