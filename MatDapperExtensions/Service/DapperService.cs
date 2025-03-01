using Dapper;
using MatDapperExtensions.Connection;
using MatDapperExtensions.Factory;
using MatDapperExtensions.Handler;
using Polly.Retry;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MatDapperExtensions.Service;

public class DapperService : IDapperService
{
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogErrorHandler _logErrorHandler;

    public DapperService(ILogErrorHandler logErrorHandler, IDbConnectionFactory connectionFactory, IRetryPolicyFactory retryPolicyFactory)
    {
        _logErrorHandler = logErrorHandler;
        _connectionFactory = connectionFactory;
        _retryPolicy = retryPolicyFactory.CreateDapperRetryPolicy();
    }
    public async Task<int> ExecuteAsyncWithRetry(string sql,
                                                 object param = null,
                                                 IDbTransaction transaction = null,
                                                 int? commandTimeout = null,
                                                 CommandType? commandType = null)
        => await _retryPolicy.ExecuteAsync(async () =>
        {
            using var connection = _connectionFactory.CreateConnection();

            var result = await connection.ExecuteAsync(sql, param, transaction, commandTimeout, commandType);

            _logErrorHandler.LogError(param);

            return result;
        });

    public async Task<IEnumerable<T>> QueryAsyncWithRetry<T>(string sql,
                                                             object param = null,
                                                             IDbTransaction transaction = null,
                                                             int? commandTimeout = null,
                                                             CommandType? commandType = null)
        => await _retryPolicy.ExecuteAsync(async () =>
        {
            using var connection = _connectionFactory.CreateConnection();

            var result = await connection.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType);

            _logErrorHandler.LogError(param);

            return result;
        });

    public async Task<T> QueryFirstOrDefaultAsyncWithRetry<T>(string sql,
                                                              object param = null,
                                                              IDbTransaction transaction = null,
                                                              int? commandTimeout = null,
                                                              CommandType? commandType = null)
        => await _retryPolicy.ExecuteAsync(async () =>
        {
            using var connection = _connectionFactory.CreateConnection();

            var result = await connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction, commandTimeout, commandType);

            _logErrorHandler.LogError(param);

            return result;
        });


}
