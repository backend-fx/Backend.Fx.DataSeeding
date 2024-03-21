using System;
using System.Collections.Generic;

namespace Backend.Fx.DataSeeding.TestApplication;

public class RootSeeder(IList<Type> invocations) : TestSeeder(invocations);