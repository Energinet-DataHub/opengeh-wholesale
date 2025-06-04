using System.Diagnostics.CodeAnalysis;
using Energinet.DataHub.Core.TestCommon.Xunit.LazyFixture;
using Xunit;

namespace Energinet.DataHub.Wholesale.SubsystemTests;

/// <summary>
/// Simplify the implementation of subsystem tests for which we only want to perform the
/// test setup phase if they should actually be executed.
/// </summary>
/// <typeparam name="TLazyFixture">A xUnit fixture that inherits from <see cref="LazyFixtureBase"/>.</typeparam>
public abstract class SubsystemTestsBase<TLazyFixture> : IClassFixture<LazyFixtureFactory<TLazyFixture>>, IAsyncLifetime
    where TLazyFixture : LazyFixtureBase
{
    /// <summary>
    /// Any fixture given in the constructor is always created and initialized by xUnit, even if no test is executed
    /// in the test class. For this reason we use a factory to handle lazy initialization of the fixture, which means
    /// we only perform the initialization if we actually execute any test.
    /// </summary>
    public SubsystemTestsBase(LazyFixtureFactory<TLazyFixture> lazyFixtureFactory)
    {
        LazyFixtureFactory = lazyFixtureFactory;
    }

    /// <summary>
    /// This property only contains a value if any test is executed.
    /// </summary>
    [NotNull]
    protected TLazyFixture? Fixture { get; set; }

    private LazyFixtureFactory<TLazyFixture> LazyFixtureFactory { get; }

    /// <summary>
    /// This method only gets called by xUnit if any test is executed.
    /// It gets called before each test.
    ///
    /// It is responsible for initializing the <see cref="Fixture"/>.
    /// </summary>
    public async Task InitializeAsync()
    {
        Fixture = await LazyFixtureFactory.LazyFixture;
    }

    /// <summary>
    /// This method only gets called by xUnit if any test is executed.
    /// It gets called after each test.
    /// </summary>
    public Task DisposeAsync()
    {
        // We currently don't need to clean up anything between test executions.
        return Task.CompletedTask;
    }
}
