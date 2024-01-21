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

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace FunctionApp.Orchestrations.Functions
{
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
    internal static class HelloSequence
    {
        [Function(nameof(HelloCities))]
        public static async Task<string> HelloCities([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var result = string.Empty;

            result += await context.CallActivityAsync<string>(nameof(SayHello), "Tokyo") + " ";
            result += await context.CallActivityAsync<string>(nameof(SayHello), "London") + " ";
            result += await context.CallActivityAsync<string>(nameof(SayHello), "Seattle");
            return result;
        }

        [Function(nameof(SayHello))]
        public static string SayHello([ActivityTrigger] string cityName, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger(nameof(SayHello));
            logger.LogInformation("Saying hello to {name}", cityName);
            return $"Hello, {cityName}!";
        }

        [Function(nameof(StartHelloCities))]
        public static async Task<HttpResponseData> StartHelloCities(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger(nameof(StartHelloCities));

            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(HelloCities));
            logger.LogInformation("Created new orchestration with instance ID = {instanceId}", instanceId);

            return client.CreateCheckStatusResponse(req, instanceId);
        }
    }
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
}
