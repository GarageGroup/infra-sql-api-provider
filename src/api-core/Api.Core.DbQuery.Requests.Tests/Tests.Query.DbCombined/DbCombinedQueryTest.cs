using System;
using System.Collections.Generic;

namespace GGroupp.Infra.Sql.Api.Core.Tests;

public static partial class DbCombinedQueryTest
{
    public static IEnumerable<object[]> GetSqlQueryTestData()
        =>
        new[]
        {
            new object[]
            {
                new DbCombinedQuery(default),
                string.Empty
            },
            new object[]
            {
                new DbCombinedQuery(
                    queries: new StubDbQuery[]
                    {
                        new("SELECT Id, Name From Country"),
                        new("INSERT INTO SomeTable (Id) VALUES (@Id);", new("Id", 1), new("Price", null))
                    }),
                "SELECT Id, Name From Country\n" +
                "INSERT INTO SomeTable (Id) VALUES (@Id);"
            }
        };

    public static IEnumerable<object[]> GetParametersTestData()
        =>
        new[]
        {
            new object[]
            {
                new DbCombinedQuery(default),
                default(FlatArray<DbParameter>)
            },
            new object[]
            {
                new DbCombinedQuery(
                    queries: new StubDbQuery[]
                    {
                        new(string.Empty, new DbParameter("SomeName", "SomeValue")),
                        new("SELECT Id, Name From Country"),
                        new("INSERT INTO SomeTable (Id) VALUES (@Id);", new("Id", 1), new("Price", null))
                    }),
                new FlatArray<DbParameter>(
                    new("SomeName", "SomeValue"),
                    new("Id", 1),
                    new("Price", null))
            },
        };
}