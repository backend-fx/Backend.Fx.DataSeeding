using System;
using System.Collections.Generic;

namespace Backend.Fx.DataSeeding.TestApplication;

public class Dep1SeederA : TestSeeder
{
    public Dep1SeederA(IList<Type> invocations) : base(invocations)
    {
        AddDependency<RootSeeder>();
    }
}