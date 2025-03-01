using System.Data;

namespace MatDapperExtensions.Connection;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

