using System;
using System.Collections.Generic;
using GGroupp.TestType;
using PrimeFuncPack.UnitTest;
using Xunit;

namespace GGroupp.Infra.Sql.Api.Core.DbEntity.Generator.Test;

public static class DbStructTypeTest
{
    [Fact]
    public static void EntityFrom_DbItemIsNull_ExpectArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(Test);
        Assert.Equal("dbItem", ex.ParamName);

        static void Test()
            =>
            _ = DbStructTypeEntity.From(null!);
    }

    [Fact]
    public static void EntityFrom_DbItemIsNotNull_ExpectCorrectInitializedEntity()
    {
        var orThrowValues = new Dictionary<string, DbValue>
        {
            ["Id"] = StubDbValue.CreateInt16(571),
            ["CreateAt"] = StubDbValue.CreateDateTimeOffset(new(2021, 11, 05, 23, 31, 17, TimeSpan.FromHours(5))),
            ["Price"] = StubDbValue.CreateDecimal(207.5m),
            ["AddionalData"] = StubDbValue.Create(TestData.MinusFifteenIdRefType)
        };

        var orDefaultValues = new Dictionary<string, DbValue?>
        {
            ["IsActual"] = StubDbValue.CreateNullableBoolean(true),
            ["ProductDate"] = StubDbValue.CreateNullableDateOnly(new(2019, 01, 27)),
            ["c.Count"] = StubDbValue.CreateNullableInt32(861723)
        };

        var dbItem = StubDbItem.Create(orThrowValues, orDefaultValues);
        var actual = DbStructTypeEntity.From(dbItem);

        var expected = new DbStructType
        {
            Id = 571,
            IsActual = true,
            CreateAt = new(2021, 11, 05, 23, 31, 17, TimeSpan.FromHours(5)),
            ProductDate = new(2019, 01, 27),
            TotalCount = 861723,
            Price = 207.5m,
            AddionalData = TestData.MinusFifteenIdRefType
        };

        Assert.StrictEqual(expected, actual);
    }
}