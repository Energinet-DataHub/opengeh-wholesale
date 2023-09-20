﻿// Copyright 2020 Energinet DataHub A/S
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

namespace Energinet.DataHub.Wholesale.WebApi.HealthChecks;

public static class HealthCheckNames
{
    public const string WholesaleInboxEventsQueue = "WholesaleInboxHealthCheck";
    public const string EdiInboxEventsQueue = "EdiInboxEventsQueueHealthCheck";
    public const string IntegrationEventsTopic = "IntegrationEventsTopicHealthCheck";
    public const string DatabricksSqlStatementsApi = "DatabricksSqlStatementsApiHealthCheck";
    public const string DatabricksJobsApi = "DatabricksJobsApiHealthCheck";
    public const string OutboxSenderTrigger = "OutboxSenderTrigger";
    public const string SqlDatabaseContext = "SqlDatabaseContextHealthCheck";
    public const string RegisterCompletedBatchesTrigger = nameof(RegisterCompletedBatchesTrigger);
    public const string UpdateBatchExecutionStateTrigger = nameof(UpdateBatchExecutionStateTrigger);
    public const string StartCalculationTrigger = nameof(StartCalculationTrigger);
    public const string DataLake = "DataLakeHealthCheck";
}
