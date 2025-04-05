using Dapper;
using MatDapperExtensions.Service;
using MatDapperExtensions.Utils;
using MatSqlFilter;
using ResultPattern;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MatDapperExtensions.Repositories;

public class Repository(IDapperService dapperRetryService)
    : IRepository
{
    public async Task<Result<Guid>> AddAsync<T>(string procedureName, T data, bool dataJson = true)
    {
        var param = (dataJson ? data.CreateParamAsDataJson() : data.CreateParam()).AddOutPublicId();

        await dapperRetryService.ExecuteAsyncWithRetry(procedureName, param, commandType: CommandType.StoredProcedure);

        var status = param.Get<bool>("@Status");

        if (!status)
            return await Result<Guid>.FailAsync(param.Get<string>("@ErrorMessage"));

        var key = param.Get<Guid>("@PublicId");

        return await Result<Guid>.SuccessAsync(key);
    }

    public async Task<Result<bool>> AddAsync<T>(string procedureName, T data, object paramOut)
    {
        var param = data.CreateParamAsDataJson().AddParams(paramOut);

        await dapperRetryService.ExecuteAsyncWithRetry(procedureName, param, commandType: CommandType.StoredProcedure);

        var status = param.Get<bool>("@Status");

        if (!status)
            return await Result<bool>.FailAsync(param.Get<string>("@ErrorMessage"));

        return await Result<bool>.SuccessAsync(true);
    }

    public async Task<Result<T>> GetAsync<T>(string procedureName, Guid publicId)
    {
        var param = new DynamicParameters().AddPublicId(publicId);

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
        var (valid, filter) = SqlFilter.Generate(queryOptions.Filter);

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

        param.AddErrorStatusMessage();

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
        var param = data.CreateParamAsDataJson().AddPublicId(publicId);

        await dapperRetryService.ExecuteAsyncWithRetry(procedureName, param, commandType: CommandType.StoredProcedure);

        var status = param.Get<bool>("@Status");

        if (!status)
            return await Result<bool>.FailAsync(param.Get<string>("@ErrorMessage"));

        return await Result<bool>.SuccessAsync(true);
    }

    public async Task<Result<bool>> DeleteAsync(string procedureName, Guid publicId)
    {
        var param = new DynamicParameters().AddPublicId(publicId);

        param.AddErrorStatusMessage();

        await dapperRetryService.ExecuteAsyncWithRetry(procedureName, param, commandType: CommandType.StoredProcedure);

        var status = param.Get<bool>("@Status");

        if (!status)
            return await Result<bool>.FailAsync(param.Get<string>("@ErrorMessage"));

        return await Result<bool>.SuccessAsync(true);
    }

    public async Task<Result<bool>> DeleteAsync(string procedureName, object param = null)
    {
        var parameters = new DynamicParameters(param).AddErrorStatusMessage();

        await dapperRetryService.ExecuteAsyncWithRetry(procedureName, parameters, commandType: CommandType.StoredProcedure);

        var status = parameters.Get<bool>("@Status");

        if (!status)
            return await Result<bool>.FailAsync(parameters.Get<string>("@ErrorMessage"));

        return await Result<bool>.SuccessAsync(true);
    }
}