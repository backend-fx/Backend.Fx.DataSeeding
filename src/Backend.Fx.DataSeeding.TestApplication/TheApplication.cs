using System;
using System.Collections.Generic;
using Backend.Fx.Execution;
using Backend.Fx.Execution.SimpleInjector;
using Backend.Fx.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.DataSeeding.TestApplication;


public class TheApplication : BackendFxApplication
{
    private readonly List<Type> _invocations = new();

    public Type[] GetInvocations() => _invocations.ToArray();

    public TheApplication() : base(
        new SimpleInjectorCompositionRoot(),
        new DebugExceptionLogger(),
        typeof(TheApplication).Assembly)
    {
        CompositionRoot.Register(ServiceDescriptor.Singleton<IList<Type>>(_invocations));
    }
}