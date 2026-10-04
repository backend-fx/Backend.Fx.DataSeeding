namespace Backend.Fx.DataSeeding;

public interface IDataSeeder
{
    IEnumerable<Type> DependsOn { get; }

    Task SeedAsync(CancellationToken cancellationToken = default);

    DataSeedingLevel Level { get; }
}