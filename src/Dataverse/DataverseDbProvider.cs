using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using PrimeFuncPack;

namespace GarageGroup.Infra;

public static class DataverseDbProvider
{
    private const string DefaultSectionName = "Dataverse";

    public static Dependency<IDbProvider<SqlConnection>> Configure(Func<IServiceProvider, DataverseDbProviderOption> optionResolver)
    {
        ArgumentNullException.ThrowIfNull(optionResolver);
        return MicrosoftDbProvider.Configure(ResolveOption);

        MicrosoftDbProviderOption ResolveOption(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            return GetMicrosoftDbProviderOption(
                option: optionResolver.Invoke(serviceProvider));
        }
    }

    public static Dependency<IDbProvider<SqlConnection>> Configure([AllowNull] string sectionName = DefaultSectionName)
    {
        return MicrosoftDbProvider.Configure(ResolveOption);

        MicrosoftDbProviderOption ResolveOption(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var configuration = serviceProvider.GetServiceOrThrow<IConfiguration>();
            return GetMicrosoftDbProviderOption(
                option: configuration.GetRequiredSection(sectionName.OrEmpty()).GetDataverseDbProviderOption());
        }
    }

    private static DataverseDbProviderOption GetDataverseDbProviderOption(this IConfigurationSection section)
        =>
        new(
            serviceUrl: section["ServiceUrl"].OrEmpty(),
            environmentId: section["EnvironmentId"],
            authClientId: section["AuthClientId"],
            authClientSecret: section["AuthClientSecret"],
            dbRetryPolicy: section.GetSqlRetryLogicOption("DbRetryPolicy"))
        {
            ConnectionTimeout = section.GetValue<int?>("ConnectionTimeout"),
            CommandTimeout = section.GetValue<int?>("CommandTimeout")
        };

    private static MicrosoftDbProviderOption GetMicrosoftDbProviderOption(DataverseDbProviderOption option)
        =>
        new(
            connectionString: BuildConnectionString(option),
            retryOption: option.DbRetryPolicy);

    private static string BuildConnectionString(DataverseDbProviderOption option)
    {
        var server = new Uri(option.ServiceUrl).Host;
        var builder = new SqlConnectionStringBuilder
        {
            ["Server"] = $"{server},5558"
        };

        if (string.IsNullOrEmpty(option.EnvironmentId) is false)
        {
            builder.InitialCatalog = option.EnvironmentId;
        }

        if ((string.IsNullOrEmpty(option.AuthClientId) is false) && (string.IsNullOrEmpty(option.AuthClientSecret) is false))
        {
            builder.Authentication = SqlAuthenticationMethod.ActiveDirectoryServicePrincipal;
            builder.UserID = option.AuthClientId;
            builder.Password = option.AuthClientSecret;
        }

        if (option.ConnectionTimeout is not null)
        {
            builder.ConnectTimeout = option.ConnectionTimeout.Value;
        }

        if (option.CommandTimeout is not null)
        {
            builder.CommandTimeout = option.CommandTimeout.Value;
        }

        return builder.ConnectionString;
    }
}