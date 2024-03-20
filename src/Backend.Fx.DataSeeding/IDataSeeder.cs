using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Fx.DataSeeding;

public interface IDataSeeder
{
    IEnumerable<Type> DependsOn { get; }

    Task SeedAsync(CancellationToken cancellationToken = default);
}