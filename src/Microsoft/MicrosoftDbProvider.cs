using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Core;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using PrimeFuncPack;

namespace GarageGroup.Infra;

public static class MicrosoftDbProvider
{
    private const string RetryPolicyDefaultSectionName = "DbRetryPolicy";

    public static Dependency<IDbProvider<SqlConnection>> Configure(Func<IServiceProvider, MicrosoftDbProviderOption> optionResolver)
    {
        ArgumentNullException.ThrowIfNull(optionResolver);
        return Dependency.From<IDbProvider<SqlConnection>>(ResolveMicrosoftDbProvider);

        MicrosoftDbProviderImpl ResolveMicrosoftDbProvider(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            return InnerResolveDbProvider(serviceProvider, optionResolver.Invoke(serviceProvider));
        }
    }

    public static Dependency<IDbProvider<SqlConnection>> Configure(
        string connectionStringName, string retryPolicySectionName = RetryPolicyDefaultSectionName)
    {
        return Dependency.From<IDbProvider<SqlConnection>>(InnerResolve);

        MicrosoftDbProviderImpl InnerResolve(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var option = serviceProvider.GetServiceOrThrow<IConfiguration>().GetMicrosoftDbProviderOption(
                connectionStringName, retryPolicySectionName);

            return InnerResolveDbProvider(serviceProvider, option);
        }
    }

    public static SqlRetryLogicOption? GetSqlRetryLogicOption(this IConfiguration configuration, string sectionName)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return configuration.InnerGetSqlRetryLogicOption(sectionName ?? string.Empty);
    }

    private static MicrosoftDbProviderImpl InnerResolveDbProvider(IServiceProvider serviceProvider, MicrosoftDbProviderOption option)
        =>
        MicrosoftDbProviderImpl.InternalCreate(
            option: option,
            tokenCredential: new SqlConnectionStringBuilder(option.ConnectionString).Authentication switch
            {
                SqlAuthenticationMethod.NotSpecified => serviceProvider.GetServiceOrAbsent<TokenCredential>().OrDefault(),
                _ => null
            });

    private static MicrosoftDbProviderOption GetMicrosoftDbProviderOption(
        this IConfiguration configuration, string connectionStringName, string? retryPolicySectionName)
    {
        var connectionString = configuration.GetConnectionString(connectionStringName ?? string.Empty);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{connectionStringName}' must be specified");
        }

        var retrySectionName = string.IsNullOrEmpty(retryPolicySectionName) ? RetryPolicyDefaultSectionName : retryPolicySectionName;

        return new(
            connectionString: connectionString,
            retryOption: configuration.InnerGetSqlRetryLogicOption(retrySectionName));
    }

    private static SqlRetryLogicOption? InnerGetSqlRetryLogicOption(this IConfiguration configuration, string sectionName)
    {
        var retrySection = configuration.GetSection(sectionName);

        if (retrySection.Exists() is false)
        {
            return null;
        }

        return new()
        {
            NumberOfTries = retrySection.GetInt32(nameof(SqlRetryLogicOption.NumberOfTries)),
            DeltaTime = retrySection.GetTimeSpan(nameof(SqlRetryLogicOption.DeltaTime)),
            MinTimeInterval = retrySection.GetTimeSpan(nameof(SqlRetryLogicOption.MinTimeInterval)),
            MaxTimeInterval = retrySection.GetTimeSpan(nameof(SqlRetryLogicOption.MaxTimeInterval)),
            TransientErrors = retrySection.GetInt32Collecton(nameof(SqlRetryLogicOption.TransientErrors))
        };
    }

    private static int GetInt32(this IConfigurationSection section, string key)
    {
        var value = section[key];

        if (string.IsNullOrEmpty(value))
        {
            return default;
        }

        return int.Parse(value);
    }

    private static TimeSpan GetTimeSpan(this IConfigurationSection section, string key)
    {
        var value = section[key];

        if (string.IsNullOrEmpty(value))
        {
            return default;
        }

        return TimeSpan.Parse(value);
    }

    private static int[] GetInt32Collecton(this IConfigurationSection section, string key)
    {
        return section.GetSection(key).AsEnumerable().Select(GetValue).Where(IsNotEmpty).Select(int.Parse).ToArray();

        static string GetValue(KeyValuePair<string, string?> kv)
            =>
            kv.Value ?? string.Empty;

        static bool IsNotEmpty(string item)
            =>
            string.IsNullOrEmpty(item) is false;
    }
}