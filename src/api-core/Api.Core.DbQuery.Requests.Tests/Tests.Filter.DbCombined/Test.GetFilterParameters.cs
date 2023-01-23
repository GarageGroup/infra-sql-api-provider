using System;
using Xunit;

namespace GGroupp.Infra.Sql.Api.Core.Tests;

partial class DbCombinedFilterTest
{
    [Theory]
    [MemberData(nameof(GetFilterParametersTestData))]
    public static void GetFilterParameters_ExpectCorrectParameters(DbCombinedFilter source, FlatArray<DbParameter> expected)
    {
        var actual = source.GetFilterParameters();
        Assert.Equal(expected, actual);
    }
}