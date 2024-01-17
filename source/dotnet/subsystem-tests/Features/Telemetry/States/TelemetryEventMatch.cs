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

#pragma warning disable SA1402 // File may only contain a single type
namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.Telemetry.States
{
    /// <summary>
    /// Marker interface for event match types.
    /// </summary>
    public interface ITelemetryEventMatch
    {
        public bool IsMatch(TelemetryQueryResult actual);
    }

    public class AppRequestMatch : ITelemetryEventMatch
    {
        /// <summary>
        /// Use if Name must match exactly.
        /// </summary>
        public string Name { get; set; }
            = string.Empty;

        public bool IsMatch(TelemetryQueryResult actual)
        {
            return actual.Name.Equals(Name);
        }
    }

    public class AppDependencyMatch : ITelemetryEventMatch
    {
        /// <summary>
        /// Use if Name must match exactly.
        /// </summary>
        public string Name { get; set; }
            = string.Empty;

        /// <summary>
        /// Use if Name is expected to contain value.
        /// </summary>
        public string NameContains { get; set; }
            = string.Empty;

        public string DependencyType { get; set; }
            = string.Empty;

        public bool IsMatch(TelemetryQueryResult actual)
        {
            if (!string.IsNullOrEmpty(NameContains))
            {
                // Compare using NameContains
                return actual.Name.Contains(NameContains)
                    && actual.DependencyType == DependencyType;
            }
            else
            {
                // Compare using Name
                return actual.Name == Name
                    && actual.DependencyType == DependencyType;
            }
        }
    }

    public class AppTraceMatch : ITelemetryEventMatch
    {
        public string EventName { get; set; }
            = string.Empty;

        public string MessageContains { get; set; }
            = string.Empty;

        public bool IsMatch(TelemetryQueryResult actual)
        {
            return (actual.EventName ?? string.Empty) == EventName
                && actual.Message.Contains(MessageContains);
        }
    }

    public class AppExceptionMatch : ITelemetryEventMatch
    {
        public string EventName { get; set; }
            = string.Empty;

        public string OuterType { get; set; }
            = string.Empty;

        public string OuterMessage { get; set; }
            = string.Empty;

        public bool IsMatch(TelemetryQueryResult actual)
        {
            return (actual.EventName ?? string.Empty) == EventName
                && actual.OuterType == OuterType
                && actual.OuterMessage == OuterMessage;
        }
    }
}
#pragma warning restore SA1402 // File may only contain a single type
