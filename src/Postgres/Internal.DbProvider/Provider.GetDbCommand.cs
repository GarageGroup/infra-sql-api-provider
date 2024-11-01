using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Npgsql;

namespace GarageGroup.Infra;

partial class PostgresDbProviderImpl
{
    public DbCommand GetDbCommand(
        NpgsqlConnection dbConnection,
        string commandText,
        [AllowNull] IReadOnlyCollection<DbParameter> parameters,
        int? timeoutInSeconds)
    {
        ArgumentNullException.ThrowIfNull(dbConnection);

        var dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = commandText ?? string.Empty;

        if (timeoutInSeconds is not null)
        {
            dbCommand.CommandTimeout = timeoutInSeconds.Value;
        }

        if (parameters?.Count is not > 0)
        {
            return dbCommand;
        }

        foreach (var parameter in parameters)
        {
            dbCommand.Parameters.Add(
                value: new()
                {
                    ParameterName = parameter.Name,
                    Value = parameter.Value ?? DBNull.Value,
                    IsNullable = parameter.Value is null
                });
        }

        return dbCommand;
    }
}