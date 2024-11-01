using Npgsql;

namespace GarageGroup.Infra;

partial class PostgresDbProviderImpl
{
    public NpgsqlConnection GetDbConnection()
        =>
        new(option.ConnectionString);
}