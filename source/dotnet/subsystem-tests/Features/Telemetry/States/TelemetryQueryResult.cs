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

namespace Energinet.DataHub.Wholesale.SubsystemTests.Features.Telemetry.States
{
    /// <summary>
    /// On each property we have documented which event type the column is supported by.
    /// </summary>
    public class TelemetryQueryResult
    {
        /// <summary>
        /// All types.
        /// </summary>
        public string TimeGenerated { get; set; }
            = string.Empty;

        /// <summary>
        /// All types.
        /// </summary>
        public string OperationId { get; set; }
            = string.Empty;

        /// <summary>
        /// All types.
        /// </summary>
        public string ParentId { get; set; }
            = string.Empty;

        /// <summary>
        /// All types.
        /// </summary>
        public string Id { get; set; }
            = string.Empty;

        /// <summary>
        /// All types.
        /// </summary>
        public string Type { get; set; }
            = string.Empty;

        /// <summary>
        /// All types.
        /// </summary>
        public string AppVersion { get; set; }
            = string.Empty;

        /// <summary>
        /// All types.
        /// Custom property set as global property.
        /// </summary>
        public string Subsystem { get; set; }
            = string.Empty;

        /// <summary>
        /// AppRequests, AppDependencies
        /// </summary>
        public string Name { get; set; }
            = string.Empty;

        /// <summary>
        /// AppDependencies
        /// </summary>
        public string DependencyType { get; set; }
            = string.Empty;

        /// <summary>
        /// Retrieved from a custom property, so it's not always available.
        /// AppTraces, AppExceptions
        /// </summary>
        public string EventName { get; set; }
            = string.Empty;

        /// <summary>
        /// AppTraces
        /// </summary>
        public string Message { get; set; }
            = string.Empty;

        /// <summary>
        /// AppRequests
        /// </summary>
        public string Url { get; set; }
            = string.Empty;

        /// <summary>
        /// AppExceptions
        /// </summary>
        public string OuterType { get; set; }
            = string.Empty;

        /// <summary>
        /// AppExceptions
        /// </summary>
        public string OuterMessage { get; set; }
            = string.Empty;

        /// <summary>
        /// All types.
        /// </summary>
        public string Properties { get; set; }
            = string.Empty;
    }
}
