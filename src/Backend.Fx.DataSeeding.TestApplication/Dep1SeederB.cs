using System;
using System.Collections.Generic;

namespace Backend.Fx.DataSeeding.TestApplication;

public class Dep1SeederB : TestSeeder
{
    public Dep1SeederB(IList<Type> invocations) : base(invocations)
    {
        AddDependency("Backend.Fx.DataSeeding.TestApplication.RootSeeder, Backend.Fx.DataSeeding.TestApplication");
    }
    
    public override DataSeedingLevel Level => DataSeedingLevel.Demonstration;
}
