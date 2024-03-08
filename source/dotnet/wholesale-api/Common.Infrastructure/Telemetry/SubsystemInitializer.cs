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

using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Energinet.DataHub.Wholesale.Common.Infrastructure.Telemetry;

public class SubsystemInitializer : ITelemetryInitializer
{
    private readonly string _subsystemName;

    public SubsystemInitializer(string subsystemName)
    {
        if (string.IsNullOrWhiteSpace(subsystemName))
            throw new ArgumentException("Cannot be null or whitespace.", nameof(subsystemName));

        _subsystemName = subsystemName;
    }

    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.GlobalProperties["Subsystem"] = _subsystemName;
    }
}
