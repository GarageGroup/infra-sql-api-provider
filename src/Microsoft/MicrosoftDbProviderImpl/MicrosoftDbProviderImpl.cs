using System.Collections.Concurrent;
using Azure.Core;
using Microsoft.Data.SqlClient;

namespace GarageGroup.Infra;

internal sealed partial class MicrosoftDbProviderImpl : IDbProvider<SqlConnection>
{
    private const string ScopeRelativeUri = "/.default";

    private static readonly ConcurrentDictionary<string, SqlRetryLogicBaseProvider> RetryProviders;

    static MicrosoftDbProviderImpl()
        =>
        RetryProviders = new();

    internal static MicrosoftDbProviderImpl InternalCreate(MicrosoftDbProviderOption option, TokenCredential? tokenCredential)
    {
        if (option.RetryOption is null)
        {
            return new(option.ConnectionString, null, tokenCredential);
        }

        var retryLogicProvider = RetryProviders.GetOrAdd(option.ConnectionString, CreateRetryLogicProvider);
        return new(option.ConnectionString, retryLogicProvider, tokenCredential);

        SqlRetryLogicBaseProvider CreateRetryLogicProvider(string connectionString)
            =>
            SqlConfigurableRetryFactory.CreateExponentialRetryProvider(option.RetryOption);
    }

    private readonly string connectionString;

    private readonly SqlRetryLogicBaseProvider? retryLogicProvider;

    private readonly TokenCredential? tokenCredential;

    private MicrosoftDbProviderImpl(string connectionString, SqlRetryLogicBaseProvider? retryLogicProvider, TokenCredential? tokenCredential)
    {
        this.connectionString = connectionString;
        this.retryLogicProvider = retryLogicProvider;
        this.tokenCredential = tokenCredential;
    }
}