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

using Microsoft.Azure.Databricks.Client;

namespace Energinet.DataHub.Wholesale.SubsystemTests.Fixtures;

/// <summary>
/// Convenient methods for using the <see cref="DatabricksClient"/>.
///
/// If we need to make several calls using the <see cref="DatabricksClient"/>,
/// we should not use these extensions but instead create and maintain an
/// instance of the client in the class where we use it.
///
/// Client documentation: https://github.com/Azure/azure-databricks-client
/// </summary>
public static class DatabricksClientExtensions
{
    /// <summary>
    /// Retrieve calculator job id from databricks.
    /// </summary>
    public static async Task<long> GetCalculatorJobIdAsync(this DatabricksClient databricksClient)
    {
        var job = await databricksClient.Jobs
            .ListPageable(name: "CalculatorJob")
            .SingleAsync();

        return job.JobId;
    }
}
