using Dapper;
using MatDapperExtensions.Service;
using MatSqlFilter;
using ResultPattern;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MatDapperExtensions.Repositories;

public class Repository(IDapperService dapperRetryService)
    : IRepository
{
    public async Task<Result<Guid>> AddAsync<T>(string procedureName, T data)
    {
        var param = new DynamicParameters();
        param.Add("@Data", JsonSerializer.Serialize(data));
        param.Add("@PublicId", dbType: DbType.Guid, direction: ParameterDirection.Output);
        AddCommonOutputParameters(param);

        await dapperRetryService.ExecuteAsyncWithRetry(procedureName, param, commandType: CommandType.StoredProcedure);

        var status = param.Get<bool>("@Status");

        if (!status)
            return await Result<Guid>.FailAsync(param.Get<string>("@ErrorMessage"));

        var key = param.Get<Guid>("@PublicId");

        return await Result<Guid>.SuccessAsync(key);
    }
    public async Task<Result<bool>> AddAsync<T>(string procedureName, T data, DynamicParameters paramOut)
    {
        var param = new DynamicParameters(paramOut);
        param.Add("@Data", JsonSerializer.Serialize(data));

        AddCommonOutputParameters(param);

        await dapperRetryService.ExecuteAsyncWithRetry(procedureName, param, commandType: CommandType.StoredProcedure);

        var status = param.Get<bool>("@Status");

        if (!status)
            return await Result<bool>.FailAsync(param.Get<string>("@ErrorMessage"));

        return await Result<bool>.SuccessAsync(true);
    }

    public async Task<Result<T>> GetAsync<T>(string procedureName, Guid publicId)
    {
        var param = new DynamicParameters();
        param.Add("@PublicId", publicId);

        var resultSQL = await dapperRetryService.QueryFirstOrDefaultAsyncWithRetry<T>(procedureName, param, commandType: CommandType.StoredProcedure);

        if (resultSQL is null)
            return await Result<T>.FailAsync("Not found");

        return await Result<T>.SuccessAsync(resultSQL);
    }

    public async Task<Result<T>> GetAsync<T>(string procedureName, object param = null)
    {
        var resultSQL = await dapperRetryService.QueryFirstOrDefaultAsyncWithRetry<T>(procedureName, param, commandType: CommandType.StoredProcedure);

        if (resultSQL is null)
            return await Result<T>.FailAsync("Not found");

        return await Result<T>.SuccessAsync(resultSQL);
    }

    public async Task<Result<List<T>>> GetAllAsync<T>(string procedureName)
    {
        var resultSQL = await dapperRetryService.QueryAsyncWithRetry<T>(procedureName, commandType: CommandType.StoredProcedure);

        var result = resultSQL.ToList();

        return await Result<List<T>>.SuccessAsync(result);
    }

    public async Task<PaginationResult<dynamic>> GetAllAsync<T>(string procedureName, QueryOptions queryOptions)
    {
        var (valid, filter) = SqlFilter.GenerateFiltr(queryOptions.Filter);

        if (!valid)
            return await PaginationResult<dynamic>.FailAsync(filter);

        var columns = queryOptions.Columns is null || queryOptions.Columns.Count == 0
        ? null
        : string.Join(",", queryOptions.Columns);

        var pager = queryOptions.Pager ?? new Pager();

        DynamicParameters param = new();
        param.Add("@Filter", filter, DbType.String, ParameterDirection.Input);
        param.Add("@Columns", columns, DbType.String, ParameterDirection.Input);
        param.Add("@PageNumber", pager.PageNumber, DbType.Int32, ParameterDirection.Input);
        param.Add("@PageSize", pager.PageSize, DbType.Int32, ParameterDirection.Input);
        param.Add("@TotalRows", dbType: DbType.Int32, direction: ParameterDirection.Output);

        AddCommonOutputParameters(param);

        var resultSQL = await dapperRetryService.QueryAsyncWithRetry<dynamic>(procedureName, param, commandType: CommandType.StoredProcedure);

        var status = param.Get<bool>("@Status");

        if (!status)
            return await PaginationResult<dynamic>.FailAsync(param.Get<string>("@ErrorMessage"));

        int totalRow = param.Get<int>("@TotalRows");

        return await PaginationResult<dynamic>.SuccessAsync([.. resultSQL], pager, totalRow);
    }
    public async Task<Result<List<T>>> GetAllAsync<T>(string procedureName, object param = null)
    {
        var resultSQL = await dapperRetryService.QueryAsyncWithRetry<T>(procedureName, param, commandType: CommandType.StoredProcedure);

        if (resultSQL is null)
            return await Result<List<T>>.SuccessAsync([]);

        var result = resultSQL.ToList();

        return await Result<List<T>>.SuccessAsync(result);
    }

    public async Task<Result<bool>> UpdateAsync<T>(string procedureName, Guid publicId, T data)
    {
        var param = new DynamicParameters();
        param.Add("@Data", JsonSerializer.Serialize(data));
        param.Add("@PublicId", publicId);

        AddCommonOutputParameters(param);

        await dapperRetryService.ExecuteAsyncWithRetry(procedureName, param, commandType: CommandType.StoredProcedure);

        var status = param.Get<bool>("@Status");

        if (!status)
            return await Result<bool>.FailAsync(param.Get<string>("@ErrorMessage"));

        return await Result<bool>.SuccessAsync(true);
    }

    public async Task<Result<bool>> DeleteAsync(string procedureName, Guid publicId)
    {
        var param = new DynamicParameters();
        param.Add("@PublicId", publicId);

        AddCommonOutputParameters(param);

        await dapperRetryService.ExecuteAsyncWithRetry(procedureName, param, commandType: CommandType.StoredProcedure);

        var status = param.Get<bool>("@Status");

        if (!status)
            return await Result<bool>.FailAsync(param.Get<string>("@ErrorMessage"));

        return await Result<bool>.SuccessAsync(true);
    }
    public async Task<Result<bool>> DeleteAsync(string procedureName, object param = null)
    {
        var parameters = new DynamicParameters(param);

        AddCommonOutputParameters(parameters);

        await dapperRetryService.ExecuteAsyncWithRetry(procedureName, parameters, commandType: CommandType.StoredProcedure);

        var status = parameters.Get<bool>("@Status");

        if (!status)
            return await Result<bool>.FailAsync(parameters.Get<string>("@ErrorMessage"));

        return await Result<bool>.SuccessAsync(true);
    }

    private static void AddCommonOutputParameters(DynamicParameters param)
    {
        param.Add("@Status", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        param.Add("@ErrorMessage", dbType: DbType.String, direction: ParameterDirection.Output, size: 200);
        param.Add("@ErrorNumber", dbType: DbType.Int32, direction: ParameterDirection.Output);
    }

}