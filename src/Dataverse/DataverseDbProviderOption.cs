using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;

namespace GarageGroup.Infra;

public sealed record class DataverseDbProviderOption
{
    public DataverseDbProviderOption(
        string serviceUrl,
        string? environmentId,
        [AllowNull] string authClientId,
        [AllowNull] string authClientSecret,
        SqlRetryLogicOption? dbRetryPolicy)
    {
        ServiceUrl = serviceUrl.OrEmpty();
        EnvironmentId = environmentId.OrNullIfEmpty();
        AuthClientId = authClientId.OrNullIfEmpty();
        AuthClientSecret = authClientSecret.OrNullIfEmpty();
        DbRetryPolicy = dbRetryPolicy;
    }

    public string ServiceUrl { get; }

    public string? EnvironmentId { get; }

    public string? AuthClientId { get; }

    public string? AuthClientSecret { get; }

    public SqlRetryLogicOption? DbRetryPolicy { get; }

    public int? ConnectionTimeout { get; init; }

    public int? CommandTimeout { get; init; }
}
