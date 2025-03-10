using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Data.SqlClient;

namespace GarageGroup.Infra;

partial class MicrosoftDbProviderImpl
{
    public SqlConnection GetDbConnection()
    {
        return new(connectionString)
        {
            RetryLogicProvider = retryLogicProvider,
            AccessTokenCallback = tokenCredential is null ? null : AuthroizeAsync
        };

        async Task<SqlAuthenticationToken> AuthroizeAsync(SqlAuthenticationParameters parameters, CancellationToken cancellationToken)
        {
            var context = new TokenRequestContext(
                scopes:
                [
                    BuildScopeUrl(parameters)
                ]);

            var accessToken = await tokenCredential.GetTokenAsync(context, cancellationToken: cancellationToken).ConfigureAwait(false);
            return new(accessToken.Token, accessToken.ExpiresOn);
        }
    }

    private static string BuildScopeUrl(SqlAuthenticationParameters parameters)
    {
        var resourceUri = new Uri(parameters.Resource);
        return new Uri(resourceUri, ScopeRelativeUri).ToString();
    }
}