using Microsoft.Data.SqlClient;
using System.Data;

namespace MatDapperExtensions.Connection;
public class DbConnectionFactory(string connectionString) : IDbConnectionFactory
{
    private readonly string _connectionString = connectionString;

    public IDbConnection CreateConnection()
        => new SqlConnection(_connectionString);

}
