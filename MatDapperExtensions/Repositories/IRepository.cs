using ResultPattern;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MatDapperExtensions.Repositories;

public interface IRepository
{
    Task<Result<Guid>> AddAsync<T>(string procedureName, T data, bool dataJson = true);
    Task<Result<bool>> AddAsync<T>(string procedureName, T data, object paramOut = null);
    Task<Result<T>> GetAsync<T>(string procedureName, Guid publicId);
    Task<Result<T>> GetAsync<T>(string procedureName, object param = null);
    Task<Result<List<T>>> GetAllAsync<T>(string procedureName);
    Task<Result<List<T>>> GetAllAsync<T>(string procedureName, object param = null);
    Task<PaginationResult<dynamic>> GetAllAsync<T>(string procedureName, QueryOptions queryOptions);
    Task<Result<bool>> UpdateAsync<T>(string procedureName, Guid publicId, T data);
    Task<Result<bool>> DeleteAsync(string procedureName, Guid publicId);
    Task<Result<bool>> DeleteAsync(string procedureName, object param = null);
}
