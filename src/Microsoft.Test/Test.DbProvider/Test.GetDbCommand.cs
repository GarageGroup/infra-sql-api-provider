using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Xunit;

namespace GarageGroup.Infra.Sql.Api.Provider.Microsoft.Test;

partial class MicrosoftDbProviderTest
{
    [Fact]
    public static void GetDbCommand_DbConnectionIsNull_ExpectArgumentNullException()
    {
        var option = new MicrosoftDbProviderOption(SomeConnectionString, retryOption: null);
        var provider = MicrosoftDbProviderImpl.InternalCreate(option, tokenCredential: null);

        _ = Assert.Throws<ArgumentNullException>(Test);

        void Test()
            =>
            _ = provider.GetDbCommand(null!, "SELECT 1", null, null);
    }

    [Fact]
    public static void GetDbCommand_CommandTextIsNull_ExpectEmptyCommandText()
    {
        var option = new MicrosoftDbProviderOption(SomeConnectionString, retryOption: null);
        var provider = MicrosoftDbProviderImpl.InternalCreate(option, tokenCredential: null);

        using var dbConnection = provider.GetDbConnection();
        using var actual = provider.GetDbCommand(dbConnection, null!, null, null);

        Assert.Equal(string.Empty, actual.CommandText);
    }

    [Fact]
    public static void GetDbCommand_TimeoutIsSpecified_ExpectCommandTimeoutFromInput()
    {
        const int timeoutInSeconds = 47;

        var option = new MicrosoftDbProviderOption(SomeConnectionString, retryOption: null);
        var provider = MicrosoftDbProviderImpl.InternalCreate(option, tokenCredential: null);

        using var dbConnection = provider.GetDbConnection();
        using var actual = provider.GetDbCommand(dbConnection, "SELECT 1", null, timeoutInSeconds);

        Assert.StrictEqual(timeoutInSeconds, actual.CommandTimeout);
    }

    [Fact]
    public static void GetDbCommand_ParametersAreSpecified_ExpectMappedSqlParameters()
    {
        var option = new MicrosoftDbProviderOption(SomeConnectionString, retryOption: null);
        var provider = MicrosoftDbProviderImpl.InternalCreate(option, tokenCredential: null);

        using var dbConnection = provider.GetDbConnection();

        IReadOnlyCollection<DbParameter> parameters =
        [
            new("First", 19),
            new("Second", null)
        ];

        using var actual = provider.GetDbCommand(dbConnection, "SELECT 1", parameters, null);

        Assert.StrictEqual(2, actual.Parameters.Count);

        var first = Assert.IsType<SqlParameter>(actual.Parameters[0]);
        Assert.Equal("First", first.ParameterName);
        Assert.StrictEqual(19, first.Value);
        Assert.False(first.IsNullable);

        var second = Assert.IsType<SqlParameter>(actual.Parameters[1]);
        Assert.Equal("Second", second.ParameterName);
        Assert.StrictEqual(DBNull.Value, second.Value);
        Assert.True(second.IsNullable);
    }
}
