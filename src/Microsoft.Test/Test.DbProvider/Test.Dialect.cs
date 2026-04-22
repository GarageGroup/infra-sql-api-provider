using Xunit;

namespace GarageGroup.Infra.Sql.Api.Provider.Microsoft.Test;

partial class MicrosoftDbProviderTest
{
    [Fact]
    public static void Dialect_ExpectTransactSql()
    {
        var option = new MicrosoftDbProviderOption(SomeConnectionString, retryOption: null);
        var provider = MicrosoftDbProviderImpl.InternalCreate(option, tokenCredential: null);

        Assert.StrictEqual(SqlDialect.TransactSql, provider.Dialect);
    }
}