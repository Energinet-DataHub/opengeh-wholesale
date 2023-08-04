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

using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.Core.Messaging.Communication.Internal;

public class HostedServiceReadinessMonitor : IHostedServiceReadinessMonitor
{
    private readonly Dictionary<Type, DateTimeOffset> _timeStamps;
    private readonly TimeSpan _timeout = TimeSpan.FromMinutes(2);

    public HostedServiceReadinessMonitor()
    {
        _timeStamps = new Dictionary<Type, DateTimeOffset>();
    }

    public void Ping<TService>()
        where TService : IHostedService
    {
        _timeStamps[typeof(TService)] = DateTimeOffset.UtcNow;
    }

    public void Ping(Type hostedServiceType)
    {
        if (!typeof(IHostedService).IsAssignableFrom(hostedServiceType))
            throw new ArgumentException($"Type {hostedServiceType} must be an implementation of {nameof(IHostedService)}", nameof(hostedServiceType));

        _timeStamps[hostedServiceType] = DateTimeOffset.UtcNow;
    }

    public bool IsReady<TService>()
        where TService : IHostedService
    {
        if (!_timeStamps.TryGetValue(typeof(TService), out var timeStamp))
        {
            return false;
        }

        return timeStamp + _timeout > DateTimeOffset.UtcNow;
    }
}
