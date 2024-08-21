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

using System.Diagnostics;
using Xunit.Abstractions;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests;

/// <summary>
/// Logs the elapsed time from the constructor was called until the instance is disposed.
/// </summary>
public sealed class PerformanceLogger : IDisposable
{
    private readonly ITestOutputHelper? _logger;
    private readonly string _message;
    private readonly Stopwatch _stopwatch;

    public PerformanceLogger(ITestOutputHelper? logger, string message, bool runOnStart = true)
    {
        _logger = logger;
        _message = message;
        _stopwatch = new Stopwatch();

        if (runOnStart)
            _stopwatch.Start();
    }

    public void Start() => _stopwatch.Start();

    public void Dispose()
    {
        _stopwatch.Stop();
        var elapsed = _stopwatch.Elapsed;
        _logger?.WriteLine("[PERFORMANCE][{0:F2}s] {1} took {0:N} seconds", elapsed.TotalSeconds, _message);
    }
}
