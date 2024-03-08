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

using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.Events.IntegrationTests.Infrastructure.WholesaleInboxRequests;

public class TestLogger<T> : ILogger<T>
{
    private readonly ILogger<T> _baseLogger;
    private readonly ITestOutputHelper _testOutput;

    public TestLogger(ILogger<T> baseLogger, ITestOutputHelper testOutput)
    {
        _baseLogger = baseLogger;
        _testOutput = testOutput;
    }

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return _baseLogger.BeginScope(state);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        try
        {
            _testOutput.WriteLine("Test log message: [{0}] {1}", logLevel.ToString(), formatter(state, exception));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error logging to test output. Exception: {e}{Environment.NewLine}Original log message: [{logLevel}] {formatter(state, exception)}");
        }

        _baseLogger.Log(logLevel, eventId, state, exception, formatter);
    }
}
