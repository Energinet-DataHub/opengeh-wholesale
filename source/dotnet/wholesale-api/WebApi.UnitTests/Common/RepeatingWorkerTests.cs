// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.Wholesale.Common.Workers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Energinet.DataHub.Wholesale.WebApi.UnitTests.Common;

public class RepeatingWorkerTests
{
    private const int CancellationTokenTimeOutMilliSeconds = 50;

    [Theory]
    [InlineAutoMoqData]
    public async Task WhenUnableToResolveService_FailToStart(
        ServiceCollection services,
        ILogger logger)
    {
        // Arrange
        var sut = new FooRepeatingWorker(services.BuildServiceProvider(), logger);

        // Act and assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.StartAsync(CancellationToken.None));
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task WhenServiceThrows_ContinuesRepeatingTask(
        Mock<IFooService> throwingServiceMock,
        ServiceCollection services,
        ILogger logger)
    {
        // Arrange
        throwingServiceMock.Setup(x => x.DoWork()).Throws<Exception>();

        services.AddScoped<IFooService>(_ => throwingServiceMock.Object);
        services.AddScoped<ICorrelationContext>(_ => new CorrelationContext());

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(CancellationTokenTimeOutMilliSeconds)); // Ought to be more than enough to invoke the service multiple times

        var sut = new FooRepeatingWorker(services.BuildServiceProvider(), logger);

        // Act
        try
        {
            await sut.StartAsync(cancellationTokenSource.Token);
        }
        catch (TaskCanceledException)
        {
            // Expected
        }

        // Assert: Service was invoked more than once
        throwingServiceMock.Verify(x => x.DoWork(), Times.AtLeast(2));
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task WhenServiceThrows_LogsError(
        Mock<IFooService> throwingServiceMock,
        ServiceCollection services,
        Mock<ILogger> loggerMock)
    {
        // Arrange
        throwingServiceMock.Setup(x => x.DoWork()).Throws<Exception>();

        services.AddScoped<IFooService>(_ => throwingServiceMock.Object);
        services.AddScoped<ICorrelationContext>(_ => new CorrelationContext());

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(CancellationTokenTimeOutMilliSeconds)); // Ought to be more than enough to invoke the service multiple times

        var sut = new FooRepeatingWorker(services.BuildServiceProvider(), loggerMock.Object);

        // Act
        try
        {
            await sut.StartAsync(cancellationTokenSource.Token);
        }
        catch (TaskCanceledException)
        {
            // Expected
        }

        // Assert: Error was logged - more than once
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(2));
    }

    [Theory]
    [InlineAutoMoqData]
    public async Task ContinuesRepeatingTask(
        Mock<IFooService> fooServiceMock,
        ServiceCollection services,
        Mock<ILogger> loggerMock)
    {
        // Arrange
        services.AddScoped<IFooService>(_ => fooServiceMock.Object);
        services.AddScoped<ICorrelationContext>(_ => new CorrelationContext());
        var provider = services.BuildServiceProvider();
        // ReSharper disable once UnusedVariable - force time consuming creation of proxy before starting token timeOut
        var unused = fooServiceMock.Object;

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(CancellationTokenTimeOutMilliSeconds)); // Ought to be more than enough to invoke the service multiple times

        var sut = new FooRepeatingWorker(provider, loggerMock.Object);

        // Act
        try
        {
            await sut.StartAsync(cancellationTokenSource.Token);
        }
        catch (TaskCanceledException)
        {
            // Expected
        }

        // Assert: Service was invoked more than once
        fooServiceMock.Verify(service => service.DoWork(), Times.AtLeast(2));
    }

    public interface IFooService
    {
        void DoWork();
    }

    private class FooRepeatingWorker : RepeatingWorker<IFooService>
    {
        public FooRepeatingWorker(IServiceProvider serviceProvider, ILogger logger)
            : base(serviceProvider, logger, TimeSpan.FromSeconds(0))
        {
        }

        protected override Task ExecuteAsync(IFooService instance)
        {
            instance.DoWork();
            return Task.CompletedTask;
        }
    }
}
