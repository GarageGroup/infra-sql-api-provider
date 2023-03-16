using System;
using System.Collections.Generic;

namespace GGroupp.Infra.Sql.Api.Core.Test;

public static partial class DbRawFilterTest
{
    public static IEnumerable<object[]> GetFilterSqlQueryTestData()
        =>
        new[]
        {
            new object[]
            {
                new DbRawFilter(null!),
                string.Empty
            },
            new object[]
            {
                new DbRawFilter("SELECT * FROM Product"),
                "SELECT * FROM Product"
            },
            new object[]
            {
                new DbRawFilter("SELECT * FROM Product")
                {
                    Parameters = new DbParameter[]
                    {
                        new("Param1", "Some value")
                    }
                },
                "SELECT * FROM Product"
            }
        };

    public static IEnumerable<object[]> GetFilterParametersTestData()
        =>
        new[]
        {
            new object[]
            {
                new DbRawFilter(null!),
                default(FlatArray<DbParameter>)
            },
            new object[]
            {
                new DbRawFilter("SELECT * FROM Product"),
                default(FlatArray<DbParameter>)
            },
            new object[]
            {
                new DbRawFilter("SELECT * FROM Product")
                {
                    Parameters = new DbParameter[]
                    {
                        new("Param1", "Some value"),
                        new("Param2", 27)
                    }
                },
                new FlatArray<DbParameter>(
                    new("Param1", "Some value"),
                    new("Param2", 27))
            }
        };
}