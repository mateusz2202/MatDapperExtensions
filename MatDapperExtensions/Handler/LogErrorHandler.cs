using Dapper;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace MatDapperExtensions.Handler;

public class LogErrorHandler(ILogger<LogErrorHandler> logger) : ILogErrorHandler
{
    public void LogError(object param)
    {
        if (param is not DynamicParameters parameters)
            return;

        HashSet<string> requiredParameters = ["ErrorMessage", "ErrorNumber", "@Status"];

        if (!requiredParameters.IsSubsetOf(parameters.ParameterNames.ToHashSet()))
            return;

        var errorMessage = parameters.Get<string>("ErrorMessage") ?? string.Empty;
        var errorNumber = parameters.Get<int?>("ErrorNumber") ?? 0;
        var status = parameters.Get<bool>("@Status");

        if (!status)
            logger.LogError("ErrorMessage: {ErrorMessage}, ErrorNumber: {ErrorNumber}", errorMessage, errorNumber);
    }
}
