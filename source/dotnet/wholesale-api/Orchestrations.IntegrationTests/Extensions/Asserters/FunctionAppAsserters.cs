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

using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.TestCommon;
using FluentAssertions;

namespace Energinet.DataHub.Wholesale.Orchestrations.IntegrationTests.Extensions.Asserters;

// TODO: Remove this, when test common has been updated! MWO, RFG
public static class FunctionAppAsserters
{
    /// <summary>
    /// Assert that the <paramref name="functionName"/> was executed by searching the log.
    /// </summary>
    /// <param name="hostManager"></param>
    /// <param name="functionName">For some azure function tool versions, this name must contain 'Functions.' as a prefix for the actual function name.</param>
    /// <param name="waitTimeSpan">Time to wait for function to be exectued. If not specified then default is set to 30 seconds.</param>
    public static async Task AssertFunctionWasExecutedAsync(this FunctionAppHostManager hostManager, string functionName, TimeSpan waitTimeSpan = default)
    {
        ArgumentNullException.ThrowIfNull(hostManager);
        ArgumentException.ThrowIfNullOrWhiteSpace(functionName);

        if (waitTimeSpan == default)
        {
            waitTimeSpan = TimeSpan.FromSeconds(30);
        }

        var functionExecuted = await Awaiter
            .TryWaitUntilConditionAsync(
                () => hostManager.CheckIfFunctionWasExecuted(
                    $"Functions.{functionName}"),
                waitTimeSpan)
            .ConfigureAwait(false);

        functionExecuted.Should().BeTrue($"'{functionName}' was expected to run.");
    }
}
