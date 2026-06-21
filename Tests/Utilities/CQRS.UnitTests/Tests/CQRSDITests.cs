using CQRS.Core;
using CQRS.Core.Contracts;
using CQRS.Core.Services;
using CQRS.UnitTests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace CQRS.UnitTests.Tests;

public class CQRSDITests
{
    [Fact]
    public void AddCQRSSupport_RegistersInfrastructureServices()
    {
        ServiceCollection services = new();

        services.AddCQRSSupport(typeof(CQRSDITests));

        services.ShouldContain(d =>
            d.ServiceType == typeof(IBackgroundDomainEventQueue) &&
            d.Lifetime == ServiceLifetime.Singleton);
        services.ShouldContain(d =>
            d.ServiceType == typeof(IDomainEventsDispatcher) &&
            d.Lifetime == ServiceLifetime.Transient);
        services.ShouldContain(d => d.ServiceType == typeof(IHostedService));
    }

    [Fact]
    public void AddCQRSSupport_DiscoversDomainEventHandlers_FromAssemblyPointer()
    {
        ServiceCollection services = new();

        services.AddCQRSSupport(typeof(CQRSDITests));

        services.ShouldContain(d => d.ServiceType == typeof(IDomainEventManager<TestDomainEvent>));
    }

    [Fact]
    public void AddCQRSSupport_InvokesConfigureDecorators()
    {
        ServiceCollection services = new();
        bool invoked = false;

        services.AddCQRSSupport(typeof(CQRSDITests), _ => invoked = true);

        invoked.ShouldBeTrue();
    }

    [Fact]
    public void TryDecorateIfImplementationsExist_AppliesDecorator_WhenImplementationRegistered()
    {
        ServiceCollection services = new();
        services.AddScoped<IDecoratable<string>, ConcreteDecoratable>();

        services.TryDecorateIfImplementationsExist(typeof(IDecoratable<>), typeof(DecoratableDecorator<>));

        using ServiceProvider provider = services.BuildServiceProvider();
        IDecoratable<string> resolved = provider.GetRequiredService<IDecoratable<string>>();

        resolved.ShouldBeOfType<DecoratableDecorator<string>>();
        resolved.Describe().ShouldBe("decorated(concrete)");
    }

    [Fact]
    public void TryDecorateIfImplementationsExist_IsNoOp_WhenNoImplementationRegistered()
    {
        ServiceCollection services = new();

        Should.NotThrow(() =>
            services.TryDecorateIfImplementationsExist(typeof(IDecoratable<>), typeof(DecoratableDecorator<>)));

        services.ShouldNotContain(d => d.ServiceType == typeof(IDecoratable<>));
    }
}
