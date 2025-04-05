using MatDapperExtensions.Connection;
using MatDapperExtensions.Factory;
using MatDapperExtensions.Handler;
using MatDapperExtensions.Repositories;
using MatDapperExtensions.Service;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MatDapperExtensions;

public static class Install
{
    public static IServiceCollection AddMatDapperExtensions(
        this IServiceCollection services,
        Func<IServiceProvider, IDbConnectionFactory> dbConnectionFactory,
        Func<IServiceProvider, IRetryPolicyFactory> retryPolicyFactory,
        Func<IServiceProvider, ILogErrorHandler> logErrorHandler)
    {
        services.AddTransient(dbConnectionFactory);
        services.AddTransient(retryPolicyFactory);
        services.AddTransient<IDapperService, DapperService>();
        services.AddTransient(logErrorHandler);
        services.AddTransient<IRepository, Repository>();

        return services;
    }

    public static IServiceCollection AddMatDapperExtensions(
        this IServiceCollection services,
        Func<IServiceProvider, IDbConnectionFactory> dbConnectionFactory,
        Func<IServiceProvider, IRetryPolicyFactory> retryPolicyFactory)
    {
        services.AddTransient(dbConnectionFactory);
        services.AddTransient(retryPolicyFactory);
        services.AddTransient<IDapperService, DapperService>();
        services.AddTransient<ILogErrorHandler, LogErrorHandler>();
        services.AddTransient<IRepository, Repository>();

        return services;
    }

    public static IServiceCollection AddMatDapperExtensions(
        this IServiceCollection services,
        Func<IServiceProvider, IDbConnectionFactory> dbConnectionFactory)
    {
        services.AddTransient(dbConnectionFactory);
        services.AddTransient<IRetryPolicyFactory, RetryPolicyFactory>();
        services.AddTransient<IDapperService, DapperService>();
        services.AddTransient<ILogErrorHandler, LogErrorHandler>();
        services.AddTransient<IRepository, Repository>();

        return services;
    }

    public static IServiceCollection AddMatDapperExtensions(this IServiceCollection services, string connectionString)
    {
        services.AddTransient<IDbConnectionFactory>(x => new DbConnectionFactory(connectionString));
        services.AddTransient<IRetryPolicyFactory, RetryPolicyFactory>();
        services.AddTransient<IDapperService, DapperService>();
        services.AddTransient<ILogErrorHandler, LogErrorHandler>();
        services.AddTransient<IRepository, Repository>();

        return services;
    }
}
