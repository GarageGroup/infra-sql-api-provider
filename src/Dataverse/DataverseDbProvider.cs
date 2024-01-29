using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using PrimeFuncPack;

namespace GarageGroup.Infra;

public static class DataverseDbProvider
{
    private const string ClientIdKey = "AZURE_CLIENT_ID";

    private const string DefaultSectionName = "Dataverse";

    public static Dependency<IDbProvider<SqlConnection>> Configure(Func<IServiceProvider, DataverseDbProviderOption> optionResolver)
    {
        ArgumentNullException.ThrowIfNull(optionResolver);
        return MicrosoftDbProvider.Configure(ResolveOption);

        MicrosoftDbProviderOption ResolveOption(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetServiceOrThrow<IConfiguration>();

            return configuration.GetMicrosoftDbProviderOption(
                option: optionResolver.Invoke(serviceProvider));
        }
    }

    public static Dependency<IDbProvider<SqlConnection>> Configure([AllowNull] string sectionName = DefaultSectionName)
    {
        return MicrosoftDbProvider.Configure(ResolveOption);

        MicrosoftDbProviderOption ResolveOption(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetServiceOrThrow<IConfiguration>();

            return configuration.GetMicrosoftDbProviderOption(
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

    private static MicrosoftDbProviderOption GetMicrosoftDbProviderOption(this IConfiguration configuration, DataverseDbProviderOption option)
        =>
        new(
            connectionString: configuration.BuildConnectionString(option),
            retryOption: option.DbRetryPolicy);

    private static string BuildConnectionString(this IConfiguration configuration, DataverseDbProviderOption option)
    {
        var connectionStringBuilder = new StringBuilder()
            .Append("Server=").Append(new Uri(option.ServiceUrl).Host).Append(",5558;");

        if (string.IsNullOrEmpty(option.EnvironmentId) is false)
        {
            connectionStringBuilder = connectionStringBuilder
                .Append("Initial Catalog=").Append(configuration[option.EnvironmentId]).Append(';');
        }

        if ((string.IsNullOrEmpty(option.AuthClientId) is false) && (string.IsNullOrEmpty(option.AuthClientSecret) is false))
        {
            connectionStringBuilder = connectionStringBuilder
                .Append("Authentication=ActiveDirectoryServicePrincipal;")
                .Append("User ID=").Append(option.AuthClientId).Append(';')
                .Append("Password=").Append(option.AuthClientSecret).Append(';');
        }
        else
        {
            var clientId = configuration[ClientIdKey];
            if (string.IsNullOrEmpty(clientId) is false)
            {
                connectionStringBuilder = connectionStringBuilder
                    .Append("Authentication=ActiveDirectoryManagedIdentity;")
                    .Append("User ID=").Append(clientId).Append(';');
            }
            else
            {
                connectionStringBuilder = connectionStringBuilder
                    .Append("Authentication=ActiveDirectoryDefault;");
            }
        }

        if (option.ConnectionTimeout is not null)
        {
            connectionStringBuilder = connectionStringBuilder
                .Append("Connection Timeout=").Append(option.ConnectionTimeout.Value).Append(';');
        }

        if (option.CommandTimeout is not null)
        {
            connectionStringBuilder = connectionStringBuilder
                .Append("Command Timeout=").Append(option.CommandTimeout.Value).Append(';');
        }

        return connectionStringBuilder.ToString();
    }
}