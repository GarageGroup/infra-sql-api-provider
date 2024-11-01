namespace GarageGroup.Infra;

public sealed record class PostgresDbProviderOption
{
    public PostgresDbProviderOption(string connectionString)
        =>
        ConnectionString = connectionString ?? string.Empty;

    public string ConnectionString { get; }
}