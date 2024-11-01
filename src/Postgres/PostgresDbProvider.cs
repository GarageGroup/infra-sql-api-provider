using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PrimeFuncPack;

namespace GarageGroup.Infra;

public static class PostgresDbProvider
{
    public static Dependency<IDbProvider<NpgsqlConnection>> Configure(Func<IServiceProvider, PostgresDbProviderOption> optionResolver)
    {
        ArgumentNullException.ThrowIfNull(optionResolver);
        return Dependency.From<IDbProvider<NpgsqlConnection>>(InnerResolveDbProvider);

        PostgresDbProviderImpl InnerResolveDbProvider(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            return new(optionResolver.Invoke(serviceProvider));
        }
    }

    public static Dependency<IDbProvider<NpgsqlConnection>> Configure(string connectionStringName)
    {
        return Dependency.From<IDbProvider<NpgsqlConnection>>(InnerResolveDbProvider);

        PostgresDbProviderImpl InnerResolveDbProvider(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var connectionString = serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString(connectionStringName ?? string.Empty);

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException($"Connection string '{connectionStringName}' must be specified");
            }

            return new(
                option: new(connectionString));
        }
    }
}