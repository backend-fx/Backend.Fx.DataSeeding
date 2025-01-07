using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Execution;
using Backend.Fx.Execution.Pipeline;

namespace Backend.Fx.DataSeeding.Feature;

public class DefaultDataSeedingContext : DataSeedingContext
{
    private readonly DataSeedingLevel _level;

    public DefaultDataSeedingContext(DataSeedingLevel level)
    {
        _level = level;
    }

    protected override async Task RunSeederInSeparateInvocationAsync(
        IBackendFxApplication application,
        Type seederType,
        CancellationToken cancellationToken)
    {
        await application.Invoker.InvokeAsync(
            async (sp, ct) => await InvokeSeeder(seederType, sp, _level, ct),
            new SystemIdentity(),
            cancellationToken);
    }
}