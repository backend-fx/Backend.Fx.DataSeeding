using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.DataSeeding.TestApplication;

public class RootSeeder(IList<Type> invocations) : TestSeeder(invocations)
{
    public override DataSeedingLevel Level => DataSeedingLevel.Production;

    protected override async Task SeedDataAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);
        await base.SeedDataAsync(cancellationToken);
    }
}
