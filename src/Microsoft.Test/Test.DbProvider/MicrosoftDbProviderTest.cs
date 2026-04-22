using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Data.SqlClient;

namespace GarageGroup.Infra.Sql.Api.Provider.Microsoft.Test;

public static partial class MicrosoftDbProviderTest
{
    private const string SomeConnectionString
        =
        "Server=tcp:localhost,1433;Database=master;Encrypt=True;TrustServerCertificate=True;";

    private static SqlRetryLogicOption CreateRetryOption()
        =>
        new()
        {
            NumberOfTries = 3,
            DeltaTime = TimeSpan.FromMilliseconds(200),
            MinTimeInterval = TimeSpan.FromMilliseconds(100),
            MaxTimeInterval = TimeSpan.FromSeconds(2),
            TransientErrors = []
        };

    private sealed class StubTokenCredential : TokenCredential
    {
        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
            =>
            throw new NotSupportedException();

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
            =>
            ValueTask.FromResult(new AccessToken("token", DateTimeOffset.UtcNow.AddMinutes(5)));
    }
}
