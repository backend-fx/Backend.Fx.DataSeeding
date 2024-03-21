using System;
using System.Collections.Generic;

namespace Backend.Fx.DataSeeding.TestApplication;

public class Dep2Seeder : TestSeeder
{
    public Dep2Seeder(IList<Type> invocations) : base(invocations)
    {
        AddDependency<Dep1SeederB>();
    }
}