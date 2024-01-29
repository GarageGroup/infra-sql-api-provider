using Microsoft.Data.SqlClient;

namespace GarageGroup.Infra;

partial class MicrosoftDbProviderImpl
{
    public SqlConnection GetDbConnection()
        =>
        new(connectionString)
        {
            RetryLogicProvider = retryLogicProvider
        };
}