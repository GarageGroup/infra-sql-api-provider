using Xunit;

namespace GarageGroup.Infra.Sql.Api.Core.Test;

partial class DbCombinedFilterTest
{
    [Theory]
    [MemberData(nameof(GetFilterSqlQueryTestData))]
    public static void GetFilterSqlQuery_ExpectCorrectQuery(DbCombinedFilter source, string expected)
    {
        var actual = source.GetFilterSqlQuery();
        Assert.Equal(expected, actual);
    }
}