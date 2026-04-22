using Xunit;

namespace GarageGroup.Infra.Sql.Api.Provider.Microsoft.Test;

partial class MicrosoftDbProviderTest
{
    [Fact]
    public static void GetDbConnection_RetryOptionIsNull_ExpectConnectionCreated()
    {
        var option = new MicrosoftDbProviderOption(SomeConnectionString, retryOption: null);
        var provider = MicrosoftDbProviderImpl.InternalCreate(option, tokenCredential: null);

        using var dbConnection = provider.GetDbConnection();
        Assert.Equal(SomeConnectionString, dbConnection.ConnectionString);
    }

    [Fact]
    public static void GetDbConnection_RetryOptionIsSpecified_ExpectRetryProviderIsNotNull()
    {
        var option = new MicrosoftDbProviderOption(SomeConnectionString, CreateRetryOption());
        var provider = MicrosoftDbProviderImpl.InternalCreate(option, tokenCredential: null);

        using var dbConnection = provider.GetDbConnection();
        Assert.NotNull(dbConnection.RetryLogicProvider);
    }

    [Fact]
    public static void GetDbConnection_SameConnectionString_ExpectSameRetryProviderInstance()
    {
        var retryOption = CreateRetryOption();
        var option = new MicrosoftDbProviderOption(SomeConnectionString, retryOption);

        var providerOne = MicrosoftDbProviderImpl.InternalCreate(option, tokenCredential: null);
        using var dbConnectionOne = providerOne.GetDbConnection();

        var providerTwo = MicrosoftDbProviderImpl.InternalCreate(option, tokenCredential: null);
        using var dbConnectionTwo = providerTwo.GetDbConnection();

        Assert.Same(dbConnectionOne.RetryLogicProvider, dbConnectionTwo.RetryLogicProvider);
    }

    [Fact]
    public static void GetDbConnection_TokenCredentialIsNull_ExpectAccessTokenCallbackIsNull()
    {
        var option = new MicrosoftDbProviderOption(SomeConnectionString, retryOption: null);
        var provider = MicrosoftDbProviderImpl.InternalCreate(option, tokenCredential: null);

        using var dbConnection = provider.GetDbConnection();
        Assert.Null(dbConnection.AccessTokenCallback);
    }

    [Fact]
    public static void GetDbConnection_TokenCredentialIsSpecified_ExpectAccessTokenCallbackIsNotNull()
    {
        var option = new MicrosoftDbProviderOption(SomeConnectionString, retryOption: null);
        var provider = MicrosoftDbProviderImpl.InternalCreate(option, tokenCredential: new StubTokenCredential());

        using var dbConnection = provider.GetDbConnection();
        Assert.NotNull(dbConnection.AccessTokenCallback);
    }
}
