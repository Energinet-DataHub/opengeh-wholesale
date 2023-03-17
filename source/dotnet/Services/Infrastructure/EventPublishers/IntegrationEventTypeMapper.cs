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

namespace Energinet.DataHub.Wholesale.Infrastructure.EventPublishers;

public class IntegrationEventTypeMapper : IIntegrationEventTypeMapper
{
    private readonly Dictionary<string, Type> _dic;

    public IntegrationEventTypeMapper()
    {
        _dic = new Dictionary<string, Type>();
    }

    public void Add(string eventName, Type eventType)
    {
        if (_dic.ContainsKey(eventName))
        {
            throw new ArgumentException("Event name already exists.");
        }

        if (_dic.ContainsValue(eventType))
        {
            throw new ArgumentException("Event type already exists.");
        }

        _dic.Add(eventName, eventType);
    }

    public Type GetEventType(string eventName)
    {
        return _dic[eventName];
    }

    public string GetEventName(Type eventType)
    {
        return _dic.Single(x => x.Value == eventType).Key;
    }
}
