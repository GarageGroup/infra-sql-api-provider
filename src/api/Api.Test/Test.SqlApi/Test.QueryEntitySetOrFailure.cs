using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using PrimeFuncPack.UnitTest;
using Xunit;

namespace GarageGroup.Infra.Sql.Api.Provider.Api.Test;

partial class SqlApiTest
{
    [Fact]
    public static async Task QueryEntitySetOrFailureAsync_QueryIsNull_ExpectArgumentNullException()
    {
        using var dbDataReader = CreateDbDataReader(3, SomeFieldNames);
        using var dbCommand = CreateDbCommand(dbDataReader);

        var mockDbConnection = CreateMockDbConnection(dbCommand);
        using var dbConnection = new StubDbConnection(mockDbConnection.Object);

        var dbProvider = CreateDbProvider(dbConnection);

        var sqlApi = new SqlApi(dbProvider);
        var cancellationToken = new CancellationToken(canceled: false);

        var ex = await Assert.ThrowsAsync<ArgumentNullException>(TestAsync);
        Assert.Equal("query", ex.ParamName);

        async Task TestAsync()
            =>
            _ = await sqlApi.QueryEntitySetOrFailureAsync<StubDbEntity>(null!, cancellationToken);
    }

    [Fact]
    public static void QueryEntitySetOrFailureAsync_CancellationTokenIsCanceled_ExpectCanceledValueTask()
    {
        using var dbDataReader = CreateDbDataReader(3, SomeFieldNames);
        using var dbCommand = CreateDbCommand(dbDataReader);

        var mockDbConnection = CreateMockDbConnection(dbCommand);
        using var dbConnection = new StubDbConnection(mockDbConnection.Object);

        var dbProvider = CreateDbProvider(dbConnection);

        var sqlApi = new SqlApi(dbProvider);
        var cancellationToken = new CancellationToken(canceled: true);

        var actual = sqlApi.QueryEntitySetOrFailureAsync<StubDbEntity>(SomeDbQuery, cancellationToken);
        Assert.True(actual.IsCanceled);
    }

    [Fact]
    public static async Task QueryEntitySetOrFailureAsync_CancellationTokenIsNotCanceled_ExpectConnectionOpenCalledOnce()
    {
        using var dbDataReader = CreateDbDataReader(3, SomeFieldNames);
        using var dbCommand = CreateDbCommand(dbDataReader);

        var mockDbConnection = CreateMockDbConnection(dbCommand);
        using var dbConnection = new StubDbConnection(mockDbConnection.Object);

        var dbProvider = CreateDbProvider(dbConnection);
        var sqlApi = new SqlApi(dbProvider);

        _ = await sqlApi.QueryEntitySetOrFailureAsync<StubDbEntity>(SomeDbQuery, default);
        mockDbConnection.Verify(static db => db.Open(), Times.Once);
    }

    [Fact]
    public static async Task QueryEntitySetOrFailureAsync_ConnectionThrowsException_ExpectFailure()
    {
        var dbConnectionException = new StubException("Some error message");

        var mockDbConnection = CreateMockDbConnection(dbConnectionException);
        using var dbConnection = new StubDbConnection(mockDbConnection.Object);

        var dbProvider = CreateDbProvider(dbConnection);
        var sqlApi = new SqlApi(dbProvider);

        var actual = await sqlApi.QueryEntitySetOrFailureAsync<StubDbEntity>(SomeDbQuery, default);
        var expected = Failure.Create("An unexpected exception was thrown when executing the input query", dbConnectionException);

        Assert.StrictEqual(expected, actual);
    }

    [Theory]
    [InlineData(TestData.EmptyString)]
    [InlineData(TestData.SomeString)]
    public static async Task QueryEntitySetOrFailureAsync_ConnectionDoesNotThrowException_ExpectCommandTextIsSqlQuery(
        string sqlQuery)
    {
        using var dbDataReader = CreateDbDataReader(3, "Param01", "Param02");
        using var dbCommand = CreateDbCommand(dbDataReader);

        var mockDbConnection = CreateMockDbConnection(dbCommand);
        using var dbConnection = new StubDbConnection(mockDbConnection.Object);

        var dbProvider = CreateDbProvider(dbConnection);
        var sqlApi = new SqlApi(dbProvider);

        var dbQuery = new StubDbQuery(
            query: sqlQuery,
            parameters: new DbParameter[]
            {
                new("Param01", null),
                new("Param03", TestData.PlusFifteenIdRefType)
            });

        _ = await sqlApi.QueryEntitySetOrFailureAsync<StubDbEntity>(dbQuery, default);
        Assert.Equal(sqlQuery, dbCommand.CommandText);
    }

