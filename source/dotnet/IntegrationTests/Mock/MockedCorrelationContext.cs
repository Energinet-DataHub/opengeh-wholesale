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

namespace Energinet.DataHub.Wholesale.IntegrationTests.Mock;

internal sealed class MockedCorrelationContext : ICorrelationContext
{
    public string Id { get; private set; } = Guid.NewGuid().ToString();

    public string? ParentId { get; private set; } = Guid.NewGuid().ToString();

    public void SetId(string id)
    {
        Id = id;
    }

    public void SetParentId(string parentId)
    {
        ParentId = parentId;
    }

    public string AsTraceContext()
    {
        if (string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(ParentId))
        {
            return string.Empty;
        }

        return $"00-{Id}-{ParentId}-00";
    }
}
