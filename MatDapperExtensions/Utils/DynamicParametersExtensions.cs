using Dapper;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using System.Text.Json;

namespace MatDapperExtensions.Utils;
public static class DynamicParametersExtensions
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    public static DynamicParameters CreateParamAsDataJson<T>(this T data, bool addErrorStatusMessage = true)
        => new DynamicParameters()
            .AddJsonData(data)
            .AddOptionalOutputs(addErrorStatusMessage);


    public static DynamicParameters CreateParam<T>(this T data, bool addErrorStatusMessage = true)
        => data
            .ToDynamicParameters()
            .AddOptionalOutputs(addErrorStatusMessage);

    public static DynamicParameters ToDynamicParameters<T>(this T data)
    {
        var param = new DynamicParameters();

        var properties = PropertyCache.GetOrAdd(typeof(T), t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        foreach (var property in properties)
            param.Add(property.Name, property.GetValue(data));

        return param;
    }

    public static DynamicParameters AddJsonData<T>(this DynamicParameters param, T data)
    {
        param.Add("@Data", JsonSerializer.Serialize(data));

        return param;
    }

    public static DynamicParameters AddOutPublicId(this DynamicParameters param)
    {
        param.Add("@PublicId", dbType: DbType.Guid, direction: ParameterDirection.Output);

        return param;
    }

    public static DynamicParameters AddPublicId(this DynamicParameters param, Guid publicId)
    {
        param.Add("@PublicId", publicId);

        return param;
    }

    public static DynamicParameters AddErrorStatusMessage(this DynamicParameters param)
    {
        param.Add("@Status", dbType: DbType.Boolean, direction: ParameterDirection.Output);
        param.Add("@ErrorMessage", dbType: DbType.String, size: 200, direction: ParameterDirection.Output);
        param.Add("@ErrorNumber", dbType: DbType.Int32, direction: ParameterDirection.Output);

        return param;
    }

    public static DynamicParameters AddParams(this DynamicParameters param, object paramToAdd)
    {
        param.AddDynamicParams(paramToAdd);

        return param;
    }

    private static DynamicParameters AddOptionalOutputs(this DynamicParameters param, bool addErrorStatusMessage)
    {
        if (addErrorStatusMessage)
            param.AddErrorStatusMessage();

        return param;
    }
}