    [Fact]
    public static async Task QueryEntitySetOrFailureAsync_ConnectionDoesNotThrowException_ExpectCommandParametersAreDistinct()
    {
        using var dbDataReader = CreateDbDataReader(3, "Field01", "Field02");
        using var dbCommand = CreateDbCommand(dbDataReader);

        var mockDbConnection = CreateMockDbConnection(dbCommand);
        using var dbConnection = new StubDbConnection(mockDbConnection.Object);

        var parameters = new Dictionary<DbParameter, object>
        {
            [new("Param01", null)] = TestData.MinusFifteenIdNullNameRecord,
            [new(string.Empty, decimal.MinusOne)] = byte.MaxValue,
            [new("Param03", long.MinValue)] = true,
            [new("Param04", TestData.NullTextStructType)] = TestData.SomeString,
            [new("Param03", TestData.AnotherString)] = TestData.WhiteSpaceString
        };

        var dbProvider = CreateDbProvider(dbConnection, parameters);
        var sqlApi = new SqlApi(dbProvider);

        var dbQuery = new StubDbQuery(
            query: "Some SQL",
            parameters: parameters.Select(GetKey).ToFlatArray());

        _ = await sqlApi.QueryEntitySetOrFailureAsync<StubDbEntity>(dbQuery, default);
        var actual = dbCommand.Parameters.GetInnerFieldValue<List<object>>("parameters") ?? new();

        var expected = new object[]
        {
            TestData.MinusFifteenIdNullNameRecord, byte.MaxValue, TestData.WhiteSpaceString, TestData.SomeString
        };

        Assert.Equal(expected, actual);

        static DbParameter GetKey(KeyValuePair<DbParameter, object> kv)
            =>
            kv.Key;
    }

    [Theory]
    [InlineData(TestData.MinusOne)]
    [InlineData(TestData.Zero)]
    [InlineData(TestData.PlusFifteen)]
    public static async Task QueryEntitySetOrFailureAsync_ConnectionDoesNotThrowExceptionAndTimeoutIsNotNull_ExpectCommandTimeoutWasConfigured(
        int timeout)
    {
        using var dbDataReader = CreateDbDataReader(3, "Field01", "Field02");
        using var dbCommand = CreateDbCommand(dbDataReader);

        var mockDbConnection = CreateMockDbConnection(dbCommand);
        using var dbConnection = new StubDbConnection(mockDbConnection.Object);

        var dbProvider = CreateDbProvider(dbConnection);
        var sqlApi = new SqlApi(dbProvider);

        var dbQuery = new StubDbQuery(
            query: "SELECT * From Product",
            parameters: default)
        {
            TimeoutInSeconds = timeout
        };

        _ = await sqlApi.QueryEntitySetOrFailureAsync<StubDbEntity>(dbQuery, default);
        Assert.Equal(timeout, dbCommand.CommandTimeout);
    }

    [Fact]
    public static async Task QueryEntitySetOrFailureAsync_CommandThrowsException_ExpectUnknownFailure()
    {
        var dbCommandException = new StubException("Some Exception Message");
        using var dbCommand = CreateDbCommand(dbCommandException);

        var mockDbConnection = CreateMockDbConnection(dbCommand);
        using var dbConnection = new StubDbConnection(mockDbConnection.Object);

        var dbProvider = CreateDbProvider(dbConnection);
        var sqlApi = new SqlApi(dbProvider);

        var actual = await sqlApi.QueryEntitySetOrFailureAsync<StubDbEntity>(SomeDbQuery, default);
        var expected = Failure.Create("An unexpected exception was thrown when executing the input query", dbCommandException);

        Assert.StrictEqual(expected, actual);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public static async Task QueryEntitySetOrFailureAsync_CommandDoesNotThrowException_ExpectSucessResultEntitySet(
        int itemsCount)
    {
        using var dbDataReader = CreateDbDataReader(itemsCount, "Field1", "Field2", "Field3");
        using var dbCommand = CreateDbCommand(dbDataReader);

        var mockDbConnection = CreateMockDbConnection(dbCommand);
        using var dbConnection = new StubDbConnection(mockDbConnection.Object);

        var dbProvider = CreateDbProvider(dbConnection);
        var sqlApi = new SqlApi(dbProvider);

        var actual = await sqlApi.QueryEntitySetOrFailureAsync<StubDbEntity>(SomeDbQuery, default);

        var expectedFieldIndexes = new Dictionary<string, int>
        {
            ["Field1"] = 0,
            ["Field2"] = 1,
            ["Field3"] = 2
        };
        var expectedEntity = new StubDbEntity(dbDataReader, expectedFieldIndexes);

        var expected = new StubDbEntity[itemsCount];
        Array.Fill(expected, expectedEntity);

        Assert.StrictEqual(expected.ToFlatArray(), actual);
    }
}