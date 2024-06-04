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

using Microsoft.DurableTask.Client;

namespace Energinet.DataHub.Wholesale.Orchestrations.Functions.WholesaleInbox;

/// <summary>
/// Holds the current durable task client following the same pattern as HttpContexAccessor.
/// The client must be set in a function by injection a DurableTaskClient with the [DurableClient] attribute.
/// Throws an InvalidOperationException if accessed before the client is set.
/// <remarks>Must be registered in Dependency Injection as scoped</remarks>
/// </summary>
public class DurableTaskClientAccessor
{
    private DurableTaskClient? _client;

    public DurableTaskClient Current
    {
        get => _client ??
               throw new InvalidOperationException(
            "DurableTaskClient is not set, it must be set in a function by injection a DurableTaskClient with the [DurableClient] attribute");
        set => _client = value;
    }

    public void Set(DurableTaskClient client)
    {
        if (_client is not null)
            throw new InvalidOperationException("DurableTaskClient is already set");

        _client = client;
    }
}
