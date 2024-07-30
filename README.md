# Backend.Fx.DataSeeding

Generating initial data for your Backend.Fx application on boot.

## How it works

All implementations of the abstract `DataSeeder` are auto wired and executed on application boot. When the target
seeding level of the seeder matches the application's seeding level, the seeder is executed.

Every seeder is executed in a transaction. If any seeder fails, the application will be considered as not booted 
successfully and won't be functional.

During seeding, the application is switched to `SingleUserMode`, so that no online transaction processing can be done
until the seeding completes.

Dependent seeders are executed in the correct order using the topological sort algorithm.

## Getting started

1. Add a reference to the `Backend.Fx.DataSeeding` package in your domain assembly
1. Start implementing your seeders by deriving from the `DataSeeder` class. Note that seeders run on every application 
boot, so they should be idempotent.
1. Add a reference to the `Backend.Fx.DataSeeding.Feature` package in your composition assembly (that's where your
`BackendFxApplication` class lives)
1. Enable the feature in the `BackendFxApplication` class

```csharp
public class MyCoolApplication 
{
    public MyCoolApplication() : base(new SimpleInjectorCompositionRoot(), new ExceptionLogger(), GetAssemblies())
    {
        CompositionRoot.RegisterModules( ... );

        EnableFeature(new DataSeedingFeature(DataSeedingLevel.Production));
    }
}
```

## Example seeder

```csharp
public class DepartmentSeeder : DataSeeder
{
    private readonly IDepartmentRepository _departmenRepository;
    
    public CostCenterSeeder(
        IOrganizationalUnitRepository organizationalUnitRepository,
        IDepartmentRepository departmenRepository)
    {
        // we can use injection here
        _organizationalUnitRepository = organizationalUnitRepository;
        _departmenRepository = departmenRepository;
        
        // add a dependency to a seeded that needs to run before this one
        AddDependency<OrganizationalUnitSeeder>();
    }

    // this seeder should only run for development applictions
    public override DataSeedingLevel Level => DataSeedingLevel.Development;

    protected override async Task SeedDataAsync(CancellationToken cancellationToken)
    {
        // here we can do whatever is needed to generate data. Feel free to be creative.
        var organizationalUnits = await _organizationalUnitRepository.GetAllAsync(cancellationToken);
        foreach (var organizationalUnit in organizationalUnits)
        {
            for (var i = 0; i < Random.Shared.Next(10); i++)
            {
                var department = new Department($"D{i:000}", organizationalUnit.Id);
                await _departmenRepository.AddAsync(costCenter, cancellationToken);
            }
        }
    }

    protected override async Task<bool> ShouldRun(CancellationToken cancellationToken)
    {
        // every seeder needs to be idempotent. So we check if any data is already there.
        bool hasAny = await _departmenRepository.HasAnyAsync(cancellationToken);
        return hasAny == false;
    }
}
```

## Advanced topics

### Global mutexes

Data seeding cannot be executed in parallel. Therefore, the feature by default uses a `SemaphoreSlim` to ensure that 
seeding is only executed once. Consequently, this works only in a single process environment. 

When running in a distributed environment, you might want to provide a distributed mutex implementation when enabling
the feature.

```csharp
EnableFeature(new DataSeedingFeature(DataSeedingLevel.Production, new MyDistributedSeedingMutex()));
```

An excellent approach backed by different database management systems can be found in the 
[DistributedLock](https://github.com/madelson/DistributedLock) package. 
