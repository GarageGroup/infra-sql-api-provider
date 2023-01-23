using System;

namespace GGroupp.Infra.Sql.Api.Core.Tests;

internal sealed partial class StubDbQuery : IDbQuery
{
    private readonly string sqlQuery;

    private readonly FlatArray<DbParameter> parameters;

    public StubDbQuery(string sqlQuery, params DbParameter[] parameters)
    {
        this.sqlQuery = sqlQuery;
        this.parameters = parameters;
    }

    public string GetSqlQuery()
        =>
        sqlQuery;

    public FlatArray<DbParameter> GetParameters()
        =>
        parameters;
}