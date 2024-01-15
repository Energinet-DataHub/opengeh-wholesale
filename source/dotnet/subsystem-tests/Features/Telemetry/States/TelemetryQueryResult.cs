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
    public class TelemetryQueryResult
    {
        public string TimeGenerated { get; set; }
            = string.Empty;

        public string OperationId { get; set; }
            = string.Empty;

        public string ParentId { get; set; }
            = string.Empty;

        public string Id { get; set; }
            = string.Empty;

        public string Type { get; set; }
            = string.Empty;

        public string Name { get; set; }
            = string.Empty;

        public string DependencyType { get; set; }
            = string.Empty;

        public string EventName { get; set; }
            = string.Empty;

        public string Message { get; set; }
            = string.Empty;

        public string Url { get; set; }
            = string.Empty;

        public string Properties { get; set; }
            = string.Empty;
    }
}
