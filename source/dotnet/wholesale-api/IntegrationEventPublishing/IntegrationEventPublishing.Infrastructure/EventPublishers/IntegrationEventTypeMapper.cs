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

namespace Energinet.DataHub.Wholesale.IntegrationEventPublishing.Infrastructure.EventPublishers;

public class IntegrationEventTypeMapper : IIntegrationEventTypeMapper
{
    private readonly Dictionary<Type, string> _dic;

    public IntegrationEventTypeMapper(Dictionary<Type, string> dic)
    {
        _dic = new Dictionary<Type, string>();
        foreach (var key in dic)
        {
            Add(key.Value, key.Key);
        }
    }

    /// <summary>
    /// An example of type could be the contract type: CalculationResultCompleted.
    /// </summary>
    public string GetMessageType(Type eventType)
    {
        return _dic[eventType];
    }

    private void Add(string eventName, Type eventType)
    {
        if (_dic.ContainsKey(eventType))
        {
            throw new ArgumentException("Event type already exists.");
        }

        if (_dic.ContainsValue(eventName))
        {
            throw new ArgumentException("Event name already exists.");
        }

        _dic.Add(eventType, eventName);
    }
}
