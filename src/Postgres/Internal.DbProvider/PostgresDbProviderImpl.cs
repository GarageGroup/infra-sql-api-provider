using Npgsql;

namespace GarageGroup.Infra;

internal sealed partial class PostgresDbProviderImpl : IDbProvider<NpgsqlConnection>
{
    private readonly PostgresDbProviderOption option;

    internal PostgresDbProviderImpl(PostgresDbProviderOption option)
        =>
        this.option = option;

    public SqlDialect Dialect { get; } = SqlDialect.PostgreSql;
}