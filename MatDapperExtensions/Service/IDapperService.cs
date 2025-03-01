using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MatDapperExtensions.Service;

public interface IDapperService
{
    Task<int> ExecuteAsyncWithRetry(string sql,
                                    object param = null,
                                    IDbTransaction transaction = null,
                                    int? commandTimeout = null,
                                    CommandType? commandType = null);
    Task<IEnumerable<T>> QueryAsyncWithRetry<T>(string sql,
                                                object param = null,
                                                IDbTransaction transaction = null,
                                                int? commandTimeout = null,
                                                CommandType? commandType = null);

    Task<T> QueryFirstOrDefaultAsyncWithRetry<T>(string sql,
                                                 object param = null,
                                                 IDbTransaction transaction = null,
                                                 int? commandTimeout = null,
                                                 CommandType? commandType = null);
}

